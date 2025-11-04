using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Models;
using LibraryManagementAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;


namespace LibraryManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly LibraryContext _context;
        private readonly HoldService _holdService;

        public TransactionsController(LibraryContext context, HoldService holdService)
        {
            _context = context;
            _holdService = holdService;
        }

        // GET: api/transactions
        [SwaggerOperation(
        Summary = "Returns all transaction entities",
        Description = "Returns all transaction entities in the database",
        OperationId = "GetTransactions")]
        [SwaggerResponse(200, "Transactions successfully returned", typeof(TransactionResponseDTO))]
        [HttpGet]
        public async Task<IActionResult> GetTransactions()
        {
            var transactions = await _context.Transactions
                .Include(t => t.User)
                .Include(t => t.Book)
                .Select(t => new TransactionResponseDTO
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    BookId = t.BookId,
                    Date = t.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                    Type = t.Type,
                    DueDate = t.DueDate.HasValue ? t.DueDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    UserName = t.User.Name,
                    BookTitle = t.Book.Title
                })
                .ToListAsync();

            return Ok(transactions);
        }


        // GET: api/transactions/id
        [SwaggerOperation(
        Summary = "Returns a specific transaction",
        Description = "Returns a specific transaction entity with given ID",
        OperationId = "GetTransaction")]
        [SwaggerResponse(200, "Transaction successfully returned", typeof(TransactionResponseDTO))]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaction(int id)
        {
            var transaction = await _context.Books.FindAsync(id);
            if (transaction == null)
                return NotFound(new { Message = $"Transaction with ID {id} not found." });

            return Ok(transaction);

        }

        // POST: api/transactions
        [SwaggerOperation(
        Summary = "Creates a new transaction entity",
        Description = "Creates a new transaction entity for a specific user and book",
        OperationId = "CreateHold")]
        [SwaggerResponse(201, "Transaction successfully created", typeof(TransactionResponseDTO))]
        [SwaggerResponse(400, "Invalid request")]
        [SwaggerResponse(404, "User or book not found")]
        [HttpPost]
        public async Task<IActionResult> CreateTransaction(CreateTransactionDTO dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return NotFound(new { Message = $"User with ID {dto.UserId} not found." });

            var book = await _context.Books.FindAsync(dto.BookId);
            if (book == null)
                return NotFound(new { Message = $"Book with ID {dto.BookId} not found." });

           
            var transaction = new Transaction
            {
                UserId = dto.UserId,
                BookId = dto.BookId,
                Date = DateTime.UtcNow,
                Type = dto.Type,
                DueDate = null
            };

            // Apply business rules depending on type
            switch (dto.Type)
            {
                case TransactionType.CheckOut:
                    if (book.Status != BookStatus.Available)
                        return BadRequest(new { Message = "Book is not available for checkout." });

                    book.Status = BookStatus.CheckedOut;
                    transaction.DueDate = DateTime.UtcNow.AddDays(14); 
                    break;

                case TransactionType.Return:
                    if (book.Status != BookStatus.CheckedOut)
                        return BadRequest(new { Message = "Book is not currently checked out." });

                    // add the return transaction
                    var returnTransaction = new Transaction
                    {
                        UserId = user.Id,
                        BookId = book.Id,
                        Date = DateTime.UtcNow,
                        Type = TransactionType.Return
                    };

                    _context.Transactions.Add(returnTransaction);
                    book.Status = BookStatus.Available;
                    await _context.SaveChangesAsync();

                    // check for next hold
                    var holdTransaction = await _holdService.AssignNextHold(book);

                    // Build response list
                    var responseList = new List<TransactionResponseDTO>();

                    responseList.Add(new TransactionResponseDTO
                    {
                        Id = returnTransaction.Id,
                        UserId = returnTransaction.UserId,
                        BookId = returnTransaction.BookId,
                        Date = returnTransaction.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                        Type = returnTransaction.Type,
                        UserName = user.Name,
                        BookTitle = book.Title
                    });

                    if (holdTransaction != null)
                    {
                        responseList.Add(new TransactionResponseDTO
                        {
                            Id = holdTransaction.Id,
                            UserId = holdTransaction.UserId,
                            BookId = holdTransaction.BookId,
                            Date = holdTransaction.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                            Type = holdTransaction.Type,
                            DueDate = holdTransaction.DueDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                            UserName = holdTransaction.User?.Name,  
                            BookTitle = holdTransaction.Book?.Title
                        });
                    }

                    return Ok(responseList);


                case TransactionType.Renewal:
                    if (book.Status != BookStatus.CheckedOut)
                        return BadRequest(new { Message = "Book is not checked out, so it cannot be renewed." });

                    transaction.DueDate = DateTime.UtcNow.AddDays(14); // extend due date
                    break;

                default:
                    return BadRequest(new { Message = "Invalid transaction type." });
            }

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            var responseDto = new TransactionResponseDTO
            {
                Id = transaction.Id,
                UserId = transaction.UserId,
                BookId = transaction.BookId,
                Date = transaction.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                Type = transaction.Type,
                DueDate = transaction.DueDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                UserName = user.Name,  
                BookTitle = book.Title  
            };

            return CreatedAtAction(nameof(GetTransactions), new { id = transaction.Id }, responseDto);
        }

        
    }
}
