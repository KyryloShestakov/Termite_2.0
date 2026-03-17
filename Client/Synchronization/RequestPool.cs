using System.Collections.Concurrent;
using Ter_Protocol_Lib.Requests;
using Utilities;

namespace Client;

public class RequestPool
{
    // Потокобезопасная коллекция для хранения запросов
    private readonly ConcurrentQueue<TerProtocol<object>> _requests;

    public RequestPool()
    {
        _requests = new ConcurrentQueue<TerProtocol<object>>();
    }

    /// <summary>ц
    /// Добавляет запрос в пул
    /// </summary>
    /// <param name="request">Запрос для добавления</param>
    public void AddRequest(TerProtocol<object> request)
    {
        if (request.Header.MessageType == TerMessageType.Empty)
        {
            Logger.Log("Request is null", LogLevel.Warning, Source.Client);
        }
        else
        {
            _requests.Enqueue(request);
        }
        
        Logger.Log($"Request added. Total requests in pool: {_requests.Count}", LogLevel.Information, Source.Client);
    }

    /// <summary>
    /// Получает и удаляет следующий запрос из пула
    /// </summary>
    /// <returns>Следующий запрос, если он есть; null, если пусто</returns>
    public TerProtocol<object>? GetNextRequest()
    {
        if (_requests.TryDequeue(out var request)) // Удаляет запрос из очереди, если он есть
        {
            Logger.Log($"Request dequeued: {request.Header.MethodType}. Remaining requests: {_requests.Count}", LogLevel.Information, Source.Client);
            return request;
        }
        else
        {
            Logger.Log("No requests available", LogLevel.Warning, Source.Client);
            return null;
        }
    }

    /// <summary>
    /// Возвращает количество запросов в пуле
    /// </summary>
    public int Count => _requests.Count;

    /// <summary>
    /// Получает все запросы в виде списка без удаления
    /// </summary>
    /// <returns>Список запросов</returns>
    public List<TerProtocol<object>> GetAllRequests()
    {
        return _requests.ToList();
    }
}