using Auth.Contracts;
using Auth.Contracts.RequestContracts;
using AuthApi.Logging;
using AuthApi.Security;
using AuthDAL.DataAccess;
using AuthDAL.Models;

namespace AuthApi.Logic
{
    public interface IUserService
    {
        Task<int> RegisterUser(CreateUserRequest user, CancellationToken token);
        Task UpdatePassword(UpdatePasswordRequest user, CancellationToken token);
        Task<User?> GetUserByUserId(int userId, CancellationToken token);
        Task<User?> GetUserByUserName(string userName, CancellationToken token);
        Task<int> UpdateUser(UpdateUserRequest user, CancellationToken token);
        Task DeleteUser(int userId, CancellationToken token);
    }

    public class UserService(IDataProvider _dataProvider) : IUserService
    {
        public async Task<int> RegisterUser(CreateUserRequest user, CancellationToken token)
        {
            var currentUser = await GetUserByUserName(user.Username, token);
            if (currentUser is not null)
            {
                AuthLogger.LogWarning<UserService>($"Attempt to register with existing username: {user.Username}");
                throw new ApplicationException("That username is already taken");
            }

            user.Password = PasswordManager.HashPassword(user.Password);
            SetUserDefaults(user);

            int result;

            try
            {
                result = await _dataProvider.SaveNewUser(user, token);
            }
            catch (Exception ex)
            {
                AuthLogger.LogError<UserService>($"Error on User Creation for Username {user.Username}: {ex.Message}");
                throw new ApplicationException("User Creation Failed");
            }

            return result;
        }

        public async Task UpdatePassword(UpdatePasswordRequest request, CancellationToken token)
        {
            var user = await GetUserByUserId(request.UserId, token);

            if (user is null)
            {
                AuthLogger.LogError<UserService>("Update Password request failed because supplied user is null");
                throw new ApplicationException("User invalid");
            }

            var hashedPassword = PasswordManager.HashPassword(request.Password);
            await _dataProvider.UpdateUserPassword(request.UserId, 0, hashedPassword, token);
        }

        public async Task<User?> GetUserByUserId(int userId, CancellationToken token)
        {
            var result = await _dataProvider.LoadData<UserDTO, dynamic>("core.User_By_UserId_S", new { userId }, token);
            var user = ConstructUser(result);
            return user;
        }

        public async Task<User?> GetUserByUserName(string userName, CancellationToken token)
        {
            var result = await _dataProvider.LoadData<UserDTO, dynamic>("core.User_By_UserName_S", new { userName }, token);
            var user = ConstructUser(result);
            return user;
        }

        public async Task<int> UpdateUser(UpdateUserRequest request, CancellationToken token)
        {
            var result = await _dataProvider.UpdateUser(0, request, token);

            if (result <= 0)
            {
                AuthLogger.LogError<UserService>("Update User operation returned null or non-positive user id");
                throw new ApplicationException($"An error ocurred updating the user");
            }

            return result;
        }

        public async Task DeleteUser(int userId, CancellationToken token)
        {
            AuthLogger.LogInformation<UserService>($"Deleting user with id: {userId}");
            await _dataProvider.DeleteUser(userId, 0, token);
            AuthLogger.LogInformation<UserService>($"Delete user {userId} successful");
        }

        private void SetUserDefaults(CreateUserRequest user)
        {
            if (user.AccountStatus == 0)
            {
                user.AccountStatus = AccountStatus.Active;
            }

            if (user.Roles is null
                || user.Roles.Count == 0
                || (user.Roles.Count == 1 && user.Roles.First() == 0))
            {
                user.Roles = [1]; // TODO: Make this an enum to avoid magic numbers or select the ID out of the DB
            }
        }

        private User? ConstructUser(IEnumerable<UserDTO> result)
        {
            var userList = result.ToList();
            if (userList.Count == 0)
            {
                return null;
            }

            var user = new User
            {
                Id = userList[0].Id,
                FirstName = userList[0].FirstName,
                LastName = userList[0].LastName,
                Email = userList[0].Email,
                UserName = userList[0].UserName,
                Password = userList[0].Password,
                AccountStatus = userList[0].AccountStatus,
                Roles = []
            };

            userList.ForEach(u => user.Roles.Add(new Role
            {
                RoleId = u.RoleId,
                RoleName = u.RoleName

            }));

            return user;
        }
    }
}
