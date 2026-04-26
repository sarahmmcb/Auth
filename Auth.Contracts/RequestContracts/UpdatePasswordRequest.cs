
namespace Auth.Contracts.RequestContracts
{
    public class UpdatePasswordRequest
    {
        public required int UserId { get; set; }
        public required string Password { get; set; }
    }
}
