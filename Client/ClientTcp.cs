using System.Net.Sockets;
using DataLib.DB.SqlLite.Interfaces;
using ModelsLib.NetworkModels;
using StorageLib.DB.SqlLite;
using Utilities;

namespace Client;

public class ClientTcp
{
    private DataSynchronizer _dataSynchronizer;
    private ConnectionManager _connectionManager;
    private IDbProcessor dbProcessor;
    private AppDbContext _appDbContext;

    public ClientTcp()
    {
        _appDbContext = new AppDbContext();
        dbProcessor = new DbProcessor();
        _connectionManager = new ConnectionManager();
        _dataSynchronizer = new DataSynchronizer(dbProcessor, _appDbContext);
    }

    public async Task RunAsync()
    {
        try
        {
            Logger.Log("Client is up and running", LogLevel.Information, Source.Client);
            await _connectionManager.InitializePeersAsync();
            List<PeerInfoModel> peersList = _connectionManager.GetActivePeersList();

            if (peersList == null || peersList.Count == 0)
            {
                Logger.Log("No active peers available.", LogLevel.Warning, Source.Client);
                return;
            }
            
            var tasks = peersList.Select(async knownPeer =>
            {
                using (TcpClient tcpClient = new TcpClient())
                {
                    try
                    {
                        string ip = knownPeer.IpAddress;
                        int port = int.Parse(knownPeer.Port.ToString());
                        await tcpClient.ConnectAsync(ip, port);
                        _connectionManager.RegisterConnection(knownPeer.IpAddress, port, tcpClient);
            
                        Logger.Log($"Client connected to server {knownPeer.IpAddress}:{knownPeer.Port}", LogLevel.Information, Source.Client);
                       
                        await _dataSynchronizer.StartSynchronization(tcpClient, knownPeer);
                        
                        
                    }
                    catch (SocketException ex)
                    {
                        Logger.Log($"Network error with {knownPeer.IpAddress}:{knownPeer.Port} - {ex.Message}", LogLevel.Error, Source.Client);
                    }
                    catch (IOException ex)
                    {
                        Logger.Log($"I/O error with {knownPeer.IpAddress}:{knownPeer.Port} - {ex.Message}", LogLevel.Error, Source.Client);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Unexpected error with {knownPeer.IpAddress}:{knownPeer.Port} - {ex.Message}", LogLevel.Error, Source.Client);
                    }
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
