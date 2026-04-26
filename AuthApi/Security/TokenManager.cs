using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auth.Contracts;
using AuthApi.Helpers;
using Microsoft.IdentityModel.Tokens;
using NuGet.Packaging;

namespace AuthApi.Security
{
    public static class TokenManager
    {
        public static string GenerateToken(User user, int expMin = 1440)
        {
            var config = ConfigurationHelper.Section("JwtConfig");
            var secret = config.GetValue<string>("Secret");
            var issuer = config.GetValue<string>("ValidIssuer");
            var audiences = config.GetSection("ValidAudiences").Get<List<string>>();

            if (secret is null || issuer is null || audiences is null)
            {
                throw new ApplicationException("Jwt is not set in configuration");
            }

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.UserName)
                }),
                Expires = DateTime.UtcNow.AddMinutes(expMin),
                Issuer = issuer,
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature)
            };

            tokenDescriptor.Audiences.AddRange(audiences);

            var claimsToAdd = new List<Claim>();

            var userRoles = user.Roles;

            foreach (var userRole in userRoles)
            {
                if (userRole.RoleName is null)
                    continue;

                var roleName = userRole.RoleName;
                claimsToAdd.Add(new Claim(ClaimTypes.Role, roleName));
            }

            tokenDescriptor.Subject.AddClaims(claimsToAdd);

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(securityToken);

            return token;
        }

        public static RefreshToken GenerateRefreshToken(User user)
        {
            var refreshToken = Guid.NewGuid().ToString();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserName = user.UserName,
                Expires = DateTimeOffset.UtcNow.AddSeconds(int.Parse(ConfigurationHelper.Setting("RefreshTokenExpirySeconds"))),
                Revoked = false
            };

            return refreshTokenEntity;
        }
    }
}
