using System.ComponentModel.DataAnnotations;

namespace LibraryManagementAPI.Models
{
    public enum BookStatus
    {
        Available = 0,
        CheckedOut = 1,
        Removed = 2
    }

    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage="Title cannot exceed 100 characters")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Author is required")]
        [StringLength(100, ErrorMessage ="Title cannot exceed 100 chracters")]
        public string? Author { get; set; }
        public BookStatus Status { get; set; } = BookStatus.Available;
        public ICollection<Transaction>? Transactions { get; set; }
        public ICollection<Hold>? Holds { get; set; }
    }

}
