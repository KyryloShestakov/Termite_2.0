using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using NetworkLibrary.Utilities;

namespace NetworkLibrary.Networking.Server;

public class TcpHandler
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
        var canncelationToken = _cancellationTokenSource.Token;

        while (!canncelationToken.IsCancellationRequested)
        {
            try
            {
                string message = await ReadAsync(tcpClient);
                if (string.IsNullOrEmpty(message))
                {
                    Logger.Log("Received empty message");
                    break;
                }
                _requests.Enqueue(new TcpRequest(message));
            }
            catch (Exception e)
            {
                Logger.Log($"Error in receiving message: {e.Message}");
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
                Logger.Log(fullMessage.ToString());
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message);
            throw;
        }
        finally
        {
            Logger.Log("Client disconnected");
        }

        return fullMessage.ToString();
    }

    public async Task<TcpRequest> AwaitRequestAsync()
    {
        await WaitForRequest(_cancellationTokenSource.Token);
        if (_requests.IsEmpty)
        {
            Logger.Log("Received empty request");
        }
        TcpRequest firstRequest = _requests.TryDequeue(out TcpRequest tcpRequest) ? tcpRequest : null;
        return firstRequest ?? throw new NullReferenceException();
    }

    private async Task WaitForRequest(CancellationToken cancellationToken)
    {
        while (_requests.IsEmpty)
        {
            if (cancellationToken.IsCancellationRequested) return;
            await Task.Delay(5000, cancellationToken);
        }
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