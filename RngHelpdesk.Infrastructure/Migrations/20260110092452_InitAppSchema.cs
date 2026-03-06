using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RngHelpdesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitAppSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.EnsureSchema(
                name: "eventstore");

            migrationBuilder.EnsureSchema(
                name: "projections");

            migrationBuilder.EnsureSchema(
                name: "points");

            migrationBuilder.CreateTable(
                name: "actor_user_links",
                schema: "identity",
                columns: table => new
                {
                    ActorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorType = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_actor_user_links", x => new { x.ActorId, x.ActorType });
                });

            migrationBuilder.CreateTable(
                name: "auth_users",
                schema: "identity",
                columns: table => new
                {
                    Username = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    MustChangePassword = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auth_users", x => x.Username);
                });

            migrationBuilder.CreateTable(
                name: "event_store",
                schema: "eventstore",
                columns: table => new
                {
                    GlobalPosition = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StreamType = table.Column<string>(type: "text", nullable: false),
                    StreamId = table.Column<int>(type: "integer", nullable: false),
                    StreamVersion = table.Column<int>(type: "integer", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    EventSchemaVer = table.Column<int>(type: "integer", nullable: false),
                    OccurredUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecordedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_store", x => x.GlobalPosition);
                });

            migrationBuilder.CreateTable(
                name: "event_streams",
                schema: "eventstore",
                columns: table => new
                {
                    StreamType = table.Column<string>(type: "text", nullable: false),
                    StreamId = table.Column<int>(type: "integer", nullable: false),
                    CurrentVersion = table.Column<int>(type: "integer", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_streams", x => new { x.StreamType, x.StreamId });
                });

            migrationBuilder.CreateTable(
                name: "projection_checkpoints",
                schema: "projections",
                columns: table => new
                {
                    ProjectionName = table.Column<string>(type: "text", nullable: false),
                    LastPosition = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projection_checkpoints", x => x.ProjectionName);
                });

            migrationBuilder.CreateTable(
                name: "rank_thresholds",
                schema: "points",
                columns: table => new
                {
                    Rank = table.Column<string>(type: "text", nullable: false),
                    PointsRequired = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rank_thresholds", x => x.Rank);
                });

            migrationBuilder.CreateIndex(
                name: "IX_event_store_GlobalPosition",
                schema: "eventstore",
                table: "event_store",
                column: "GlobalPosition",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_event_store_StreamType_StreamId_StreamVersion",
                schema: "eventstore",
                table: "event_store",
                columns: new[] { "StreamType", "StreamId", "StreamVersion" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "actor_user_links",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "auth_users",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "event_store",
                schema: "eventstore");

            migrationBuilder.DropTable(
                name: "event_streams",
                schema: "eventstore");

            migrationBuilder.DropTable(
                name: "projection_checkpoints",
                schema: "projections");

            migrationBuilder.DropTable(
                name: "rank_thresholds",
                schema: "points");
        }
    }
}
