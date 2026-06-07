using Api.Dto;
using Core.Documents.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api")]
public class ThreadsController : ControllerBase
{
    private readonly IRagSearchService _rag;

    public ThreadsController(IRagSearchService rag) => _rag = rag;

    [HttpPost("threads/{threadId:guid}/stream")]
    [Produces("text/event-stream")]
    public async Task StreamMessage(
        Guid threadId,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Accel-Buffering", "no");

        await foreach (var token in _rag.AskStreamingAsync(
                           request.Content,
                           request.Collection,
                           ct: cancellationToken))
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(new { token });
            await Response.WriteAsync($" {payload}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        await Response.WriteAsync("event: done\n {}\n\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }
}