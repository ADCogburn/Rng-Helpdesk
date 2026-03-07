using Microsoft.EntityFrameworkCore;
using RngHelpdesk.Infrastructure.Persistence.Contexts;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder model)
    {
        ConfigureEventStore(model);
        ConfigureProjections(model);
        ConfigureIdentity(model);
        ConfigurePoints(model);
    }

    private static void ConfigureEventStore(ModelBuilder model)
    {
        model.Entity<EventStreamRow>(e =>
        {
            e.ToTable("event_streams", "eventstore");
            e.HasKey(x => new { x.StreamType, x.StreamId });

            e.Property(x => x.StreamType).IsRequired();
            e.Property(x => x.StreamId).IsRequired();
            e.Property(x => x.CurrentVersion).IsRequired();

            e.Property(x => x.CreatedUtc)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            e.Property(x => x.UpdatedUtc)
                .HasColumnType("timestamp with time zone")
                .IsRequired();
        });

        model.Entity<EventRecordRow>(e =>
        {
            e.ToTable("event_store", "eventstore");
            e.HasKey(x => x.GlobalPosition);

            e.HasIndex(x => new { x.StreamType, x.StreamId, x.StreamVersion })
                .IsUnique();

            e.HasIndex(x => x.GlobalPosition).IsUnique();

            e.Property(x => x.EventType).IsRequired();
            e.Property(x => x.StreamType).IsRequired();

            e.Property(x => x.Payload)
                .HasColumnType("jsonb")
                .IsRequired();

            e.Property(x => x.Metadata)
                .HasColumnType("jsonb")
                .IsRequired();

            e.Property(x => x.OccurredUtc)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            e.Property(x => x.RecordedUtc)
                .HasColumnType("timestamp with time zone")
                .IsRequired();
        });
    }

    private static void ConfigureProjections(ModelBuilder model)
    {
        model.Entity<ProjectionCheckpointRow>(e =>
        {
            e.ToTable("projection_checkpoints", "projections");
            e.HasKey(x => x.ProjectionName);

            e.Property(x => x.LastPosition).IsRequired();

            e.Property(x => x.UpdatedUtc)
                .HasColumnType("timestamp with time zone")
                .IsRequired();
        });
    }

    private static void ConfigureIdentity(ModelBuilder model)
    {
        model.Entity<ActorUserLinkRow>(e =>
        {
            e.ToTable("actor_user_links", "identity");
            e.HasKey(x => new { x.ActorId, x.ActorType });
        });

        model.Entity<AuthUserRow>(e =>
        {
            e.ToTable("auth_users", "identity");
            e.HasKey(x => x.Username);

            e.Property(x => x.Username).IsRequired();
            e.Property(x => x.PasswordHash).IsRequired();
        });
    }

    private static void ConfigurePoints(ModelBuilder model)
    {
        model.Entity<RankThresholdRow>(e =>
        {
            e.ToTable("rank_thresholds", "points");
            e.HasKey(x => x.Rank);

            e.Property(x => x.Rank).IsRequired();
            e.Property(x => x.PointsRequired).IsRequired();
            e.Property(x => x.SortOrder).IsRequired();
        });
    }
}
