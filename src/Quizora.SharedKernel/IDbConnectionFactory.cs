using System.Data;

namespace Quizora.SharedKernel;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
