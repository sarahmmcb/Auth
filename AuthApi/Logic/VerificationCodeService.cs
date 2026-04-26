using Auth.Contracts;
using AuthApi.Email;
using AuthApi.Models;
using AuthApi.Security;
using AuthDAL.DataAccess;
using Microsoft.Extensions.Options;

namespace AuthApi.Logic
{
    public interface IVerificationCodeService
    {
        Task<bool> SendVerificationCodeByEmail(string userName, CancellationToken token);
        Task<string> ValidateVerificationCode(string userName, string code, CancellationToken token);
    }

    public class VerificationCodeService : IVerificationCodeService
    {
        private readonly SmtpSettings _smtpSettings;
        private EmailManager _emailManager;
        private IDataProvider _dataProvider;
        private IUserService _userService;

        public VerificationCodeService(IOptions<SmtpSettings> smtpSettings, EmailManager emailManager, IDataProvider dataProvider, IUserService userService)
        {
            _smtpSettings = smtpSettings.Value;
            _emailManager = emailManager;
            _dataProvider = dataProvider;
            _userService = userService;
        }

        public async Task<bool> SendVerificationCodeByEmail(string userName, CancellationToken token)
        {
            var user = await _userService.GetUserByUserName(userName, token);

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

            await _dataProvider.ExecuteSimpleProc("core.VerificationCode_I", new
            {
                Code = code,
                UserId = user.Id,
                ExpirationDate = DateTimeOffset.UtcNow.AddMinutes(5)
            }, token);

            var result = await _emailManager.SendEmail(
                                _smtpSettings.FromEmail,
                                _smtpSettings.FromEmailDisplayName,
                                [user.Email],
                                "CE Tracker Verification Code",
                                $"<p>Here is the verification code you requested: {code}</p>" +
                                $"<p>If you did not request this code, please ignore this message.</p>");

            return result;
        }

        public async Task<string> ValidateVerificationCode(string userName, string code, CancellationToken token)
        {
            var user = await _userService.GetUserByUserName(userName, token) ?? throw new ApplicationException("An error occurred");

            var verificationCode = (await _dataProvider.LoadData<VerificationCode, dynamic>("core.VerificationCode_S", new
            {
                UserId = user.Id,
                Code = code
            }, token)).ToList().FirstOrDefault();

            if (verificationCode is null || verificationCode.ExpirationDate < DateTime.UtcNow)
            {
                throw new ApplicationException("Verification code incorrect, please try again");
            }

            await _dataProvider.ExecuteSimpleProc("core.VerificationCode_D", new
            {
                verificationCode.Id
            }, token);

            var authToken = TokenManager.GenerateToken(user) ?? throw new ApplicationException("An error occurred");

            return authToken;
        }

        public static string GenerateVerificationCode()
        {
            var code = "";
            var random = new Random();

            for (int i = 0; i < 6; i++)
            {
                code += random.Next(0, 10);
            }

            return code;
        }
    }
}

