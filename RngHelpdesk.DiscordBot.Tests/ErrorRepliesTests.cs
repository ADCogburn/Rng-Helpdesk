using System.Net;
using RngHelpdesk.DiscordBot.Api;
using RngHelpdesk.DiscordBot.Interactions;

namespace RngHelpdesk.DiscordBot.Tests;

public class ErrorRepliesTests
{
    [Fact]
    public void Exchange_404_means_not_registered()
        => Assert.Equal(new ErrorReplies.Reply(ErrorReplies.NotRegistered, false),
            ErrorReplies.Describe(new TokenExchangeException(HttpStatusCode.NotFound)));

    [Fact]
    public void Exchange_403_means_deactivated()
        => Assert.Equal(new ErrorReplies.Reply(ErrorReplies.Deactivated, false),
            ErrorReplies.Describe(new TokenExchangeException(HttpStatusCode.Forbidden)));

    [Fact]
    public void Endpoint_403_means_admin_required()
        => Assert.Equal(new ErrorReplies.Reply(ErrorReplies.NeedsAdmin, false),
            ErrorReplies.Describe(new ApiException(HttpStatusCode.Forbidden)));

    [Fact]
    public void Endpoint_404_uses_api_message_or_falls_back()
    {
        Assert.Equal("RSN not linked.", ErrorReplies.Describe(new ApiException(HttpStatusCode.NotFound, "RSN not linked.")).Message);
        Assert.Equal(ErrorReplies.NotFound, ErrorReplies.Describe(new ApiException(HttpStatusCode.NotFound)).Message);
    }

    [Fact]
    public void BadRequest_uses_api_message()
        => Assert.Equal("Points must be positive.", ErrorReplies.Describe(new ApiException(HttpStatusCode.BadRequest, "Points must be positive.")).Message);

    [Theory]
    [MemberData(nameof(Outages))]
    public void Outages_are_generic_and_logged(Exception exception)
        => Assert.Equal(new ErrorReplies.Reply(ErrorReplies.Unavailable, true), ErrorReplies.Describe(exception));

    public static TheoryData<Exception> Outages() => new()
    {
        new HttpRequestException("connection refused"),
        new TaskCanceledException(),
        new ApiException(HttpStatusCode.InternalServerError),
        new ApiException(HttpStatusCode.Unauthorized),
        new InvalidOperationException("boom"),
    };
}
