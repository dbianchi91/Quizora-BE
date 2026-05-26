using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ExamEngine.Infrastructure.Persistence;

public sealed class ExamEngineDbContextFactory : IDesignTimeDbContextFactory<ExamEngineDbContext>
{
    public ExamEngineDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ExamEngineDbContext>()
            .UseSqlServer(
                "Server=localhost,1433;Database=Quizora;User Id=sa;Password=Quizora@2024!;TrustServerCertificate=true",
                sql => sql.MigrationsAssembly(typeof(ExamEngineDbContext).Assembly.FullName))
            .Options;
        return new ExamEngineDbContext(options);
    }
}
