using DataLib.DB.SqlLite.Services.NetServices;
using ModelsLib.NetworkModels;
using StorageLib.DB.SqlLite.Services;
using RRLib;
using RRLib.Requests.NetRequests;
using RRLib.Responses;
using StorageLib.DB.SqlLite;

namespace PeerLib.Services;

public class KnownPeersService
{
    private readonly PeerInfoService _peerInfoService;
    
    // Constructor that initializes the PeersListService and ensures the database is created
    public KnownPeersService()
    {
        _peerInfoService = new PeerInfoService(new AppDbContext());
    }

    // Method for adding new known peers
    public async Task<Response> PostKnownPeers(Request request)
    {
        KnownPeersRequest knownPeersRequest = request as KnownPeersRequest;
        List<PeerInfoModel> knownPeers = knownPeersRequest.GetKnownPeers();
        
        
        // Iterate through the list of known peers and add them to the database
        foreach (PeerInfoModel knownPeer in knownPeers)
        { 
            await _peerInfoService.Add(knownPeer);
        }
        
        // Return a successful response after adding the peers
        var response = new ServerResponseService().GetResponse(true, "New known peers added successfully.");
        return response;
    }

    // Method for getting a list of known peers
    public async Task<Response> GetKnownPeers(Request request)
    {
        // Retrieve all peers from the database
        var peersList = await _peerInfoService.GetAll();
        
        // If no peers are found, return a response indicating that
        if (peersList == null || !peersList.Any())
        {
            return new ServerResponseService().GetResponse(false, "No known peers found.");
        }
        
        // Return a successful response with the list of peers
        var response = new ServerResponseService().GetResponse(true, "Known peers retrieved successfully.", peersList);
        return response;
    }

    // Method for updating known peers
    public async Task<Response> UpdateKnownPeers(Request request)
    {
        KnownPeersRequest knownPeersRequest = request as KnownPeersRequest;
        List<PeerInfoModel> updatedPeers = knownPeersRequest.GetKnownPeers();
        
        // Iterate through the list of updated peers and either update or add them
        foreach (PeerInfoModel updatedPeer in updatedPeers)
        {
            var existingPeer = await _peerInfoService.Get(updatedPeer.NodeId);
            PeerInfoModel model = existingPeer as PeerInfoModel;
            if (existingPeer != null)
            {
                // If the peer exists, update its details
                model.Id = updatedPeer.Id; // Example: updating only the name, but can include other fields
                model.IpAddress = updatedPeer.IpAddress;
                await _peerInfoService.Update(model.NodeId,model);
            }
            else
            {
                // If the peer doesn't exist, add it to the database
                await _peerInfoService.Add(updatedPeer);
            }
        }
        
        // Return a successful response after updating the peers
        var response = new ServerResponseService().GetResponse(true, "Known peers updated successfully.");
        return response;
    }

    // Method for deleting known peers
    public async Task<Response> DeleteKnownPeers(Request request)
    {
        KnownPeersRequest knownPeersRequest = request as KnownPeersRequest;
        List<PeerInfoModel> peersToDelete = knownPeersRequest.GetKnownPeers();
        
        // Iterate through the list of peers to be deleted and remove them from the database
        foreach (PeerInfoModel peer in peersToDelete)
        {
            var existingPeer = await _peerInfoService.Get(peer.NodeId);
            PeerInfoModel model = existingPeer as PeerInfoModel;
            
            if (existingPeer != null)
            {
                // If the peer exists, delete it from the database
                await _peerInfoService.Delete(model.NodeId);
            }
        }
        
        // Return a successful response after deleting the peers
        var response = new ServerResponseService().GetResponse(true, "Known peers deleted successfully.");
        return response;
    }
}
