using System;
using System.ComponentModel.DataAnnotations;

namespace Auth.Contracts.RequestContracts
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required")]
        public required string UserName { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }
    }
}
