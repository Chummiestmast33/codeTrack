using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Backend.Infrastructure.Persistence.DesignTime;

/// <summary>
/// Design-time factory so dotnet-ef commands never boot Program.cs
/// (which requires real connection strings and JWT secrets).
/// The fallback connection string is only a placeholder: commands like
/// migrations add/script/list never connect; database update still
/// needs a real ConnectionStrings__DefaultConnection.
/// </summary>
public sealed class TallerDbContextFactory : IDesignTimeDbContextFactory<TallerDbContext>
{
    public TallerDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=codetrack_dummy;Username=codetrack;Password=dummy";

        var options = new DbContextOptionsBuilder<TallerDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new TallerDbContext(options);
    }
}
