using Microsoft.EntityFrameworkCore;
using SagaService.Domain.States;

namespace SagaService.Infrastructure.Data;

public class SagaStateDbContext : DbContext
{
    public DbSet<UserOnboardingState> UserOnboardingStates { get; set; }

    public SagaStateDbContext(DbContextOptions<SagaStateDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure UserOnboardingState mapping
        modelBuilder.ApplyConfiguration(new UserOnboardingStateMap());
    }
}
