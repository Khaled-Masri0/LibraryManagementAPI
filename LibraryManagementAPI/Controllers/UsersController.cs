using LibraryManagementAPI.DTOs;
using LibraryManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace LibraryManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly LibraryContext _context;

        public UsersController(LibraryContext context)
        {
            _context = context;
        }

        // GET: api/users
        [SwaggerOperation(
        Summary = "Returns all user entities",
        Description = "Returns all user entities in the database",
        OperationId = "GetUsers")]
        [SwaggerResponse(200, "Users successfully returned", typeof(UserResponseDTO))]
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new UserResponseDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    Phone = u.Phone,
                    Address = u.Address,
                    Role = u.Role
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/users/id
        [SwaggerOperation(
       Summary = "Returns a specific user",
       Description = "Returns a specific user entity with given ID",
       OperationId = "GetUser")]
        [SwaggerResponse(200, "User successfully returned", typeof(UserResponseDTO))]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserResponseDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    Phone = u.Phone,
                    Address = u.Address,
                    Role = u.Role
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // GET: api/users/id/transactions
        [SwaggerOperation(
       Summary = "Returns all transactions of a specific user",
       Description = "Returns all transactions of a specific user with given ID",
       OperationId = "GetUserTransactions")]
        [SwaggerResponse(200, "User's transactions successfully returned", typeof(TransactionSummaryDTO))]
        [HttpGet("{id}/transactions")]
        public async Task<IActionResult> GetUserTransactions(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { Message = $"User with ID {id} not found." });

            var transactions = await _context.Transactions
                .Where(t => t.UserId == id)
                .Include(t => t.Book)
                .Select(t => new TransactionSummaryDTO
                {
                    Id = t.Id,
                    BookId = t.BookId,
                    BookTitle = t.Book.Title,
                    BookAuthor = t.Book.Author,
                    Date = t.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                    Type = t.Type,
                    DueDate = t.DueDate.HasValue ? t.DueDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : null

                })
                .ToListAsync();

            return Ok(new
            {
                UserId = user.Id,
                UserName = user.Name,
                Transactions = transactions
            });
        }

        // GET: api/users/holds
        [SwaggerOperation(
        Summary = "Returns all holds  of a specific user",
        Description = "Returns all holds of a specific user with given ID",
        OperationId = "GetUserHolds")]
        [SwaggerResponse(200, "User's holds successfully returned", typeof(HoldSummaryDTO))]
        [HttpGet("{id})/holds")]
        public async Task<IActionResult> GetUserHolds(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { Message = $"User with ID {id} not found." });

            var holds = await _context.Holds
               .Where(h => h.UserId == id)
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
            return Ok(new
            {
                UserId = user.Id,
                UserName = user.Name,
                Holds = holds
            });

        }

        // POST: api/users
        [SwaggerOperation(
        Summary = "Creates a new user",
        Description = "Creates a new user entity",
        OperationId = "CreateUser")]
        [SwaggerResponse(201, "User successfully created", typeof(UserResponseDTO))]
        [SwaggerResponse(400, "Invalid request")]
        [SwaggerResponse(409, "Phone number already exists")]
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var exists = await _context.Users.AnyAsync(u => u.Phone == dto.Phone);
            if (exists)
            {
                return Conflict(new { Message = "A user with this phone number already exists." });
            }

            var user = new User
            {
                Name = dto.Name,
                Phone = dto.Phone,
                Address = dto.Address,
                Role = dto.Role,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var responseDto = new UserResponseDTO
            {
                Id = user.Id,
                Name = user.Name,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, responseDto);
        }

        // PUT: api/users/id
        [SwaggerOperation(
        Summary = "Updates a user's entity",
        Description = "Updates a specifc user's details with given ID",
        OperationId = "UpdateUser")]
        [SwaggerResponse(200, "User successfully updated", typeof(UserResponseDTO))]
        [SwaggerResponse(400, "Invalid Request")]
        [SwaggerResponse(404, "User could not be found")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, CreateUserDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { Message = $"User with ID {id} not found." });

            user.Name = dto.Name;
            user.Phone = dto.Phone;
            user.Address = dto.Address;
            user.Role = dto.Role;

            await _context.SaveChangesAsync();

            var responseDto = new UserResponseDTO
            {
                Id = user.Id,
                Name = user.Name,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role
            };

            return Ok(responseDto);
        }
        [SwaggerOperation(
        Summary = "Deletes a user",
        Description = "Deletes a specific user with given ID",
        OperationId = "DeleteUser")]
        [SwaggerResponse(204, "User successfully deleted")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { Message = $"User with ID {id} not found." });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

       
    }
}