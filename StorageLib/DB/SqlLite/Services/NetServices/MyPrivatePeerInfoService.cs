using DataLib.DB.SqlLite.Interfaces;
using Microsoft.EntityFrameworkCore;
using ModelsLib;
using ModelsLib.NetworkModels;
using StorageLib.DB.SqlLite;
using Utilities;

namespace DataLib.DB.SqlLite.Services.NetServices
{
    public class MyPrivatePeerInfoService : IDbProvider
    {
        private readonly AppDbContext _context;

        public MyPrivatePeerInfoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Exists(string id)
        {
            try
            {
                Logger.Log($"Checking if peer with NodeId {id} exists...", LogLevel.Information, Source.Server);
                bool exists = await _context.PeersInfo.AnyAsync(p => p.NodeId == id);
                Logger.Log($"Peer with NodeId {id} exists: {exists}", LogLevel.Information, Source.Server);
                return exists;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error checking existence of peer with NodeId {id}: {ex.Message}", LogLevel.Error, Source.Server);
                return false;
            }
        }

        public async Task<bool> Add(IModel model)
        {
            try
            {
                Logger.Log("Adding new peer info...", LogLevel.Information, Source.Server);
                var peerInfoModel = model as MyPrivatePeerInfoModel;
                if (peerInfoModel == null)
                {
                    Logger.Log("Invalid model type provided for Add method.", LogLevel.Error, Source.Server);
                    return false;
                }

                await _context.MyPrivatePeerInfo.AddAsync(peerInfoModel);
                await _context.SaveChangesAsync();
                Logger.Log("Peer info added successfully.", LogLevel.Information, Source.Server);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error adding peer info: {ex.Message}", LogLevel.Error, Source.Server);
                return false;
            }
        }

        public async Task<IModel> Get(string id)
        {
            try
            {
                Logger.Log($"Fetching peer info for NodeId {id}...", LogLevel.Information, Source.Server);
                var peerInfo = await _context.MyPrivatePeerInfo.FirstOrDefaultAsync();
                if (peerInfo == null)
                {
                    Logger.Log($"No peer info found for NodeId {id}.", LogLevel.Warning, Source.Server);
                    return null;
                }
                Logger.Log($"Peer info for NodeId {id} fetched successfully.", LogLevel.Information, Source.Server);
                return peerInfo;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error fetching peer info for NodeId {id}: {ex.Message}", LogLevel.Error, Source.Server);
                return null;
            }
        }

        public async Task<List<IModel>> GetAll()
        {
            try
            {
                Logger.Log("Fetching all peer info...", LogLevel.Information, Source.Server);
                var allPeers = await _context.MyPrivatePeerInfo.ToListAsync();
                Logger.Log($"Fetched {allPeers.Count} peers.", LogLevel.Information, Source.Server);
                return allPeers.Cast<IModel>().ToList();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error fetching all peer info: {ex.Message}", LogLevel.Error, Source.Server);
                return new List<IModel>();
            }
        }

        public async Task<bool> Delete(string id)
        {
            try
            {
                Logger.Log($"Deleting peer info for NodeId {id}...", LogLevel.Information, Source.Server);
                var peerInfo = await _context.MyPrivatePeerInfo.FirstOrDefaultAsync(p => p.NodeId == id);
                if (peerInfo == null)
                {
                    Logger.Log($"Peer info for NodeId {id} not found.", LogLevel.Warning, Source.Server);
                    return false;
                }

                _context.MyPrivatePeerInfo.Remove(peerInfo);
                await _context.SaveChangesAsync();
                Logger.Log($"Peer info for NodeId {id} deleted successfully.", LogLevel.Information, Source.Server);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error deleting peer info for NodeId {id}: {ex.Message}", LogLevel.Error, Source.Server);
                return false;
            }
        }

        public async Task<bool> Update(string id, IModel model)
        {
            try
            {
                Logger.Log($"Updating peer info for NodeId {id}...", LogLevel.Information, Source.Server);
                var peerInfoModel = model as MyPrivatePeerInfoModel;
                if (peerInfoModel == null)
                {
                    Logger.Log("Invalid model type provided for Update method.", LogLevel.Error, Source.Server);
                    return false;
                }

                var existingPeerInfo = await _context.PeersInfo.FindAsync(id);
                if (existingPeerInfo == null)
                {
                    Logger.Log($"No existing peer info found for NodeId {id}.", LogLevel.Warning, Source.Server);
                    return false;
                }

                // Обновление свойств существующей записи
                // existingPeerInfo.Property1 = peerInfoModel.Property1;
                // existingPeerInfo.Property2 = peerInfoModel.Property2;
                // Добавьте остальные свойства, которые нужно обновить

                _context.PeersInfo.Update(existingPeerInfo);
                await _context.SaveChangesAsync();
                Logger.Log($"Peer info for NodeId {id} updated successfully.", LogLevel.Information, Source.Server);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error updating peer info for NodeId {id}: {ex.Message}", LogLevel.Error, Source.Server);
                return false;
            }
        }
    }
}
