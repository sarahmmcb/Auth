namespace Auth.Contracts
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public required string Token { get; set; }
        public required int UserId { get; set; }
        public DateTimeOffset Expires { get; set; }
        public bool Revoked { get; set; }
    }

}
