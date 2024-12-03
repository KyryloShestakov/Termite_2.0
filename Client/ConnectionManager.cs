using ModelsLib.NetworkModels;
using StorageLib.DB.SqlLite.Services;
using StorageLib.DB.SqlLite;

namespace Client;

public class ConnectionManager
{
    private List<KnownPeersModel> _peers;
    private AppDbContext _appDbContext;
    private PeersListService _peersService;

    public ConnectionManager()
    {
        _peers = new List<KnownPeersModel>();
        _peersService = new PeersListService(_appDbContext);
        
        //TODO надо сделать опять же чтоб раз в какое то время обновлялся список
        
        while (true)
        {
            AddPeer();
        }
    }

    private void AddPeer()
    {
        //TODO Надо написать нормальный метод
        Task<List<KnownPeersModel>> KnownPeers = _peersService.GetAllPeersAsync();
        foreach (var knownPeer in KnownPeers.Result)
        {
           _peers.Add(knownPeer); 
        }
        
    }

    public List<KnownPeersModel> GetPeersList()
    {
        return _peers;
    }
    
    //TODO я должен опредялять ко скольким узлам я подключен и какой у низ статус если к одному из этих узлов уже подключены 5 узлов 
    //TODO я должен перебирать свободные узлы
}