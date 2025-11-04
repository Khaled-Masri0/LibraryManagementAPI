using LibraryManagementAPI.Models;

namespace LibraryManagementAPI.DTOs
{
    public class TransactionSummaryDTO
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string? BookTitle { get; set; }
        public string? BookAuthor { get; set; }
        public string? Date { get; set; }
        public TransactionType Type { get; set; }
        public string? DueDate { get; set; }
    }
}
