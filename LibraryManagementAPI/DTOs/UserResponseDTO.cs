using LibraryManagementAPI.Models;

namespace LibraryManagementAPI.DTOs
{
    public class UserResponseDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public UserRole Role { get; set; }
    }
}
