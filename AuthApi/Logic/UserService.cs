using Auth.Contracts;
using Auth.Contracts.RequestContracts;
using AuthApi.Data;
using AuthApi.Security;

namespace AuthApi.Logic
{
    public interface IUserService
    {
        Task RegisterUser(UpdateUserRequest user);
    }

    public class UserService(UserDbContext _userDbContext) : IUserService
    {
        public async Task RegisterUser(UpdateUserRequest user)
        {
            if (_userDbContext.User.Any(u => string.Equals(u.UserName, user.Username)))
            {
                throw new ApplicationException("That username is already taken");
            }

            // TODO: these saves should be in a transaction
            await SaveNewUser(user);
            await SaveUserRoles(user);
        }

        internal virtual async Task SaveNewUser(UpdateUserRequest user)
        {
            var newUser = new User
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.Username,
                Password = PasswordManager.HashPassword(user.Password),
                AccountStatus = AccountStatus.Active,
                CreateDate = DateTimeOffset.Now
            };

            _userDbContext.User.Add(newUser);
            await _userDbContext.SaveChangesAsync();
        }

        internal virtual async Task SaveUserRoles(UpdateUserRequest user)
        {
            if (user.UserId is null)
            {
                throw new ApplicationException("UserId is invalid");
            }

            if (user.RoleIds.Length > 0)
            {
                foreach (var roleId in user.RoleIds)
                {
                    var newUserRole = new UserRole { UserId = (int)user.UserId, RoleId = roleId };
                    _userDbContext.UserRoles.Add(newUserRole);
                    await _userDbContext.SaveChangesAsync();
                }
            }
        }
    }
}
