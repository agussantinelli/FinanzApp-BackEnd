using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Data
{
    public class DBFinanzasContextFactory : IDesignTimeDbContextFactory<DBFinanzasContext>
    {
        public DBFinanzasContext CreateDbContext(string[] args)
        {
            // EF design-time normalmente se para en el proyecto de inicio (WebAPI)
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // WebAPI como startup
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("FinanzAppDb");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("No se encontró la ConnectionString 'FinanzAppDb'.");

            var optionsBuilder = new DbContextOptionsBuilder<DBFinanzasContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new DBFinanzasContext(optionsBuilder.Options);
        }
    }
}
