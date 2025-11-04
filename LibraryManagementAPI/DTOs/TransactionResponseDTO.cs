using LibraryManagementAPI.Models;

namespace LibraryManagementAPI.DTOs
{
    public class TransactionResponseDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public string? Date { get; set; }
        public TransactionType Type { get; set; }
        public string? DueDate { get; set; }

        public string? UserName { get; set; }
        public string? BookTitle { get; set; }
    }
}
