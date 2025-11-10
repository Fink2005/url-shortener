using Microsoft.EntityFrameworkCore;
using SagaService.Domain.States;

namespace SagaService.Infrastructure.Data;

public class SagaStateDbContext
    : MassTransit.EntityFrameworkCoreIntegration.SagaDbContext<UserOnboardingState, UserOnboardingStateMap>
{
    public SagaStateDbContext(DbContextOptions<SagaStateDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // MassTransit tự map dựa trên UserOnboardingStateMap
        base.OnModelCreating(modelBuilder);
    }
}
