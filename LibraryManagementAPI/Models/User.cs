using System.ComponentModel.DataAnnotations;

namespace LibraryManagementAPI.Models
{

    public enum UserRole
    {
        Member = 0,
        Clerk = 1
    }


    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage= "Name cannot exceed 100 characters")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string? Address { get; set; }
        public UserRole Role { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
        public ICollection<Hold>? Holds { get; set; }

    }
}
