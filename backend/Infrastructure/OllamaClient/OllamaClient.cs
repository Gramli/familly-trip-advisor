using Ardalis.GuardClauses;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OllamaSharp;
using System.Text;

namespace familly_trip_advisor.Infrastructure.OllamaClient
{
    public interface IOllamaClient
    {
        Task<string> GetResponseAsync(string prompt, CancellationToken cancellationToken);
        Task<string> GetPreservedResponseAsync(string sessionId, string prompt, CancellationToken cancellationToken);
    }

    internal sealed class OllamaClient : IOllamaClient
    {
        private readonly IChatClient _chatClient;
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan SessionExpiry = TimeSpan.FromMinutes(30);

        private static readonly ChatOptions NothinkOptions = new()
        {
            AdditionalProperties = new AdditionalPropertiesDictionary
            {
                ["think"] = false,
                ["keep_alive"] = 10m,
            }
        };

        public OllamaClient(IOptions<OllamaOptions> options, IMemoryCache cache)
        {
            var ollamaOptions = Guard.Against.Null(options).Value;
            _chatClient = new OllamaApiClient(new Uri(ollamaOptions.BaseUrl), ollamaOptions.ModelName);
            _cache = Guard.Against.Null(cache);
        }

        public async Task<string> GetResponseAsync(string prompt, CancellationToken cancellationToken)
        {
            var builder = new StringBuilder();
            await foreach (ChatResponseUpdate item in
                _chatClient.GetStreamingResponseAsync(
                    [new(ChatRole.User, prompt)],
                    NothinkOptions,
                    cancellationToken))
            {
                builder.Append(item.Text);
            }
            return builder.ToString();
        }

        public async Task<string> GetPreservedResponseAsync(string sessionId, string prompt, CancellationToken cancellationToken)
        {
            var history = _cache.GetOrCreate(sessionId, entry =>
            {
                entry.SlidingExpiration = SessionExpiry;
                return new List<ChatMessage>();
            })!;

            history.Add(new ChatMessage(ChatRole.User, prompt));

            var builder = new StringBuilder();
            await foreach (ChatResponseUpdate item in
                _chatClient.GetStreamingResponseAsync(
                    history,
                    NothinkOptions,
                    cancellationToken))
            {
                builder.Append(item.Text);
            }

            var response = builder.ToString();

            history.Add(new ChatMessage(ChatRole.Assistant, response));

            _cache.Set(sessionId, history, new MemoryCacheEntryOptions
            {
                SlidingExpiration = SessionExpiry
            });

            return response;
        }
    }
}
