namespace Auth.Contracts
{
    public class Role
    {
        public required int Id { get; set; }
        public required string RoleName { get; set; }

        public ICollection<UserRole>? UserRoles { get; set; }
    }
}
