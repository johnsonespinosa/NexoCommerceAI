using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexoCommerceAI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxDeadLetterTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedOn",
                table: "OutboxMessages");

            migrationBuilder.AlterColumn<int>(
                name: "RetryCount",
                table: "OutboxMessages",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "AggregateId",
                table: "OutboxMessages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "OutboxMessages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "OutboxMessages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OutboxDeadLetters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Content = table.Column<string>(type: "jsonb", nullable: false),
                    Error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    OccurredOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MovedToDeadLetterAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxDeadLetters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_AggregateId",
                table: "OutboxMessages",
                column: "AggregateId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_CorrelationId",
                table: "OutboxMessages",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_EventType",
                table: "OutboxMessages",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOn",
                table: "OutboxMessages",
                column: "ProcessedOn",
                filter: "\"ProcessedOn\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOn_OccurredOn",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOn", "OccurredOn" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOn_RetryCount",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOn", "RetryCount" },
                filter: "\"ProcessedOn\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxDeadLetters_EventType",
                table: "OutboxDeadLetters",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxDeadLetters_MovedToDeadLetterAt",
                table: "OutboxDeadLetters",
                column: "MovedToDeadLetterAt");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxDeadLetters_OriginalMessageId",
                table: "OutboxDeadLetters",
                column: "OriginalMessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxDeadLetters");

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_AggregateId",
                table: "OutboxMessages");

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_CorrelationId",
                table: "OutboxMessages");

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_EventType",
                table: "OutboxMessages");

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedOn",
                table: "OutboxMessages");

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedOn_OccurredOn",
                table: "OutboxMessages");

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedOn_RetryCount",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "AggregateId",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "OutboxMessages");

            migrationBuilder.AlterColumn<int>(
                name: "RetryCount",
                table: "OutboxMessages",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOn",
                table: "OutboxMessages",
                column: "ProcessedOn");
        }
    }
}
