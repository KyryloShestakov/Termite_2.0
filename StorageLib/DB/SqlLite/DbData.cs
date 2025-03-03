using ModelsLib;

namespace StorageLib.DB.SqlLite;

public class DbData
{
    public string Id { get; set; } = null;
    public IModel Model {get;set;} = null;

    public DbData(IModel model = null, string id = null)
    {
        Id = id;
        Model = model;
    }
}