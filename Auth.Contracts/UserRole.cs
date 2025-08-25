
namespace Auth.Contracts
{
    public class UserRole
    {
        public required int UserId { get; set; }
        public required int RoleId { get; set; }

        internal User User { get; set; }
        internal Role Role { get; set; }
    }
}
