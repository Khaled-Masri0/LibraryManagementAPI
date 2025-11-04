namespace LibraryManagementAPI.DTOs
{
    public class HoldSummaryDTO
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string? BookTitle { get; set; } 
        public string? BookAuthor { get; set; }
        public string? StartDate { get; set; } 
        public string? EndDate { get; set; } 
        public bool IsActive { get; set; }
    }
}
