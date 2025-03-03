using BlockchainLib.Addresses;
using Client;
using DataLib.DB.SqlLite.Interfaces;
using DataLib.DB.SqlLite.Services;
using DataLib.DB.SqlLite.Services.NetServices;
using EnumsLib;
using ModelsLib.NetworkModels;
using Server;
using StorageLib.DB.Redis;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services;
using Utilities;

namespace CoreLib;

public class Peer
{
    private AddressManager _addressManager;
    private IDbProcessor _dbProcessor;
    private readonly AppDbContext _appDbContext;
    
    public Peer()
    {
        _addressManager = new AddressManager();
        _appDbContext = new AppDbContext();
        _dbProcessor = new DbProcessor();
    }

    public async Task CreatePeer()
    {
        var response =  _addressManager.GenerateAddressWithKeys();

        string address =  response.Address;
        string privateKey = response.PrivateKey;
        string publicKey = response.PublicKey;
        
        var MyPeerInfo = new MyPrivatePeerInfoModel()
        {
            NodeId = Guid.NewGuid().ToString(),
            IpAddress = "192.168.17.25",
            Port = 8080,
            Status = NodeStatus.Active,
            SoftwareVersion = "0.1",
            NodeType = "1",
            LastSeen = DateTime.Now,
            AddressWallet = address,
            PrivateKey = privateKey,
            PublicKey = publicKey
        };
        
        var peerInfo = new PeerInfoModel()
        {
            NodeId = Guid.NewGuid().ToString(),
            IpAddress = "192.168.17.25",
            Port = 8080,
            Status = NodeStatus.Active,
            SoftwareVersion = "0.1",
            NodeType = "1",
            LastSeen = DateTime.Now,
        };
        
       
        await _dbProcessor.ProcessService<bool>(new MyPrivatePeerInfoService(_appDbContext), CommandType.Add, new DbData(MyPeerInfo));
        bool result = await _dbProcessor.ProcessService<bool>(new PeerInfoService(new AppDbContext()), CommandType.Add, new DbData(peerInfo));
        Logger.Log($"Prossec result : {result}");
        Logger.Log($"Created peer, id: {MyPeerInfo.NodeId}, status: {MyPeerInfo.Status} ", LogLevel.Information, Source.PeerCore);
    }

    public void Start()
    {
        while (true)
        {
            var server = new TcpServer();
            var serverTask = server.StartAsync();

            Logger.Log("Press Enter to stop the server...", LogLevel.Information, Source.Server);
            Console.ReadLine();

            server.Stop();
           
        }
        
    }
    
    

}