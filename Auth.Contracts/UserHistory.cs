namespace Auth.Contracts
{
    public class UserHistory
    {
        public int Uniqueifier { get; set; }
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        public DateTimeOffset UpdateDate { get; set; }
        public int UpdateUserId { get; set; }
        public bool IsDeleted { get; set; }

    }
}
