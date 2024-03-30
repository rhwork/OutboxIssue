using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OutboxIssue;
using System.Text.Json;
using Testcontainers.MsSql;

namespace Phoenix.Quotation.Api.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public JsonSerializerOptions HttpClientJsonSerialiserOptions { get; private set; } = default!;
    public ITestHarness ServiceBusHarness => Services.GetTestHarness();

    public ADbContext CreateDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ADbContext>();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var dbContainer = new MsSqlBuilder().WithReuse(true).Build();
        dbContainer.StartAsync().Wait();
        var dbConnectionString = dbContainer.GetConnectionString();
        var connectionString = new SqlConnectionStringBuilder(dbContainer.GetConnectionString())
        {
            InitialCatalog = "TestDb"
        };
        var optionsBuilder = new DbContextOptionsBuilder<ADbContext>();
        optionsBuilder.UseSqlServer(connectionString.ConnectionString);
        var dbContext = new ADbContext(optionsBuilder.Options);
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        builder.ConfigureTestServices(services =>
        {
            services.AddDbContext<ADbContext>(options => options.UseSqlServer(dbContainer.GetConnectionString()));
            services.AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<MessageAConsumer>();
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ReceiveEndpoint("message-a", e =>
                    {
                        e.ConfigureConsumer<MessageAConsumer>(context);
                        e.UseEntityFrameworkOutbox<ADbContext>(context);
                    });

                    cfg.ConfigureEndpoints(context);
                });

                x.AddEntityFrameworkOutbox<ADbContext>(x =>
                {
                    x.UseSqlServer();
                    x.UseBusOutbox();
                });
            });
        });
    }
}
