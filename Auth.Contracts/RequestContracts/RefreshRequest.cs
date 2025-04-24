
namespace Auth.Contracts.RequestContracts
{
    public class RefreshRequest
    {
        public required int UserId { get; set; }

        public required string RefreshToken { get; set; }
    }
}
