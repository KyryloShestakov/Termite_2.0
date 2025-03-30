using DataLib.DB.SqlLite.Interfaces;
using DataLib.DB.SqlLite.Services;
using ModelsLib;
using IModel = Microsoft.EntityFrameworkCore.Metadata.IModel;

namespace StorageLib.DB.SqlLite;

public class DbProcessor : IDbProcessor
{
    
    public async Task<T> ProcessService<T>(IDbProvider dbProvider, CommandType commandType, DbData data)
    {
        if (dbProvider == null)
        {
            throw new ArgumentNullException(nameof(dbProvider), "dbProvider is null");
        }
        
        object result;
        switch (commandType)
        {
            case CommandType.Get:
                result = await dbProvider.Get(data.Id);
                break;
            case CommandType.GetAll:
                result =await dbProvider.GetAll();
                break;
            case CommandType.Add:
                result = await dbProvider.Add(data.Model);
                break;
            case CommandType.Update:
                result = await dbProvider.Update(data.Id, data.Model);
                break;
            case CommandType.Delete:
                result = await dbProvider.Delete(data.Id);
                break;
            default:
                throw new ArgumentException("Unsupported command type", nameof(commandType));
            
        }
        if (result is T typedResult)
        {
            return typedResult;
        }
        throw new InvalidCastException($"Unable to cast object of type {result.GetType()} to type {typeof(T)}.");

        
    }
}

