using System.Linq;
using System.Net.Sockets;
using DataLib.DB.SqlLite.Interfaces;
using DataLib.DB.SqlLite.Services.NetServices;
using EnumsLib;
using ModelsLib;
using ModelsLib.NetworkModels;
using PeerLib.Services;
using StorageLib.DB.SqlLite.Services;
using StorageLib.DB.SqlLite;
using Utilities;

namespace Client;

public class ConnectionManager
{
    private readonly List<PeerInfoModel> _peers;
    private readonly Dictionary<string, TcpClient> _activeConnections = new();
    private readonly IDbProcessor _dbProcessor;
    private readonly AppDbContext _appDbContext;

    public ConnectionManager()
    {
         _peers = new List<PeerInfoModel>();
         _appDbContext = new AppDbContext();
         _dbProcessor = new DbProcessor();
    }

    /// <summary>
    /// Асинхронно инициализирует список известных узлов.
    /// </summary>
    public async Task InitializePeersAsync()
    {
        Logger.Log("Initializing known peers...", LogLevel.Information, Source.Client);
        List<IModel> models = await _dbProcessor.ProcessService<List<IModel>>(new PeerInfoService(_appDbContext), CommandType.GetAll);
        List<PeerInfoModel> peers = models.Cast<PeerInfoModel>().ToList();
        lock (_peers)
        {
            _peers.Clear();
            _peers.AddRange(peers);
        }
        Logger.Log($"Loaded {_peers.Count} peers from database.", LogLevel.Information, Source.Client);
    }

    /// <summary>
    /// Возвращает список известных узлов.
    /// </summary>
    public List<PeerInfoModel> GetPeersList()
    {
        lock (_peers)
        {
            return new List<PeerInfoModel>(_peers);
        }
    }

    /// <summary>
    /// Возвращает список активных узлов.
    /// </summary>
    public List<PeerInfoModel> GetActivePeersList()
    {
        lock (_peers)
        {
            var activePeers = _peers.Where(peer => peer.Status == NodeStatus.Active).ToList();
            Logger.Log($"Found {activePeers.Count} active peers.", LogLevel.Information, Source.Client);
            return activePeers;
        }
    }
    
    public void RegisterConnection(string address, int port, TcpClient tcpClient)
    {
        string key = $"{address}:{port}";
        lock (_activeConnections)
        {
            if (!_activeConnections.ContainsKey(key))
            {
                _activeConnections[key] = tcpClient;
                Logger.Log($"Connection registered: {key}", LogLevel.Information, Source.Client);
            }
        }
    }

    /// <summary>
    /// Обновляет статус узла.
    /// </summary>
    public void UpdatePeerStatus(string address, NodeStatus newStatus)
    {
        lock (_peers)
        {
            var peer = _peers.FirstOrDefault(p => p.IpAddress == address);
            if (peer != null)
            {
                peer.Status = newStatus;
                Logger.Log($"Updated peer {address} to status {newStatus}.", LogLevel.Information, Source.Client);
            }
            else
            {
                Logger.Log($"Peer {address} not found for status update.", LogLevel.Warning, Source.Client);
            }
        }
    }

    /// <summary>
    /// Проверяет и возвращает список свободных узлов для подключения.
    /// </summary>
    public List<PeerInfoModel> GetAvailablePeersForConnection(int maxConnectionsPerPeer)
    {
        lock (_peers)
        {
            var availablePeers = _peers
                .Where(peer => peer.Status != NodeStatus.Inactive)
                .Take(maxConnectionsPerPeer)
                .ToList();

            Logger.Log($"Found {availablePeers.Count} available peers for connection.", LogLevel.Information, Source.Client);
            return availablePeers;
        }
    }

    /// <summary>
    /// Логирует список всех известных узлов.
    /// </summary>
    public void LogPeers()
    {
        lock (_peers)
        {
            foreach (var peer in _peers)
            {
                Logger.Log(
                    $"Peer: {peer.IpAddress}:{peer.Port}, Status: {peer.Status}",
                    LogLevel.Information,
                    Source.Client
                );
            }
        }
    }
    
    public void CloseAllConnections()
    {
        lock (_activeConnections)
        {
            foreach (var connection in _activeConnections)
            {
                try
                {
                    connection.Value.Close();
                    Logger.Log($"Connection to {connection.Key} closed.", LogLevel.Information, Source.Client);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error closing connection to {connection.Key}: {ex.Message}", LogLevel.Error, Source.Client);
                }
            }
            _activeConnections.Clear();
            Logger.Log("All active connections have been closed.", LogLevel.Information, Source.Client);
        }
    }


}
