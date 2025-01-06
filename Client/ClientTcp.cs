using System.Net.Sockets;
using ModelsLib.NetworkModels;
using Utilities;

namespace Client;

public class ClientTcp
{
    private DataSynchronizer _dataSynchronizer;
    private ConnectionManager _connectionManager;

    public ClientTcp()
    {
        _connectionManager = new ConnectionManager();
        _dataSynchronizer = new DataSynchronizer();
    }

    public async Task RunAsync()
    {
        try
        {
            Logger.Log("Client is up and running", LogLevel.Information, Source.Client);
            await _connectionManager.InitializePeersAsync();
            List<KnownPeersModel> peersList = _connectionManager.GetActivePeersList();

            if (peersList == null || peersList.Count == 0)
            {
                Logger.Log("No active peers available.", LogLevel.Warning, Source.Client);
                return;
            }

            var tasks = peersList.Select(async knownPeer =>
            {
                TcpClient tcpClient = new TcpClient();
                    try
                    {
                        await tcpClient.ConnectAsync(knownPeer.Address, knownPeer.Port);
                        _connectionManager.RegisterConnection(knownPeer.Address, knownPeer.Port, tcpClient);

                        Logger.Log($"Client connected to server {knownPeer.Address}:{knownPeer.Port}", LogLevel.Information, Source.Client);

                        tcpClient.ReceiveTimeout = 5000;
                        tcpClient.SendTimeout = 5000;
                        
                        await _dataSynchronizer.StartSynchronization(tcpClient);
                        
                        
                    }
                    catch (SocketException ex)
                    {
                        Logger.Log($"Network error with {knownPeer.Address}:{knownPeer.Port} - {ex.Message}", LogLevel.Error, Source.Client);
                    }
                    catch (IOException ex)
                    {
                        Logger.Log($"I/O error with {knownPeer.Address}:{knownPeer.Port} - {ex.Message}", LogLevel.Error, Source.Client);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Unexpected error with {knownPeer.Address}:{knownPeer.Port} - {ex.Message}", LogLevel.Error, Source.Client);
                    }
            });

             await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            Logger.Log($"General error during client execution: {ex.Message}", LogLevel.Error, Source.Client);
            throw;
        }
    }

    public async Task StopAsync()
    {
        _connectionManager.CloseAllConnections();
    }

}
