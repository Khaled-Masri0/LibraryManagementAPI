using System.ComponentModel.DataAnnotations;

namespace LibraryManagementAPI.DTOs
{
    public class CreateHoldDTO
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public int BookId { get; set; }
    }
}
