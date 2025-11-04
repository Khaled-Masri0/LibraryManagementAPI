using System.ComponentModel.DataAnnotations;

namespace LibraryManagementAPI.DTOs
{
    public class CreateBookDTO
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Author is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 chracters")]
        public string? Author { get; set; }
    }
}
