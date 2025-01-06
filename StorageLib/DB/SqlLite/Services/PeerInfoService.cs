using Microsoft.EntityFrameworkCore;
using ModelsLib.NetworkModels;

namespace StorageLib.DB.SqlLite.Services
{
    public class PeerInfoService
    {
        private readonly AppDbContext _context;

        public PeerInfoService(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task AddPeerInfoAsync(PeerInfoModel peerInfo)
        {
            await _context.PeersInfo.AddAsync(peerInfo);
            await _context.SaveChangesAsync();
        }
        
        public async Task<List<PeerInfoModel>> GetAllPeerInfoAsync()
        {
            return await _context.PeersInfo.ToListAsync();
        }
        
        public async Task<PeerInfoModel?> GetPeerInfoByIdAsync(string nodeId)
        {
            return await _context.PeersInfo.FindAsync(nodeId);
        }
        public async Task<PeerInfoModel?> GetMyPeerInfo()
        {
            return await _context.PeersInfo.FirstOrDefaultAsync();
        }
        
        
        public async Task<bool> DeletePeerInfoAsync(string nodeId)
        {
            var peerInfo = await _context.PeersInfo.FindAsync(nodeId);
            if (peerInfo == null) return false;

            _context.PeersInfo.Remove(peerInfo);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> UpdatePeerInfoAsync(PeerInfoModel updatedPeerInfo)
        {
            var existingPeerInfo = await _context.PeersInfo.FindAsync(updatedPeerInfo.Id);
            if (existingPeerInfo == null) return false;

            // Обновление свойств существующей записи
            //existingPeerInfo.Property1 = updatedPeerInfo.Property1;
            //existingPeerInfo.Property2 = updatedPeerInfo.Property2;
            // Добавьте остальные свойства, которые нужно обновить

            _context.PeersInfo.Update(existingPeerInfo);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.PeersInfo.AnyAsync(p => p.Id == id);
        }
    }
}
