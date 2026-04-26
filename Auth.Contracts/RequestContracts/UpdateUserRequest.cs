
namespace Auth.Contracts.RequestContracts
{
    public class UpdateUserRequest
    {
        public int? UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public AccountStatus? AccountStatus { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
