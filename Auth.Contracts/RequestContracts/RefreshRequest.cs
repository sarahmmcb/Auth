
namespace Auth.Contracts.RequestContracts
{
    public class RefreshRequest
    {
        public required string UserName { get; set; }

        public required string RefreshToken { get; set; }
    }
}
