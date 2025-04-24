
namespace AuthApi.Security
{
    public static class PasswordManager
    {
        public static string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

        public static bool VerifyPassword(string entered, string stored) {
            
            return BCrypt.Net.BCrypt.Verify(entered, stored);

        } 
    }
}
