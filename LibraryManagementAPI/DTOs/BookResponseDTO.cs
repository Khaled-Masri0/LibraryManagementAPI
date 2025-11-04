using LibraryManagementAPI.Models;

namespace LibraryManagementAPI.DTOs
{
    public class BookResponseDTO
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public BookStatus Status { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
    }
}
