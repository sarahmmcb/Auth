namespace Auth.Contracts
{
    public class VerificationCode
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int UserId { get; set; }
        public DateTimeOffset ExpirationDate { get; set; }
    }
}
