using System.Runtime.Serialization;
using ModelsLib;

namespace DataLib.DB.SqlLite.Interfaces;

public interface IDbProvider
{
    Task<bool> Add(IModel model);
    Task<IModel> Get(string id); 
    Task<List<IModel>> GetAll();
    Task<bool> Delete(string id);
    Task<bool> Update(string id, IModel model);
    Task<bool> Exists(string id);
    
}
