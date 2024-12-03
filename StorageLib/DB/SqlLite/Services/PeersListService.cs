using Microsoft.EntityFrameworkCore;
using ModelsLib.NetworkModels;
using Utilities;

namespace StorageLib.DB.SqlLite.Services;

public class PeersListService
{
    private readonly AppDbContext _context;

    public PeersListService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task AddPeerAsync(KnownPeersModel knownPeerModel)
    {
        _context.PeersList.Add(knownPeerModel);
        await _context.SaveChangesAsync();
        Logger.Log($"Add Peer to DB | id:{knownPeerModel.Id}", LogLevel.Information, Source.Storage);
    }
    
    public async Task<KnownPeersModel> GetPeerByIdAsync(int id)
    {
        return await _context.PeersList.FindAsync(id);
    }
    
    public async Task<List<KnownPeersModel>> GetAllPeersAsync()
    {
        return await _context.PeersList.ToListAsync();
    }
    
    public async Task UpdatePeerAsync(KnownPeersModel knownPeerModel)
    {
        _context.PeersList.Update(knownPeerModel);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeletePeerAsync(int id)
    {
        var peer = await _context.PeersList.FindAsync(id);
        if (peer != null)
        {
            _context.PeersList.Remove(peer);
            await _context.SaveChangesAsync();
        }
    }
}