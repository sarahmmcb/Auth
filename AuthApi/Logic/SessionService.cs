using Microsoft.EntityFrameworkCore;
using AuthApi.Data;
using AuthApi.Security;
using System.Security.Claims;

namespace AuthApi.Logic
{
    public interface ISessionService
    {
        Task<(string, string)> Login(string userName, string password);
        Task<(string, string)> RefreshAccessToken(string userName, string refreshToken);
        Task Logout(IEnumerable<Claim> claims);
    }

    public class SessionService(UserDbContext _userDbContext) : ISessionService
    {
        public async Task<(string, string)> Login(string userName, string password)
        {
            var user = await _userDbContext.User
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => string.Equals(u.UserName, userName))
                .FirstOrDefaultAsync();

            if (user is null || !PasswordManager.VerifyPassword(password, user.Password))
            {
                throw new UnauthorizedAccessException("Username or Password is incorrect");
            }

            var token = await TokenManager.GenerateToken(user, _userDbContext) ?? throw new ApplicationException("An error ocurred");

            var refreshTokenEntity = TokenManager.GenerateRefreshToken(user);

            _userDbContext.RefreshToken.Add(refreshTokenEntity);
            await _userDbContext.SaveChangesAsync();

            return (token, refreshTokenEntity.Token);
        }

        public async Task<(string, string)> RefreshAccessToken(string userName, string refreshToken)
        {
            var storedToken = await _userDbContext.RefreshToken
                .Where(u => string.Equals(u.UserName, userName) && string.Equals(u.Token, refreshToken)).FirstOrDefaultAsync();

            var user = await _userDbContext.User
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.UserName == userName)
                .FirstOrDefaultAsync();

            if (storedToken is null ||
                storedToken.Revoked ||
                storedToken.Expires < DateTimeOffset.UtcNow)
            {
                throw new UnauthorizedAccessException("Please log in again");
            }

            storedToken.Revoked = true;

            var token = await TokenManager.GenerateToken(user, _userDbContext) ?? throw new ApplicationException("An error ocurred");

            var refreshTokenEntity = TokenManager.GenerateRefreshToken(user);

            _userDbContext.RefreshToken.Add(refreshTokenEntity);
            await _userDbContext.SaveChangesAsync();

            return (token, refreshTokenEntity.Token);
        }

        public async Task Logout(IEnumerable<Claim> claims)
        {
            var userName = claims.Where(c => c.Type == ClaimTypes.Name).FirstOrDefault() ?? throw new ApplicationException("Username was null");

            var refreshTokenRows = _userDbContext.RefreshToken.Where(t => t.UserName.Equals(userName.Value) && !t.Revoked);

            foreach (var row in refreshTokenRows)
            {
                row.Revoked = true;
            }

            await _userDbContext.SaveChangesAsync();
        }
    }
}