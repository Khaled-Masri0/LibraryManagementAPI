using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Migrations;
using LibraryManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace LibraryManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HoldsController : ControllerBase
    {
        private readonly LibraryContext _context;

        public HoldsController(LibraryContext context)
        {
            _context = context;
        }

        // GET: api/holds
        [SwaggerOperation(
        Summary = "Returns all holds entities",
        Description = "Returns all holds entities in the database",
        OperationId = "GetHolds")]
        [SwaggerResponse(200, "Holds successfully returned", typeof(HoldSummaryDTO))]
        [HttpGet]
        public async Task<IActionResult> GetHolds()
        {
            var holds = await _context.Holds
                .Include(h => h.Book)
                .Select(h => new HoldSummaryDTO
            {
                Id = h.Id,
                BookId = h.BookId,
                BookTitle = h.Book.Title,
                BookAuthor = h.Book.Author,
                StartDate = h.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                EndDate = h.EndDate.ToString("yyyy-MM-dd HH:mm:ss"),
                IsActive = h.IsActive

            }).ToListAsync();
            return Ok(holds);

        }

        // GET: api/holds/id
        [SwaggerOperation(
       Summary = "Returns a specific hold entity",
       Description = "Returns a specific hold entity with given ID",
       OperationId = "GetHold")]
        [SwaggerResponse(200, "Hold successfully returned", typeof(HoldSummaryDTO))]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetHold(int id)
        {
            var holdDto = await _context.Holds
                .Where(h => h.Id == id)
                .Select(h => new HoldSummaryDTO
                {
                    Id = h.Id,
                    BookId = h.BookId,
                    BookTitle = h.Book.Title,
                    BookAuthor = h.Book.Author,
                    StartDate = h.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    EndDate = h.EndDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    IsActive = h.IsActive
                })
                .FirstOrDefaultAsync();

            if (holdDto == null)
                return NotFound(new { Message = $"Hold with ID {id} not found" });

            return Ok(holdDto);
        }


        // POST: api/holds
        [SwaggerOperation(
         Summary = "Creates a new hold for a user",
         Description = "Adds a hold entry for a specific user and book. Book must not be available.",
         OperationId = "CreateHold")]
        [SwaggerResponse(201, "Hold successfully created", typeof(HoldSummaryDTO))]
        [SwaggerResponse(400, "Invalid request")]
        [SwaggerResponse(404, "User or book not found")]
        [HttpPost]
        public async Task<IActionResult> CreateHold(CreateHoldDTO dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return NotFound(new { Message = $"User with ID {dto.UserId} not found." });

            var book = await _context.Books.FindAsync(dto.BookId);
            if (book == null)
                return NotFound(new { Message = $"Book with ID {dto.BookId} not found." });

            if (book.Status == BookStatus.Available)
                return BadRequest(new { Message = "Cannot place hold, book is currently available." });

            if (book.Status == BookStatus.Removed)
                return BadRequest(new { Message = "Cannot place hold, book was removed from the library." });

            bool alreadyHasHold = await _context.Holds
            .AnyAsync(h => h.UserId == dto.UserId && h.BookId == dto.BookId && h.IsActive);

            if (alreadyHasHold)
                return BadRequest(new { Message = $"User with ID {dto.UserId} already have an active hold for this book." });

            var hold = new Hold
            {
                UserId = dto.UserId,
                BookId = dto.BookId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(14)
            };

            _context.Holds.Add(hold);
            await _context.SaveChangesAsync();

            var holdDto = new HoldSummaryDTO
            {
                Id = hold.Id,
                BookId = hold.BookId,
                BookTitle = book.Title,
                BookAuthor = book.Author,
                StartDate = hold.StartDate.ToString("yyyy-MM-dd"),
                EndDate = hold.EndDate.ToString("yyyy-MM-dd"),
                IsActive = true
            };

            return CreatedAtAction(nameof(GetHold), new { id = hold.Id }, holdDto);


        }
        [SwaggerOperation(
       Summary = "Deletes a specific hold entity",
       Description = "Marks a specific hold as NOT active, IsActive flag = false",
       OperationId = "DeleteHold")]
        [SwaggerResponse(204, "Hold successfully deleted")]
        [SwaggerResponse(400, "Hold could not be deleted, hold is already NOT active")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHold(int id)
        {
            var hold = await _context.Holds.FindAsync(id);
            if (hold == null)
                return NotFound(new { Message = $"Hold with ID {id} not found" });

            if (!hold.IsActive)
                return BadRequest(new { Message = $"Hold with ID {id} is already not active" });

            hold.IsActive = false;
            await _context.SaveChangesAsync();


            return NoContent();
        }
    }
}