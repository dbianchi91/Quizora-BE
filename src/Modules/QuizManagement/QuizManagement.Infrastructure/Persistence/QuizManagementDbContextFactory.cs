using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QuizManagement.Infrastructure.Persistence;

public sealed class QuizManagementDbContextFactory : IDesignTimeDbContextFactory<QuizManagementDbContext>
{
    public QuizManagementDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<QuizManagementDbContext>()
            .UseSqlServer(
                "Server=localhost,1433;Database=Quizora;User Id=sa;Password=Quizora@2024!;TrustServerCertificate=true",
                sql => sql.MigrationsAssembly(typeof(QuizManagementDbContext).Assembly.FullName))
            .Options;
        return new QuizManagementDbContext(options);
    }
}
