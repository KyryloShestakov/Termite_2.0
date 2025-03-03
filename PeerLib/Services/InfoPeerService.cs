using DataLib.DB.SqlLite.Interfaces;
using DataLib.DB.SqlLite.Services;
using DataLib.DB.SqlLite.Services.NetServices;
using ModelsLib.NetworkModels;
using RRLib;
using RRLib.Requests.NetRequests;
using RRLib.Responses;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services;
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

    public async Task<Response> PostPeerInfo(Request request)
    {
        if (request is not PeerInfoRequest peerInfoRequest)
        {
            return new ServerResponseService().GetResponse(false, "Invalid request type.");
        }

        var data = peerInfoRequest.GetPeerInfo();
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

      //  await _peerInfoService.AddPeerInfoAsync(peerInfoModel);
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

    public async Task<Response> GetPeerById(Request request)
    {
        if (request is not PeerInfoRequest peerInfoRequest)
        {
            return new ServerResponseService().GetResponse(false, "Invalid request type.");
        }

        var peer = await _peerInfoService.Get(peerInfoRequest.GetPeerInfo().NodeId);
        if (peer == null)
        {
            return new ServerResponseService().GetResponse(false, "Peer not found.");
        }

        return new ServerResponseService().GetResponse(true, "Peer retrieved successfully.", peer);
    }

    // UPDATE: Обновить информацию об узле
    public async Task<Response> UpdatePeer(Request request)
    {
        if (request is not PeerInfoRequest peerInfoRequest)
        {
            return new ServerResponseService().GetResponse(false, "Invalid request type.");
        }

        var updatedData = peerInfoRequest.GetPeerInfo();
        var peer = await _peerInfoService.Get(updatedData.NodeId);
        
        PeerInfoModel peerInfoModel = peer as PeerInfoModel;
        
        if (peer == null)
        {
            return new ServerResponseService().GetResponse(false, "Peer not found.");
        }

        peerInfoModel.IpAddress = updatedData.IpAddress;
        peerInfoModel.Port = updatedData.Port;
        peerInfoModel.LastSeen = updatedData.LastSeen;
        peerInfoModel.NodeType = updatedData.NodeType;
        peerInfoModel.Status = updatedData.Status;
        peerInfoModel.SoftwareVersion = updatedData.SoftwareVersion;

        await _peerInfoService.Update(peerInfoModel.NodeId,peerInfoModel);

        return new ServerResponseService().GetResponse(true, "Peer updated successfully.");
    }

    // DELETE: Удалить информацию об узле
    public async Task<Response> DeletePeer(Request request)
    {
        if (request is not PeerInfoRequest peerInfoRequest)
        {
            return new ServerResponseService().GetResponse(false, "Invalid request type.");
        }

        var data = peerInfoRequest.GetPeerInfo();
        var peer = await _peerInfoService.Get(data.NodeId);
        PeerInfoModel peerInfoModel = peer as PeerInfoModel;
        
        if (peer == null)
        {
            return new ServerResponseService().GetResponse(false, "Peer not found.");
        }

        await _peerInfoService.Delete(peerInfoModel.NodeId);

        return new ServerResponseService().GetResponse(true, "Peer deleted successfully.");
    }
}
