using ModelsLib.BlockchainLib;
using Newtonsoft.Json;
using StackExchange.Redis;
using StorageLib.DB.Redis;
using Utilities;

namespace StorageLib.DB.Redis;

public class RedisService
{
    private readonly IDatabase _db;
    private RedisConnectionManager _connectionManager;
    private ConnectionMultiplexer _redis;
     
    public RedisService()
    {
        _connectionManager = new RedisConnectionManager();
        _db = _connectionManager.GetDatabase();
        _redis = _connectionManager.GetConnection();
    }

   // ********** Key Operations **********
    public async Task<bool> KeyExistsAsync(string key) => await _db.KeyExistsAsync(key);

    public async Task<bool> DeleteKeyAsync(string key) => await _db.KeyDeleteAsync(key);

    public async Task<bool> SetKeyExpirationAsync(string key, TimeSpan expiry) => await _db.KeyExpireAsync(key, expiry);

    public async Task<TimeSpan?> GetKeyTTLAsync(string key) => await _db.KeyTimeToLiveAsync(key);

    public async Task<bool> RenameKeyAsync(string key, string newKey) => await _db.KeyRenameAsync(key, newKey);

    public async Task<RedisType> GetKeyTypeAsync(string key) => await _db.KeyTypeAsync(key);

    // ********** String Operations **********
    public async Task<bool> SetStringAsync(string key, string value) => await _db.StringSetAsync(key, value);

    public async Task<string> GetStringAsync(string key) => await _db.StringGetAsync(key);

    public async Task<long> IncrementStringAsync(string key) => await _db.StringIncrementAsync(key);

    public async Task<long> DecrementStringAsync(string key) => await _db.StringDecrementAsync(key);

    public async Task<bool> AppendStringAsync(string key, string value)
    {
        await _db.StringAppendAsync(key, value);
        return true;
    }

    // ********** Hash Operations **********
    public async Task<bool> SetHashFieldAsync(string hash, string field, string value) => await _db.HashSetAsync(hash, field, value);

    public async Task<string> GetHashFieldAsync(string hash, string field) => await _db.HashGetAsync(hash, field);

    public async Task<Dictionary<string, string>> GetAllHashFieldsAsync(string hash)
    {
        var entries = await _db.HashGetAllAsync(hash);
        var result = new Dictionary<string, string>();
        foreach (var entry in entries)
        {
            result.Add(entry.Name, entry.Value);
        }
        return result;
    }

    public async Task<bool> DeleteHashFieldAsync(string hash, string field) => await _db.HashDeleteAsync(hash, field);

    public async Task<long> GetHashLengthAsync(string hash) => await _db.HashLengthAsync(hash);

    // ********** List Operations **********
    public async Task<long> PushToListLeftAsync(string list, string value) => await _db.ListLeftPushAsync(list, value);

    public async Task<long> PushToListRightAsync(string list, string value) => await _db.ListRightPushAsync(list, value);

    public async Task<string> PopFromListLeftAsync(string list) => await _db.ListLeftPopAsync(list);

    public async Task<string> PopFromListRightAsync(string list) => await _db.ListRightPopAsync(list);

    public async Task<List<string>> GetListRangeAsync(string list, long start, long stop)
    {
        var values = await _db.ListRangeAsync(list, start, stop);
        var result = new List<string>();
        foreach (var value in values)
        {
            result.Add(value);
        }
        return result;
    }

    // ********** Set Operations **********
    public async Task<bool> AddToSetAsync(string set, string value) => await _db.SetAddAsync(set, value);

    public async Task<bool> RemoveFromSetAsync(string set, string value) => await _db.SetRemoveAsync(set, value);

    public async Task<bool> IsMemberOfSetAsync(string set, string value) => await _db.SetContainsAsync(set, value);

    public async Task<HashSet<string>> GetAllSetMembersAsync(string set)
    {
        var values = await _db.SetMembersAsync(set);
        var result = new HashSet<string>();
        foreach (var value in values)
        {
            result.Add(value);
        }
        return result;
    }

    // ********** Sorted Set Operations **********
    public async Task<bool> AddToSortedSetAsync(string sortedSet, string member, double score) => await _db.SortedSetAddAsync(sortedSet, member, score);

    public async Task<bool> RemoveFromSortedSetAsync(string sortedSet, string member) => await _db.SortedSetRemoveAsync(sortedSet, member);

    public async Task<List<string>> GetSortedSetRangeByScoreAsync(string sortedSet, double start, double stop)
    {
        var values = await _db.SortedSetRangeByScoreAsync(sortedSet, start, stop);
        var result = new List<string>();
        foreach (var value in values)
        {
            result.Add(value);
        }
        return result;
    }

    // ********** Pub/Sub Operations **********
    public void PublishMessage(string channel, string message)
    {
        var sub = _redis.GetSubscriber();
        sub.Publish(channel, message);
    }

    public void SubscribeToChannel(string channel, Action<RedisChannel, RedisValue> messageAction)
    {
       var sub = _redis.GetSubscriber(); 
       sub.Subscribe(channel, messageAction);
    }
    
    
    public async Task<List<TransactionModel>> GetAllTransactionsAsync()
    {
        var db = _redis.GetDatabase();
        var server = _redis.GetServer("localhost", 6379);

        List<TransactionModel> transactions = new List<TransactionModel>();
        
        var keys = server.Keys(pattern: "*-*");

        foreach (var key in keys)
        {
            string serializedTransaction = await db.StringGetAsync(key);

            TransactionModel transaction = JsonConvert.DeserializeObject<TransactionModel?>(serializedTransaction);
            if (transaction != null)
            {
                transactions.Add(transaction);
            }

            // transactions.Add(transaction);
        }

        return transactions;
    }
    
    
    public async Task RemoveTransactionsFromRedisAsync(List<TransactionModel> usedTransactions)
    {
        var db = _redis.GetDatabase();

        foreach (var transaction in usedTransactions)
        {
            // Формируем ключ для каждой транзакции, используя ID или уникальное свойство транзакции
            string transactionKey = $"{transaction.Id}"; // Предположим, что у транзакции есть свойство Id

            // Удаляем транзакцию из Redis
            bool wasRemoved = await db.KeyDeleteAsync(transactionKey);

            if (wasRemoved)
            {
                Logger.Log($"Transaction with ID {transaction.Id} has been removed from Redis.", LogLevel.Information, Source.Storage);
            }
            else
            {
                Logger.Log($"Transaction with ID {transaction.Id} was not found in Redis or failed to remove.", LogLevel.Warning, Source.Storage);
            }
        }
    }

    // ********** Transaction Operations **********
    public ITransaction CreateTransaction() => _db.CreateTransaction();

    // ********** Lua Scripting Operations **********
    public async Task<RedisResult> EvaluateScriptAsync(string script, RedisKey[] keys, RedisValue[] values) => await _db.ScriptEvaluateAsync(script, keys, values);
    
     public void Dispose() => _redis.Dispose();

     
     
}