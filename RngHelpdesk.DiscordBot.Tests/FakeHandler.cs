using System.Net;
using System.Text;

namespace RngHelpdesk.DiscordBot.Tests;

public sealed record RecordedRequest(HttpMethod Method, string PathAndQuery, string? Authorization, string? Body);

/// <summary>Scripted HttpMessageHandler: records every request and answers via the supplied function.</summary>
public sealed class FakeHandler(Func<RecordedRequest, HttpResponseMessage> respond) : HttpMessageHandler
{
    public List<RecordedRequest> Requests { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        var recorded = new RecordedRequest(request.Method, request.RequestUri!.PathAndQuery, request.Headers.Authorization?.ToString(), body);
        Requests.Add(recorded);
        return respond(recorded);
    }

    public static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK)
        => new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    public static HttpResponseMessage Text(string text, HttpStatusCode status)
        => new(status) { Content = new StringContent(text, Encoding.UTF8, "text/plain") };

    public static HttpResponseMessage Empty(HttpStatusCode status) => new(status);

    public HttpClient CreateClient() => new(this) { BaseAddress = new Uri("https://api.test/") };
}

public sealed class FakeTimeProvider(DateTimeOffset start) : TimeProvider
{
    private DateTimeOffset _now = start;
    public override DateTimeOffset GetUtcNow() => _now;
    public void Advance(TimeSpan by) => _now += by;
}
