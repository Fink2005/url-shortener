using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SagaService.Domain.States;

namespace SagaService.Infrastructure.Data;

public class UserOnboardingStateMap : IEntityTypeConfiguration<UserOnboardingState>
{
    public void Configure(EntityTypeBuilder<UserOnboardingState> entity)
    {
        entity.ToTable("UserOnboardingStates");
        entity.HasKey(x => x.CorrelationId);

        entity.Property(x => x.CurrentState);
        entity.Property(x => x.AuthId);
        entity.Property(x => x.UserId);
        entity.Property(x => x.Username);
        entity.Property(x => x.Email);
        entity.Property(x => x.ConfirmationToken);
        entity.Property(x => x.EmailConfirmed);
        entity.Property(x => x.AssignedRole);
        entity.Property(x => x.CreatedAt);
        entity.Property(x => x.CompletedAt);
        entity.Property(x => x.FailureReason);
    }
}
