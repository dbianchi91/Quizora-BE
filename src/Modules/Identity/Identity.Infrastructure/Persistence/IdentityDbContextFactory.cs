using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Identity.Infrastructure.Persistence;

public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=Quizora;User Id=sa;Password=Quizora@2024!;TrustServerCertificate=true",
            sql => sql.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName));

        return new IdentityDbContext(optionsBuilder.Options);
    }
}
