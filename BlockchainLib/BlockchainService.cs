using ModelsLib;
using ModelsLib.BlockchainLib;
using RRLib.Responses;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;
using Ter_Protocol_Lib.Requests;
using Utilities;

namespace BlockchainLib;

public class BlockchainService
{
    private readonly DbProcessor _dbProcessor;
    private readonly ServerResponseService _serverResponseService;

    public BlockchainService()
    {
        _dbProcessor = new DbProcessor();
        _serverResponseService = new ServerResponseService();
    }

    public async Task<Response> GetBlockchainInfo(TerProtocol<object> terTxProtocol)
    {
        Logger.Log("GetBlockchainInfo", LogLevel.Warning, Source.Blockchain);
       List<IModel> modelsBl = await _dbProcessor.ProcessService<List<IModel>>(new BlocksBdService(new AppDbContext()), CommandType.GetAll, new DbData());
       List<BlockModel> blocks = modelsBl.Cast<BlockModel>().ToList();
       
       List<IModel> modelsTx = await _dbProcessor.ProcessService<List<IModel>>(new TransactionBdService(new AppDbContext()), CommandType.GetAll, new DbData());
       List<TransactionModel> transactions = modelsTx.Cast<TransactionModel>().ToList();
       
       InfoSyncRequest infoSyncRequest = new InfoSyncRequest();
       infoSyncRequest.BlocksCount = modelsBl.Count;
       infoSyncRequest.LastBlockHash = blocks.LastOrDefault().Hash;
       infoSyncRequest.TimeOfLastBlock = blocks.LastOrDefault().Timestamp;
       infoSyncRequest.CountOfTransactions = transactions.Count;
       infoSyncRequest.TransactionIds = transactions.Select(x => x.Id).ToList();
       return _serverResponseService.GetResponse(true, "Info", infoSyncRequest);
    }
}