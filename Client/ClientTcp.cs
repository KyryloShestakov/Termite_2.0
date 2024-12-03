using System.Net.Sockets;
using ModelsLib.NetworkModels;
using Utilities;

namespace Client;

public class ClientTcp
{
    private readonly string _serverIp;
    private readonly int _serverPort;
    
    private DataSynchronizer _dataSynchronizer;
    private ConnectionManager _connectionManager;

    public ClientTcp(string serverIp, int serverPort)
    {
        _serverIp = serverIp;
        _serverPort = serverPort;
        
        _connectionManager = new ConnectionManager();
    }

    public async Task RunAsync()
    {
        using (TcpClient tcpClient = new TcpClient())
        {
            try
            {   //TODO мне надо перебирать список все реализованных узлов и подключатся к ним ожновременно я подключаюсь к 5 узлам но после всей передачи информации они могут подкл.чться к другим
                List<KnownPeersModel> peersList = _connectionManager.GetPeersList();

                //TODO надо написать метод под это
                foreach (var VARIABLE in peersList)
                {
                    
                }
                await tcpClient.ConnectAsync(_serverIp, _serverPort);
                Logger.Log($"Client connected to server {_serverIp}:{_serverPort}", LogLevel.Information, Source.Client);
                _dataSynchronizer = new DataSynchronizer(tcpClient);
            }
            catch (SocketException ex)
            {
                Logger.Log($"Network error while connecting to {_serverIp}:{_serverPort} - {ex.Message}", LogLevel.Error, Source.Client);
            }
            catch (IOException ex)
            {
                Logger.Log($"I/O error: {ex.Message}", LogLevel.Error, Source.Client);
            }
            catch (Exception ex)
            {
                Logger.Log($"Unexpected error: {ex.Message}", LogLevel.Error, Source.Client);
                throw;
            }
        }
    }

}