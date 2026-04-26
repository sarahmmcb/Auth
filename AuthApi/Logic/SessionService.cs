using AuthApi.Security;
using System.Security.Claims;
using AuthApi.Logging;
using AuthDAL.DataAccess;
using Auth.Contracts;

namespace AuthApi.Logic
{
    public interface ISessionService
    {
        Task<(string, string)> Login(string userName, string password, CancellationToken token);
        Task<(string, string)> RefreshAccessToken(string userName, string refreshToken, CancellationToken token);
        Task Logout(IEnumerable<Claim> claims, CancellationToken token);
    }

    public class SessionService(IDataProvider _dataProvider, IUserService _userService) : ISessionService
    {
        public async Task<(string, string)> Login(string userName, string password, CancellationToken token)
        {
            var user = await _userService.GetUserByUserName(userName, token);

            if (user is null || !PasswordManager.VerifyPassword(password, user.Password))
            {
                AuthLogger.LogError<SessionService>($"Error logging in. User: {user} (if user is defined, then pw was incorrect)");
                throw new UnauthorizedAccessException("Username or Password is incorrect");
            }

            var authToken = TokenManager.GenerateToken(user) ?? throw new ApplicationException("An error ocurred");

            var refreshTokenEntity = TokenManager.GenerateRefreshToken(user);

            await _dataProvider.ExecuteSimpleProc("core.RefreshToken_I", new  // TODO: Bonus exercise: code an Object.Assign C# equivalent
            {
                refreshTokenEntity.Token,
                refreshTokenEntity.Expires,
                refreshTokenEntity.Revoked,
                refreshTokenEntity.UserName
            }, token);

            AuthLogger.LogInformation<SessionService>($"Returning token and refresh token for {userName}");
            return (authToken, refreshTokenEntity.Token);
        }

        public async Task<(string, string)> RefreshAccessToken(string userName, string refreshToken, CancellationToken token)
        {
            var storedRefreshToken = (await _dataProvider.LoadData<RefreshToken, dynamic>("core.RefreshToken_S", new
            {
                UserName = userName
            }, token)).Where(t => string.Equals(t.Token, refreshToken)).FirstOrDefault();

            if (storedRefreshToken is null ||
                storedRefreshToken.Revoked ||
                storedRefreshToken.Expires < DateTimeOffset.UtcNow)
            {
                AuthLogger.LogWarning<SessionService>($"Refresh token null, expired, or revoked for {userName}");
                throw new UnauthorizedAccessException("Session expired, please log in again");
            }

            var user = await _userService.GetUserByUserName(userName, token);

            if (user == null)
            {
                throw new ApplicationException("User not found");
            }

            await _dataProvider.ExecuteSimpleProc("core.RefreshToken_Revoke", new { storedRefreshToken.Id }, token);

            var authToken = TokenManager.GenerateToken(user) ?? throw new ApplicationException("An error ocurred");

            var refreshTokenEntity = TokenManager.GenerateRefreshToken(user);

            await _dataProvider.ExecuteSimpleProc("core.RefreshToken_I", new
            {
                refreshTokenEntity.Token,
                refreshTokenEntity.Expires,
                refreshTokenEntity.Revoked,
                refreshTokenEntity.UserName
            }, token);

            return (authToken, refreshTokenEntity.Token);
        }

        public async Task Logout(IEnumerable<Claim> claims, CancellationToken token)
        {
            var userName = claims.Where(c => c.Type == ClaimTypes.Name).FirstOrDefault();
            
            if (userName is null)
            {
                AuthLogger.LogError<SessionService>("Logout attempt failed: username was null");
                throw new ApplicationException("Username was null");
            }

            await _dataProvider.ExecuteSimpleProc("core.RefreshToken_RevokeAll_ByUserName", new
            {
                userName
            }, token);
        }
    }
}