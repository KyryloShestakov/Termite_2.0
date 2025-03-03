using System.Diagnostics;
using System.IO.Pipes;
using System.Net.Sockets;
using Client.Synchronization;
using DataLib.DB.SqlLite.Interfaces;
using ModelsLib;
using StorageLib.DB.SqlLite.Services;
using Newtonsoft.Json;
using RRLib;
using SecurityLib.Security;
using StorageLib.DB.Redis;
using StorageLib.DB.SqlLite;
using Utilities;

namespace Client;

public class DataSynchronizer
{
   private TcpClient _tcpClient;
   private RequestPool _requestPool;
   private RequestFactory _requestFactory;
   private RequestExecutor _requestExecutor;
   
   
   public DataSynchronizer(IDbProcessor dbProcessor, AppDbContext appDbContext)
   {
      _requestPool = new RequestPool();
      _requestFactory = new RequestFactory(dbProcessor, appDbContext);
      _requestExecutor = new RequestExecutor();
   }

   public async Task StartSynchronization(TcpClient tcpClient, IModel peer)
   {
       try
       {
           Logger.Log("Starting synchronization...", LogLevel.Information, Source.Client);

           //await _requestFactory.CreateKeyExchageRequest(_requestPool, peer);
          // await _requestFactory.CreateMyPeerInfoRequest(_requestPool, peer);
            await _requestFactory.CreateTransactionRequest(_requestPool, peer);
           //await _requestFactory.CreateBlockRequest(_requestPool, peer);
           //Известные узлы
           
           await _requestExecutor.StartExecution(tcpClient, _requestPool);
           
           Logger.Log("Request execution finished.", LogLevel.Information, Source.Client);
       }
       catch (Exception e)
       {
           Logger.Log($"Error during synchronization: {e}", LogLevel.Error, Source.Client);
           throw;
       }
   }
}