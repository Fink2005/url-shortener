using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SagaService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitSaga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserOnboardingStates",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentState = table.Column<string>(type: "text", nullable: false),
                    AuthId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    ConfirmationToken = table.Column<string>(type: "text", nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    AssignedRole = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOnboardingStates", x => x.CorrelationId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserOnboardingStates");
        }
    }
}
