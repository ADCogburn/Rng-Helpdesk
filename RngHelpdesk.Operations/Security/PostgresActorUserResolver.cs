using Npgsql;
using RngHelpdesk.Contracts.Security;

namespace RngHelpdesk.Operations.Security;

public sealed class PostgresActorUserResolver : IActorUserResolver
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresActorUserResolver(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public int? ResolveUserId(Guid actorId, ActorType actorType)
    {
        using var conn = _dataSource.OpenConnection();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = """
            select "UserId"
            from identity.actor_user_links
            where "ActorId" = @actorId and "ActorType" = @actorType
        """;

        cmd.Parameters.AddWithValue("actorId", actorId);
        cmd.Parameters.AddWithValue("actorType", actorType.ToString());

        var result = cmd.ExecuteScalar();
        return result == null ? null : (int)result;
    }

    public void RegisterActor(Guid actorId, ActorType actorType, int userId)
    {
        using var conn = _dataSource.OpenConnection();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = """
            insert into identity.actor_user_links("ActorId", "ActorType", "UserId")
            values (@actorId, @actorType, @userId)
            on conflict ("ActorId", "ActorType")
            do update set "UserId" = excluded."UserId"
        """;

        cmd.Parameters.AddWithValue("actorId", actorId);
        cmd.Parameters.AddWithValue("actorType", actorType.ToString());
        cmd.Parameters.AddWithValue("userId", userId);

        cmd.ExecuteNonQuery();
    }
}