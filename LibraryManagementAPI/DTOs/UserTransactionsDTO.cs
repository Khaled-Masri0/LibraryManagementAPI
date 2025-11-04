using LibraryManagementAPI.Models;

namespace LibraryManagementAPI.DTOs
{
    public class UserTransactionsDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public UserRole Role { get; set; }
        public List<TransactionSummaryDTO>? Transactions { get; set; }
    }
}
