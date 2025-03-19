using DataLib.DB.SqlLite.Interfaces;
using DataLib.DB.SqlLite.Services;
using DataLib.DB.SqlLite.Services.NetServices;
using ModelsLib.NetworkModels;
using RRLib.Responses;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services;
using Ter_Protocol_Lib.Requests;
using Utilities;

namespace PeerLib.Services;

public class InfoPeerService
{
    private readonly PeerInfoService _peerInfoService;
    private readonly IDbProcessor _dbProcessor;
    public InfoPeerService()
    {
        var context = new AppDbContext();
        context.Database.EnsureCreated();
        _peerInfoService = new PeerInfoService(context);
        _dbProcessor = new DbProcessor();
    }

    public async Task<Response> PostPeerInfo(TerProtocol<PeerInfoRequest> request)
    {

        var data = request.Payload.Data.Peers.First();
        var peerInfoModel = new PeerInfoModel
        {
            NodeId = data.NodeId,
            IpAddress = data.IpAddress,
            Port = data.Port,
            LastSeen = data.LastSeen,
            NodeType = data.NodeType,
            Status = data.Status,
            SoftwareVersion = data.SoftwareVersion
        };

        var result = await _dbProcessor.ProcessService<bool>(new PeerInfoService(new AppDbContext()), CommandType.Add, new DbData(peerInfoModel));
       
        return new ServerResponseService().GetResponse(true, $"New information about peer added successfully.{result}");
    }

    public async Task<Response> GetPeers()
    {
        var peers = await _peerInfoService.GetAll();
        if (peers == null || !peers.Any())
        {
            return new ServerResponseService().GetResponse(false, "No known peers found.");
        }

        return new ServerResponseService().GetResponse(true, "Peers retrieved successfully.", peers);
    }

    public async Task<Response> GetPeerById(TerProtocol<DataRequest<string>> request)
    {
        var peer = await _peerInfoService.Get(request.Payload.Data.Value);
        if (peer == null)
        {
            return new ServerResponseService().GetResponse(false, "Peer not found.");
        }

        return new ServerResponseService().GetResponse(true, "Peer retrieved successfully.", peer);
    }

    public async Task<Response> UpdatePeer(TerProtocol<PeerInfoRequest> request)
    {
        try
        {
            List<PeerInfoModel> updatedData = request.Payload.Data.Peers;

            var peers = await _peerInfoService.GetAll();
            List<PeerInfoModel> peersList = peers.Cast<PeerInfoModel>().ToList();
            if (peersList == null || !peersList.Any())
            {
                return new ServerResponseService().GetResponse(false, "Peer not found.");
            }

            var updatedPeers = peersList
                .Select(peer => new PeerInfoModel
                {
                    NodeId = peer.NodeId,
                    IpAddress = updatedData.First().IpAddress,
                    Port = updatedData.First().Port,
                    LastSeen = updatedData.First().LastSeen,
                    NodeType = updatedData.First().NodeType,
                    Status = updatedData.First().Status,
                    SoftwareVersion = updatedData.First().SoftwareVersion
                })
                .ToList();

            await Task.WhenAll(updatedPeers.Select(peer => _peerInfoService.Update(peer.NodeId, peer)));

            return new ServerResponseService().GetResponse(true, "Peers updated successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    
    public async Task<Response> DeletePeer(TerProtocol<DataRequest<string>> request)
    {
        var data = request.Payload.Data.Value;
        var peer = await _peerInfoService.Get(data);
        PeerInfoModel peerInfoModel = peer as PeerInfoModel;
        
        if (peer == null)
        {
            return new ServerResponseService().GetResponse(false, "Peer not found.");
        }
        await _dbProcessor.ProcessService<string>(new PeerInfoService(new AppDbContext()), CommandType.Delete, new DbData(null, peerInfoModel.NodeId));

        return new ServerResponseService().GetResponse(true, "Peer deleted successfully.");
    }
}
