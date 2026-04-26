using System.ComponentModel.DataAnnotations;

namespace Auth.Contracts.RequestContracts
{
    public class CreateUserRequest
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }

        public AccountStatus AccountStatus { get; set; }
        public List<int>? Roles { get; set; }
    }
}
