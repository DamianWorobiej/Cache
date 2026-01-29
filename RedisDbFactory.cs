using StackExchange.Redis;

namespace Cache;

public static class RedisDbFactory
{
    public static IDatabase GetDatabase()
    {
        var redisConnection = ConnectionMultiplexer.Connect($"localhost:6379");
        
        return redisConnection.GetDatabase();
    }
}
