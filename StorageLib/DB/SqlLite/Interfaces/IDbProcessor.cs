using StorageLib.DB.SqlLite;

namespace DataLib.DB.SqlLite.Interfaces;

public interface IDbProcessor
{
    Task<T> ProcessService<T>(IDbProvider dbProvider, CommandType commandType, DbData data = null);
    
}