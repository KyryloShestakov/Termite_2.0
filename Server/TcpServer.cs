using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using RRLib;
using RRLib.Responses;
using Server.Controllers;
using Server.Executors;
using Server.Requests;
using Utilities;

public class TcpServer
{
    private readonly TcpListener _tcpListener;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ConcurrentQueue<TcpRequest> _requestQueue;
    private readonly Controller _controller;
    private readonly RequestExecutor _requestExecutor;
    private readonly SemaphoreSlim _connectionSemaphore;

    private const int MaxConnections = 5;

    public TcpServer()
    {
        _tcpListener = new TcpListener(IPAddress.Any, 8080);
        _cancellationTokenSource = new CancellationTokenSource();
        _requestQueue = new ConcurrentQueue<TcpRequest>();
        _controller = new Controller();
        _requestExecutor = new RequestExecutor();
        _connectionSemaphore = new SemaphoreSlim(MaxConnections, MaxConnections);
    }

    public async Task StartAsync()
    {
        _tcpListener.Start();
        Logger.Log("Server started...", LogLevel.Information, Source.Server);

        _ = Task.Run(ProcessRequestQueueAsync);

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                await _connectionSemaphore.WaitAsync();

                TcpClient tcpClient = await _tcpListener.AcceptTcpClientAsync();
                Logger.Log($"Client connected: {tcpClient.Client.RemoteEndPoint}", LogLevel.Information, Source.Server);
                _ = HandleClientAsync(tcpClient);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error accepting client: {ex.Message}", LogLevel.Error, Source.Server);
            }
        }
    }

    private async Task HandleClientAsync(TcpClient tcpClient)
    {
        using (tcpClient)
        {
            try
            {
                NetworkStream networkStream = tcpClient.GetStream();
                using var reader = new StreamReader(networkStream, Encoding.UTF8);
                using var writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true };

                while (true)
                {
                    string? request = await reader.ReadLineAsync();
                    if (request == null)
                    {
                        Logger.Log("Client disconnected.", LogLevel.Information, Source.Server);
                        break;
                    }
                    
                    Logger.Log($"Raw request received: {request}", LogLevel.Information, Source.Server);
                    request = request.Trim('\uFEFF');
                    
                    var tcpRequest = new TcpRequest(request);
                    _requestQueue.Enqueue(tcpRequest);
                    
                    Response response = await ProcessRequestAsync(tcpRequest);
                    string responseJson = JsonSerializer.Serialize(response);
                    await writer.WriteLineAsync(responseJson);
                    Logger.Log($"Response sent to client: {response.Status} | {response.Message}", LogLevel.Information, Source.Server);

                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error handling client: {ex.Message}", LogLevel.Error, Source.Server);
            }
            finally
            {
                _connectionSemaphore.Release();
            }
        }
    }

    private async Task<Response> ProcessRequestAsync(TcpRequest tcpRequest)
    {
        try
        {
            Request? request = Request.Deserialize(tcpRequest.Message);
            if (request == null)
            {
                Logger.Log("Invalid request received.", LogLevel.Warning, Source.Server);
                return null;
            }

            if (request.PayLoad.IsEncrypted)
            {
                request = await _requestExecutor.DecryptRequest(request);
            }
            Logger.Log($"Received request of type: {request.RequestType}", LogLevel.Information, Source.Server);

            return await _controller.HandleRequestAsync(request);
        }
        catch (Exception ex)
        {
            Logger.Log($"Error processing request: {ex.Message}", LogLevel.Error, Source.Server);
            return null;
        }
    }

    private async Task ProcessRequestQueueAsync()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            while (_requestQueue.TryDequeue(out var request))
            {
                await ProcessRequestAsync(request);
            }
            await Task.Delay(100);
        }
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _tcpListener.Stop();
        Logger.Log("Server stopped", LogLevel.Information, Source.Server);
    }
}