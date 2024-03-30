using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace OutboxIssue;

public class ADbContext(DbContextOptions<ADbContext> options) : DbContext(options)
{
    public DbSet<AEntity> AEntities { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddTransactionalOutboxEntities();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var changedItems = ChangeTracker
             .Entries()
             .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
             .ToList();

        if (!changedItems.Any(x => x.Entity is InboxState))
        {
            GlobalStateForAssertions.ActualSendEndpointProviderType = this.GetService<ISendEndpointProvider>().GetType();
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
