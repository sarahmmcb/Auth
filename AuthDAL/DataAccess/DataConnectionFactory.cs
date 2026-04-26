using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AuthDAL.DataAccess;

public interface IDataConnectionFactory
{
    DbConnection CeTrackerSqlConnection();
}
public class DataConnectionFactory : IDataConnectionFactory
{
    private readonly IConfiguration _config;

    public DataConnectionFactory(IConfiguration config)
    {
        _config = config;
    }

    public DbConnection CeTrackerSqlConnection() => new SqlConnection(_config.GetSection("ConnectionStrings:Default").Value);
}

