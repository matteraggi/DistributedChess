using StackExchange.Redis;


namespace Shared.Redis
{
    public partial class RedisService
    {
        private readonly ConnectionMultiplexer _redis;
        public ISubscriber Subscriber => _redis.GetSubscriber();
        public IDatabase Db => _redis.GetDatabase();

        public RedisService(string connectionString)
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
        }
        public IServer GetServer()
        {
            var endpoint = _redis.GetEndPoints().First();
            return _redis.GetServer(endpoint);
        }

    }
}
