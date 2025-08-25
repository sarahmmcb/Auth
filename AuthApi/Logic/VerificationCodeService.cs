using AuthApi.Data;
using AuthApi.Email;
using AuthApi.Models;
using Auth.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthApi.Logic
{
    public interface IVerificationCodeService
    {
        Task<bool> SendVerificationCodeByEmail(string email);
    }
    public class VerificationCodeService : IVerificationCodeService
    {
        private readonly SmtpSettings _smtpSettings;
        private EmailManager _emailManager;
        private readonly UserDbContext _userDbContext;

        public VerificationCodeService(IOptions<SmtpSettings> smtpSettings, EmailManager emailManager, UserDbContext context)
        {
            _smtpSettings = smtpSettings.Value;
            _emailManager = emailManager;
            _userDbContext = context;
        }

        public async Task<bool> SendVerificationCodeByEmail(string email)
        {
            var user = await _userDbContext.User.FirstOrDefaultAsync(u => string.Equals(u.UserName, email));

            if (user is null)
            {
                return false;
            }

            var code = GenerateVerificationCode();

            var verificationCode = new VerificationCode
            {
                Code = code,
                UserId = user.Id,
                ExpirationDate = DateTimeOffset.UtcNow.AddMinutes(5)
            };

            await _userDbContext.AddAsync(verificationCode);

            var result = await _emailManager.SendEmail(
                                _smtpSettings.FromEmail,
                                _smtpSettings.FromEmailDisplayName,
                                [email],
                                "CE Tracker Verification Code",
                                $"<p>Here is the verification code you requested: {code}</p>" +
                                $"<p>If you did not request this code, please ignore this message.</p>");

            return result;
        }

        public static int GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(10000, 1000000);
        }
    }
}
