using LibraryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementAPI.Services
{
    public class HoldService
    {
        private readonly LibraryContext _context;

        public HoldService(LibraryContext context)
        {
            _context = context;
        }

        public async Task<Transaction?> AssignNextHold(Book book)
        {
            var nextHold = await _context.Holds
                .Where(h => h.BookId == book.Id && h.IsActive && h.EndDate > DateTime.UtcNow)
                .OrderBy(h => h.StartDate)
                .FirstOrDefaultAsync();

            if (nextHold == null)
                return null;

           
            nextHold.IsActive = false;

            
            book.Status = BookStatus.CheckedOut;

            // Create a new checkout transaction for that hold
            var holdTransaction = new Transaction
            {
                UserId = nextHold.UserId,
                BookId = book.Id,
                Date = DateTime.UtcNow,
                Type = TransactionType.CheckOut,
                DueDate = DateTime.UtcNow.AddDays(14),
                User = nextHold.User,
                Book = nextHold.Book
            };

            _context.Transactions.Add(holdTransaction);
            await _context.SaveChangesAsync();

            return holdTransaction;
        }
    }
}
