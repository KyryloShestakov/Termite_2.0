using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using RRLib.Responses;
using Server.Requests;
using Utilities;
using Server.Responses;

namespace Server;

public class TcpHandler : ResponseService
{
    private TcpClient _currentClient;
    private CancellationTokenSource _cancellationTokenSource;
    private ConcurrentQueue<TcpRequest> _requests;
    public bool IsDisposed { get; private set; }

    public TcpHandler(TcpClient tcpClient)
    {
        _currentClient = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
        _cancellationTokenSource = new CancellationTokenSource();
        IsDisposed = false;
        _requests = new ConcurrentQueue<TcpRequest>();

        _ = StartReceivingAsync(_currentClient);
    }

    private async Task StartReceivingAsync(TcpClient tcpClient)
    {
        var cancellationToken = _cancellationTokenSource.Token;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                string message = await ReadAsync(tcpClient);
                if (string.IsNullOrEmpty(message))
                {
                    Logger.Log("Received empty message", LogLevel.Warning, Source.Server);
                    break;
                }
                _requests.Enqueue(new TcpRequest(message));
            }
            catch (Exception e)
            {
                Logger.Log($"Error in receiving message: {e.Message}", LogLevel.Error, Source.Server);
                break;
            }
        }
    }
    
    private async Task<string> ReadAsync(TcpClient tcpClient)
    {
        if (tcpClient == null) throw new ArgumentNullException(nameof(tcpClient));
        if (IsDisposed) throw new ObjectDisposedException(nameof(tcpClient));

        StringBuilder fullMessage = new StringBuilder();
        NetworkStream stream = tcpClient.GetStream();
        byte[] buffer = new byte[1024];

        try
        {
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                fullMessage.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                Logger.Log("ReadAsync completed", LogLevel.Information, Source.Server);
                break;
            }
        }
        catch (OperationCanceledException)
        {
            Logger.Log("Operation was canceled", LogLevel.Warning, Source.Server);
            return string.Empty;
        }
        catch (Exception ex)
        {
            Logger.Log($"Error while reading data: {ex.Message}", LogLevel.Error, Source.Server);
            throw;
        }
        finally
        {
            Logger.Log("Client disconnected", LogLevel.Information, Source.Server);
        }

        return fullMessage.ToString();
    }

    public async Task<TcpRequest> AwaitRequestAsync()
    {
        Logger.Log("Awaiting request", LogLevel.Information, Source.Server);
    
        await WaitForRequest(_cancellationTokenSource.Token);

        if (_requests.IsEmpty)
        {
            Logger.Log("Received empty request", LogLevel.Warning, Source.Server);
            throw new InvalidOperationException("No requests were received.");
        }

        if (!_requests.TryDequeue(out TcpRequest? tcpRequest))
        {
            Logger.Log("Failed to dequeue a request", LogLevel.Warning, Source.Server);
            throw new InvalidOperationException("Failed to dequeue a request.");
        }

        return tcpRequest;
    }

    private async Task WaitForRequest(CancellationToken cancellationToken)
    {
        while (_requests.IsEmpty)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Logger.Log("Cancellation requested in WaitForRequest", LogLevel.Warning, Source.Server);
                return;
            }
            await Task.Delay(5000, cancellationToken);
        }
    }
    
    public async Task Write(string message)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        await Write(_currentClient, messageBytes);
    }

    public async Task Write(byte[] message)
    {
        await Write(_currentClient, message);
    }

    public async Task Write(Response response)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(response.Message);
        await Write(_currentClient, messageBytes);
    }


    public void Dispose()
    {
        if (IsDisposed) return;
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _requests.Clear();
        _currentClient?.Close();
        IsDisposed = true;
    }
}
