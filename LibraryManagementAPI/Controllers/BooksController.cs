using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace LibraryManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly LibraryContext _context;

        public BooksController(LibraryContext context)
        {
            _context = context;
        }

        // GET: api/books
        [SwaggerOperation(
        Summary = "Returns all books entities",
        Description = "Returns all books entities in the database",
        OperationId = "GetBooks")]
        [SwaggerResponse(200, "Books successfully returned")]
        [HttpGet]
        public async Task<IActionResult> GetBooks()
        {
            var books = await _context.Books
                .Include(b => b.Transactions)
                    .ThenInclude(t => t.User)
                .Select(b => new BookResponseDTO
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Status = b.Status,
                    // Find the user who has the book checked out (if any)
                    UserId = b.Transactions
                        .Where(t => t.Type == TransactionType.CheckOut)
                        .OrderByDescending(t => t.Id)
                        .Select(t => t.UserId)
                        .FirstOrDefault(),

                    UserName = b.Transactions
                        .Where(t => t.Type == TransactionType.CheckOut)
                        .OrderByDescending(t => t.Id)
                        .Select(t => t.User.Name)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(books);
        }


        // GET: api/books/{id}
        [SwaggerOperation(
          Summary = "Returns a specific book entity",
          Description = "Returns a specific book entity with given ID",
          OperationId = "GetBook")]
        [SwaggerResponse(200, "Hold successfully returned")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound(new { Message = $"Book with ID {id} not found." });

            return Ok(book);
        }

        // POST: api/books
        [SwaggerOperation(
       Summary = "Adds a new book",
       Description = "Creates a book entity, Author and title must be written",
       OperationId = "CreateBook")]
        [SwaggerResponse(201, "Book successfully added", typeof(CreateBookDTO))]
        [HttpPost]
        public async Task<IActionResult> CreateBook(CreateBookDTO dto)
        {
          
            var exists = await _context.Books .AnyAsync(b => b.Title == dto.Title && b.Author == dto.Author);

            if (exists)
                return Conflict(new { Message = "This book already exists in the library." });

            var book = new Book
            {
                Title = dto.Title,
                Author = dto.Author,
            };
         
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
        }

        // PUT: api/books/{id}
        [SwaggerOperation(
        Summary = "Updates a book's entity",
        Description = "Updates a book's author and title",
        OperationId = "UpdateBook")]
        [SwaggerResponse(200, "Book successfully updated", typeof(CreateBookDTO))]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, CreateBookDTO updatedBook)
        {

            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound(new { Message = $"Book with ID {id} not found." });

            // don’t allow updates if the book is removed
            if (book.Status == BookStatus.Removed)
                return BadRequest(new { Message = "Cannot update a removed book." });

            
            book.Title = updatedBook.Title;
            book.Author = updatedBook.Author;

      
            await _context.SaveChangesAsync();
            return Ok(book);
        }

        // DELETE: api/books/{id}
        [SwaggerOperation(
        Summary = "Deletes a specific book entity",
        Description = "Marks a specific book as REMOVED, BookStatus.Removed ",
        OperationId = "DeleteBook")]
        [SwaggerResponse(204, "Book successfully deleted")]
        [SwaggerResponse(400, "Book could not be deleted, book is already REMOVED")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id) // instead of actual delete, mark as "Removed"
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound(new { Message = $"Book with ID {id} not found." });
           

            if (book.Status == BookStatus.Removed)
                return BadRequest(new { Message = "Book is already removed." });

            book.Status = BookStatus.Removed;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
