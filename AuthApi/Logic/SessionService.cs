using Microsoft.EntityFrameworkCore;
using AuthApi.Data;
using AuthApi.Security;
using Auth.Contracts;
using AuthApi.Helpers;
using System.Security.Cryptography;

namespace AuthApi.Logic
{
    public interface ISessionService
    {
        Task<(string, string)> Login(string userName, string password);

        Task<(string, string)> RefreshAccessToken(int userId, string refreshToken);
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
                throw new ApplicationException("Username or Password is incorrect");
            }

            var token = TokenManager.GenerateToken(user) ?? throw new ApplicationException("An error ocurred");

            var refreshTokenEntity = TokenManager.GenerateRefreshToken(user);

            _userDbContext.RefreshToken.Add(refreshTokenEntity);
            await _userDbContext.SaveChangesAsync();

            return (token, refreshTokenEntity.Token);
        }

        public async Task<(string, string)> RefreshAccessToken(int userId, string refreshToken)
        {
            var storedToken = await _userDbContext.RefreshToken
                .Where(u => u.UserId == userId && string.Equals(u.Token, refreshToken)).FirstOrDefaultAsync();

            var user = await _userDbContext.User
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();

            if (storedToken is null ||
                storedToken.Revoked ||
                storedToken.Expires < DateTimeOffset.UtcNow)
            {
                throw new ApplicationException("Please log in again");
            }

            storedToken.Revoked = true;

            var token = TokenManager.GenerateToken(user) ?? throw new ApplicationException("An error ocurred");

            var refreshTokenEntity = TokenManager.GenerateRefreshToken(user);

            _userDbContext.RefreshToken.Add(refreshTokenEntity);
            await _userDbContext.SaveChangesAsync();

            return (token, refreshTokenEntity.Token);
        }
    }

}