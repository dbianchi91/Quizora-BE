using System.Data;
using Microsoft.Data.SqlClient;
using Quizora.SharedKernel;

namespace Quizora.API.Infrastructure;

internal sealed class SqlConnectionFactory(IConfiguration configuration) : IDbConnectionFactory
{
    public IDbConnection CreateConnection() =>
        new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
}
