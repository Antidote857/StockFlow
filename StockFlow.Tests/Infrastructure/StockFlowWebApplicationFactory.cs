using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StockFlow.Data;

namespace StockFlow.Tests.Infrastructure;

public sealed class StockFlowWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly string _databaseName =
        $"StockFlowTests_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the original SQL Server configuration.
            services.RemoveAll<
                IDbContextOptionsConfiguration<StockFlowDbContext>>();

            services.RemoveAll<
                DbContextOptions<StockFlowDbContext>>();

            services.RemoveAll<StockFlowDbContext>();

            // Register InMemory as the only provider for testing.
            services.AddDbContext<StockFlowDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}