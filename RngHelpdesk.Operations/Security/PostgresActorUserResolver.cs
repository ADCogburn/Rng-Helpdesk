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
            select user_id
            from actor_user_links
            where actor_id = @actorId and actor_type = @actorType
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
            insert into actor_user_links(actor_id, actor_type, user_id)
            values (@actorId, @actorType, @userId)
            on conflict (actor_id, actor_type)
            do update set user_id = excluded.user_id
        """;

        cmd.Parameters.AddWithValue("actorId", actorId);
        cmd.Parameters.AddWithValue("actorType", actorType.ToString());
        cmd.Parameters.AddWithValue("userId", userId);

        cmd.ExecuteNonQuery();
    }
}