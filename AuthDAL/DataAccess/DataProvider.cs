using System.Data;
using System.Data.Common;
using System.Transactions;
using Auth.Contracts;
using Auth.Contracts.RequestContracts;
using Dapper;

namespace AuthDAL.DataAccess;

public interface IDataProvider
{
    Task<int> SaveNewUser(CreateUserRequest user, CancellationToken token);
    Task<int> UpdateUser(int updateUserId, UpdateUserRequest request, CancellationToken token);
    Task UpdateUserPassword(int userId, int updateUserId, string password, CancellationToken token);
    Task DeleteUser(int userId, int updateUserId, CancellationToken token);
    Task<IEnumerable<T>> LoadData<T, U>(string storedProcedure,
        U parameters,
        CancellationToken token);
    Task ExecuteSimpleProc<U>(string storedProcedure, U parameters, CancellationToken token);
}

public class DataProvider : IDataProvider
{
    private IDataConnectionFactory _dataConnectionFactory;

    public DataProvider(IDataConnectionFactory dataConnectionFactory)
    {
        _dataConnectionFactory = dataConnectionFactory;
    }

    public async Task<int> SaveNewUser(CreateUserRequest user, CancellationToken token)
    {
        using var txScope = new TransactionScope(TransactionScopeOption.RequiresNew,
            new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        using DbConnection connection = _dataConnectionFactory.CeTrackerSqlConnection();
        await connection.OpenAsync(token);

        try
        {
            var newUserId = await InsertNewUser(user, connection, token);
            await SaveUserRoles(user, newUserId, connection, token);

            txScope.Complete();

            return newUserId;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Exception occurred when saving a new user: {ex.Message}");
        }
        finally
        {
            txScope.Dispose();
        }

    }

    public async Task<int> UpdateUser(int updateUserId, UpdateUserRequest request, CancellationToken token)
    {
        var connection = _dataConnectionFactory.CeTrackerSqlConnection();

        // Select the record first so we can use existing data for null fields on the request
        var currentUser = (await LoadData<User, dynamic>("core.User_By_UserId_S", new { request.UserId }, token)).ToList().FirstOrDefault();

        if (currentUser == null)
        {
            throw new InvalidDataException("No user found for supplied User Id");
        }

        var command = new CommandDefinition("core.User_By_UserId_U_I",
            new
            {
                UserId = currentUser.Id
                ,FirstName = request.FirstName ?? currentUser.FirstName
                ,LastName = request.LastName ?? currentUser.LastName
                ,Email = request.Email ?? currentUser.Email
                ,UserName = request.Username ?? currentUser.UserName
                ,Password = currentUser.Password
                ,AccountStatus = request.AccountStatus ?? currentUser.AccountStatus
                ,UpdateUserId = updateUserId
                ,IsDeleted = request.IsDeleted ?? false
            },
            cancellationToken: token);

        var result = await connection.QuerySingleAsync<int>(command);

        return result;
    }

    public async Task UpdateUserPassword(int userId, int updateUserId, string password, CancellationToken token)
    {
        var connection = _dataConnectionFactory.CeTrackerSqlConnection();

        var command = new CommandDefinition("core.User_Password_U", new
        {
            userId,
            password,
            updateUserId
        },
        cancellationToken: token);

        await connection.ExecuteAsync(command);
    }

    public async Task DeleteUser(int userId, int updateUserId, CancellationToken token)
    {
        var connection = _dataConnectionFactory.CeTrackerSqlConnection();

        var command = new CommandDefinition("core.User_D", new
        {
            userId,
            updateUserId
        },
        cancellationToken: token);

        await connection.ExecuteAsync(command);
    }

    public async Task<IEnumerable<T>> LoadData<T, U>(
        string storedProcedure,
        U parameters,
        CancellationToken token
    )
    {
        using IDbConnection connection = _dataConnectionFactory.CeTrackerSqlConnection();

        var command = new CommandDefinition(storedProcedure, parameters,
            commandType: CommandType.StoredProcedure, cancellationToken: token);

        var result = await connection.QueryAsync<T>(command);

        return result;
    }

    public async Task ExecuteSimpleProc<U>(
        string storedProcedure,
        U parameters,
        CancellationToken token)
    {
        using IDbConnection connection = _dataConnectionFactory.CeTrackerSqlConnection();

        var command = new CommandDefinition(storedProcedure, parameters,
            commandType: CommandType.StoredProcedure, cancellationToken: token);

        await connection.ExecuteAsync(command);
    }

    private async Task<int> InsertNewUser(CreateUserRequest user, IDbConnection connection, CancellationToken token)
    {
        var command = new CommandDefinition("core.User_By_UserId_U_I", new
        {
            UserId = 0,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            UserName = user.Username,
            Password = user.Password,
            AccountStatus = user.AccountStatus,
            UpdateUserId = 0,
            IsDeleted = false
        },
        cancellationToken: token);

        var userId = await connection.QuerySingleAsync<int>(command);

        return userId;
    }

    private async Task SaveUserRoles(
        CreateUserRequest user,
        int newUserId,
        DbConnection connection,
        CancellationToken token)
    {
        foreach ( var role in user.Roles )
        {
            var command = new CommandDefinition("core.UserRole_I", new
            {
                UserId = newUserId,
                RoleId = role
            },
            cancellationToken: token);

            await connection.ExecuteAsync(command);
        }
    }
}
