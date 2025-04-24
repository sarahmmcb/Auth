using System.ComponentModel.DataAnnotations;

namespace Auth.Contracts.RequestContracts
{
    public class UpdateUserRequest
    {
        public int? UserId { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password is Required")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Roles are Required")]

        public required int[] RoleIds { get; set; }
    }
}
