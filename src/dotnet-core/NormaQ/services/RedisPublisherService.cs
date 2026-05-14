using StackExchange.Redis;
using System.Text.Json;
using NormaQ.ViewModels;

namespace NormaQ.Services;

public class RedisPublisherService
{
    private readonly IConnectionMultiplexer _redis;
    private const string Channel = "documents:approved";

    public RedisPublisherService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task PublishDocumentAsync(DocumentApprovedMessage message)
    {
        var db = _redis.GetSubscriber();
        var payload = JsonSerializer.Serialize(message);
        await db.PublishAsync(Channel, payload);
    }
}