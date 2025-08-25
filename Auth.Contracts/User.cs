
namespace Auth.Contracts
{
    public class User
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public DateTimeOffset CreateDate { get; set; }

        internal ICollection<UserRole> UserRoles { get; set; }
        internal ICollection<VerificationCode> VerificationCodes { get; set; }
    }
}
