using System.Net;
using System.Net.Sockets;
using System.Text;
using NetworkLibrary.Utilities;

namespace NetworkLibrary.Networking.Server;

public class TcpServer
{
    private TcpListener _tcpListener;
    private bool _isRunning;

    public TcpServer()
    {
        _tcpListener = new TcpListener(IPAddress.Any, 8080);
    }

    public async Task StartAsync()
    {
        _tcpListener.Start();
        _isRunning = true;
        Logger.Log("Server started");
        while (_isRunning)
        {
            TcpClient tcpClient = await _tcpListener.AcceptTcpClientAsync();
            Logger.Log(tcpClient);

            _ = ProcessClientAsync(tcpClient);


        }
    }

    private async Task ProcessClientAsync(TcpClient tcpClient)
    {
        Logger.Log("Processing client started");
        try
        {
            if (!tcpClient.Connected) Logger.Log("Client is not connected");
            TcpHandler handler = new TcpHandler(tcpClient);

            while (!handler.IsDisposed)
            {
                try
                {
                    TcpRequest request = await handler.AwaitRequestAsync();
                    Logger.Log(request.ToString());
                    
                    await ProcessRequestAsync(tcpClient, handler, request);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error while processing request: {ex.Message}");
                    break;
                }
            }

            Logger.Log("Processing cliet finished ");
        }
        catch (Exception ex){ Logger.Log($"Error in ProcessClientAsync {ex.Message}");}
        finally{tcpClient.Close(); Logger.Log("Client connection closed");}
    }

    private async Task ProcessRequestAsync(TcpClient tcpClient, TcpHandler handler, TcpRequest request)
    {
        
    }
    
    public void Stop()
    {
        _isRunning = false;
        _tcpListener.Stop();
        Logger.Log("Server stopped");
    }

    
}