using System.Net;
using System.Net.Sockets;
using System.Text;
using Server.Executors;
using Server.Requests;
using Utilities;


namespace Server;

public class TcpServer
{
    private TcpListener _tcpListener;
    private bool _isRunning;
    
    RequestExecutor _requestExecutor;

    public TcpServer()
    {
        _tcpListener = new TcpListener(IPAddress.Any, 8080);
    }

    public async Task StartAsync()
    {
        _tcpListener.Start();
        _isRunning = true;
        Logger.Log("Server started...", LogLevel.Information, Source.Server);
        while (_isRunning)
        {
            TcpClient tcpClient = await _tcpListener.AcceptTcpClientAsync();
            Logger.Log(tcpClient, LogLevel.Information, Source.Server);

            _ = ProcessClientAsync(tcpClient);


        }
    }

    private async Task ProcessClientAsync(TcpClient tcpClient)
    {
        Logger.Log("Processing client started",LogLevel.Information, Source.Server);
        try
        {
            if (!tcpClient.Connected) Logger.Log("Client is not connected", LogLevel.Information, Source.Server);
            TcpHandler handler = new TcpHandler(tcpClient);

            while (!handler.IsDisposed)
            {
                try
                {
                    TcpRequest request = await handler.AwaitRequestAsync();
                    Logger.Log($"Sender is {tcpClient.Client.RemoteEndPoint} | {request.ToString()}", LogLevel.Information, Source.Server);
                    
                    await ProcessRequestAsync(tcpClient, handler, request);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error while processing request: {ex.Message}", LogLevel.Error, Source.Server);
                    break;
                }
            }

            Logger.Log("Processing client finished ", LogLevel.Information, Source.Server);
        }
        catch (Exception ex){ Logger.Log($"Error in ProcessClientAsync {ex.Message}", LogLevel.Error, Source.Server); ;}
        finally{tcpClient.Close(); Logger.Log("Client connection closed", LogLevel.Information, Source.Server);}
    }

    private async Task ProcessRequestAsync(TcpClient tcpClient, TcpHandler handler, TcpRequest request)
    {
        Logger.Log("Processing request started", LogLevel.Information, Source.Server);
        _requestExecutor = new RequestExecutor(tcpClient, handler, request);
    }
    
    public void Stop()
    {
        _isRunning = false;
        _tcpListener.Stop();
        Logger.Log("Server stopped", LogLevel.Information, Source.Server);
    }

    
}