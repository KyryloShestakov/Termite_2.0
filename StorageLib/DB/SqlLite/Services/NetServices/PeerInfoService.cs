using DataLib.DB.SqlLite.Interfaces;
using Microsoft.EntityFrameworkCore;
using ModelsLib;
using ModelsLib.NetworkModels;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services;

namespace DataLib.DB.SqlLite.Services.NetServices
{
    public class PeerInfoService : IDbProvider
    {
        private readonly AppDbContext _context;

        public PeerInfoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Exists(string peerId)
        {
            return await _context.PeersInfo.AnyAsync(p => p.NodeId == peerId);
        }

        public async Task<bool> Add(IModel model)
        {
            var peerInfo = model as PeerInfoModel;
            
            await _context.PeersInfo.AddAsync(peerInfo);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<IModel> Get(string id)
        {
            var peerInfoModel = await _context.PeersInfo
                .FirstOrDefaultAsync(p => p.NodeId == id); 
            var model = peerInfoModel as IModel;
            return model;
        }

        public async Task<List<IModel>> GetAll()
        {
            return await _context.PeersInfo.Cast<IModel>().ToListAsync();
        }


        public async Task<bool> Delete(string id)
        {
            var peerInfo = await _context.PeersInfo.FindAsync(id);
            if (peerInfo == null) return false;

            _context.PeersInfo.Remove(peerInfo);
            await _context.SaveChangesAsync();
            
            return true;
        }
        
        public async Task<bool> Update(string id, IModel model)
        {
            var existingPeerInfo = await _context.PeersInfo.FindAsync(id);
            if (existingPeerInfo == null) return false;

            // Обновление свойств существующей записи
            //existingPeerInfo.Property1 = updatedPeerInfo.Property1;
            //existingPeerInfo.Property2 = updatedPeerInfo.Property2;
            // Добавьте остальные свойства, которые нужно обновить

            _context.PeersInfo.Update(existingPeerInfo);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
