using ModelsLib.NetworkModels;
using RRLib;
using RRLib.Requests.NetRequests;
using RRLib.Responses;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services;

namespace PeerLib.Services;

public class InfoPeerService
{
    private readonly PeerInfoService _peerInfoService;

    public InfoPeerService()
    {
        var context = new AppDbContext();
        context.Database.EnsureCreated();
        _peerInfoService = new PeerInfoService(context);
    }

    // CREATE: Добавить информацию о новом узле
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

        await _peerInfoService.AddPeerInfoAsync(peerInfoModel);

        return new ServerResponseService().GetResponse(true, "New information about peer added successfully.");
    }

    public async Task<Response> GetPeers()
    {
        var peers = await _peerInfoService.GetAllPeerInfoAsync();
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

        var peer = await _peerInfoService.GetPeerInfoByIdAsync(peerInfoRequest.GetPeerInfo().NodeId);
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
        var peer = await _peerInfoService.GetPeerInfoByIdAsync(updatedData.NodeId);

        if (peer == null)
        {
            return new ServerResponseService().GetResponse(false, "Peer not found.");
        }

        peer.IpAddress = updatedData.IpAddress;
        peer.Port = updatedData.Port;
        peer.LastSeen = updatedData.LastSeen;
        peer.NodeType = updatedData.NodeType;
        peer.Status = updatedData.Status;
        peer.SoftwareVersion = updatedData.SoftwareVersion;

        await _peerInfoService.UpdatePeerInfoAsync(peer);

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
        var peer = await _peerInfoService.GetPeerInfoByIdAsync(data.NodeId);

        if (peer == null)
        {
            return new ServerResponseService().GetResponse(false, "Peer not found.");
        }

        await _peerInfoService.DeletePeerInfoAsync(peer.NodeId);

        return new ServerResponseService().GetResponse(true, "Peer deleted successfully.");
    }
}
