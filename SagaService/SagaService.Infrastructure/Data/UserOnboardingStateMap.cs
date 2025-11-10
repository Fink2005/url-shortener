using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SagaService.Domain.States;

namespace SagaService.Infrastructure.Data;

// ✅ Dùng SagaClassMap của MassTransit (fully-qualified)
public class UserOnboardingStateMap
    : MassTransit.EntityFrameworkCoreIntegration.SagaClassMap<UserOnboardingState>
{
    protected override void Configure(EntityTypeBuilder<UserOnboardingState> entity, ModelBuilder model)
    {
        entity.ToTable("UserOnboardingStates");
        entity.HasKey(x => x.CorrelationId);

        entity.Property(x => x.CurrentState);
        entity.Property(x => x.AuthId);
        entity.Property(x => x.Username);
        entity.Property(x => x.Email);
        entity.Property(x => x.CreatedAt);
        entity.Property(x => x.CompletedAt);
    }
}
