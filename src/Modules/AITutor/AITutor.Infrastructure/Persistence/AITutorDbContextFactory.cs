using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AITutor.Infrastructure.Persistence;

public sealed class AITutorDbContextFactory : IDesignTimeDbContextFactory<AITutorDbContext>
{
    public AITutorDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AITutorDbContext>()
            .UseSqlServer(
                "Server=localhost,1433;Database=Quizora;User Id=sa;Password=Quizora@2024!;TrustServerCertificate=true",
                sql => sql.MigrationsAssembly(typeof(AITutorDbContext).Assembly.FullName))
            .Options;
        return new AITutorDbContext(options);
    }
}
