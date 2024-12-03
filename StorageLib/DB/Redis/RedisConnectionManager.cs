using StackExchange.Redis;

namespace StorageLib.DB.Redis;

public class RedisConnectionManager
{
    private readonly ConnectionMultiplexer _connection;
    private string connectionString = "localhost:6379";

    public RedisConnectionManager()
    {
        _connection = ConnectionMultiplexer.Connect(connectionString);
    }

    public IDatabase GetDatabase()
    {
        return _connection.GetDatabase();
    }

    public IServer GetServer(string host, int port)
    {
        return _connection.GetServer(host, port);
    }

    public ConnectionMultiplexer GetConnection()
    {
        return _connection;
    }

}