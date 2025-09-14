
namespace Auth.Contracts.RequestContracts
{
    public class UpdatePasswordRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
