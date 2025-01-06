using ModelsLib.NetworkModels;
using StorageLib.DB.SqlLite.Services;
using RRLib;
using RRLib.Requests.NetRequests;
using RRLib.Responses;
using StorageLib.DB.SqlLite;

namespace PeerLib.Services;

public class KnownPeersService
{
    private PeersListService _peersListService;
    
    // Constructor that initializes the PeersListService and ensures the database is created
    public KnownPeersService()
    {
        var context = new AppDbContext();
        context.Database.EnsureCreated(); // Ensures that the database is created if it doesn't exist
        _peersListService = new PeersListService(context);
    }

    // Method for adding new known peers
    public async Task<Response> PostKnownPeers(Request request)
    {
        KnownPeersRequest knownPeersRequest = request as KnownPeersRequest;
        List<KnownPeersModel> knownPeers = knownPeersRequest.GetKnownPeers();
        
        // Iterate through the list of known peers and add them to the database
        foreach (KnownPeersModel knownPeer in knownPeers)
        { 
            await _peersListService.AddPeerAsync(knownPeer);
        }
        
        // Return a successful response after adding the peers
        var response = new ServerResponseService().GetResponse(true, "New known peers added successfully.");
        return response;
    }

    // Method for getting a list of known peers
    public async Task<Response> GetKnownPeers(Request request)
    {
        // Retrieve all peers from the database
        var peersList = await _peersListService.GetAllPeersAsync();
        
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
        List<KnownPeersModel> updatedPeers = knownPeersRequest.GetKnownPeers();
        
        // Iterate through the list of updated peers and either update or add them
        foreach (KnownPeersModel updatedPeer in updatedPeers)
        {
            var existingPeer = await _peersListService.GetPeerByIdAsync(updatedPeer.Id);
            if (existingPeer != null)
            {
                // If the peer exists, update its details
                existingPeer.Id = updatedPeer.Id; // Example: updating only the name, but can include other fields
                existingPeer.Address = updatedPeer.Address;
                await _peersListService.UpdatePeerAsync(existingPeer);
            }
            else
            {
                // If the peer doesn't exist, add it to the database
                await _peersListService.AddPeerAsync(updatedPeer);
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
        List<KnownPeersModel> peersToDelete = knownPeersRequest.GetKnownPeers();
        
        // Iterate through the list of peers to be deleted and remove them from the database
        foreach (KnownPeersModel peer in peersToDelete)
        {
            var existingPeer = await _peersListService.GetPeerByIdAsync(peer.Id);
            if (existingPeer != null)
            {
                // If the peer exists, delete it from the database
                await _peersListService.DeletePeerAsync(existingPeer.Id);
            }
        }
        
        // Return a successful response after deleting the peers
        var response = new ServerResponseService().GetResponse(true, "Known peers deleted successfully.");
        return response;
    }
}
