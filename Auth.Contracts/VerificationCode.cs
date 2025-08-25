namespace Auth.Contracts
{
    public class VerificationCode
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public int UserId { get; set; }
        public DateTimeOffset ExpirationDate { get; set; }

        internal User user { get; set; }
    }
}
