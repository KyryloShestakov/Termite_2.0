using DataLib.DB.SqlLite.Interfaces;
using DataLib.DB.SqlLite.Services;
using DataLib.DB.SqlLite.Services.NetServices;
using EnumsLib;
using ModelsLib;
using ModelsLib.BlockchainLib;
using ModelsLib.NetworkModels;
using Newtonsoft.Json;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;
using Utilities;

class Program
{
    static void Main(string[] args)
    {
        // Создаем или обновляем базу данных
        using (var context = new AppDbContext())
        {
            context.Database.EnsureCreated(); // Убедитесь, что база данных создана
        }

        // Добавляем новые записи в базу данных
        // AddSampleData();
       //  addDataToDb();
        // Читаем и выводим данные из базы данных
       // ReadData();
       // DeleteData();
       
      // AddTransaction();
     //   ReadData();
     
    // AddBlockToDb();
  //  DeleteBlocksFromDb();
  // AddPeerInfo();

  
  
  
  
  
  
    IDbProcessor dbProcessor = new DbProcessor();
    var model = dbProcessor.ProcessService<IModel>(new PeerInfoService(new AppDbContext()), CommandType.Get, new DbData(null,"2"));
    Console.WriteLine($"Data: {JsonConvert.SerializeObject(model.Result)}");




    }


    // public static void AddBlockToDb()
    // {
    //     var genesisTransaction = new TransactionModel
    //     {
    //         Id = Guid.NewGuid().ToString(),
    //         Sender = null, // Генезис-блок, откуда токены исходят
    //         Receiver = "address_1", // Первый адрес, на который поступают токены
    //         Amount = 1000, // Начальное количество токенов
    //         Timestamp = DateTime.Now
    //     };
    //     
    //     string transactions = JsonConvert.SerializeObject(genesisTransaction);
    //     var genesisBlock = new BlockModel()
    //     {
    //         Id = Guid.NewGuid().ToString(),
    //         Index = 1,
    //         Timestamp = DateTime.Now, 
    //         Transactions = transactions,
    //         MerkleRoot = "12",
    //         PreviousHash = "0",
    //         Hash = "",
    //         Difficulty = 2,
    //         Nonce = "00",
    //         Signature = "siignature",
    //         Size = 21321,
    //
    //     };
    //
    //     Logger.Log($"[Blockchain] Converting Block to BlockModel: Id={genesisBlock.Id}");
    //     Logger.Log($"Block = {genesisBlock.Transactions.Count()} transactions from blockchain.");
    //
    //     
    // }

    // public static void DeleteBlocksFromDb()
    // {
    //     BlocksBdService _blocksBdService = new BlocksBdService(new AppDbContext());
    //     Task<List<BlockModel>> blocks =_blocksBdService.GetAllBlocksAsync();
    //     foreach (var block in blocks.Result)
    //     {
    //         _blocksBdService.DeleteBlockAsync(block.Id);
    //     }
    // }


//     public static async Task AddTransaction()
//     {
//         try
//         {
//             // Создаем экземпляр сервиса
//             TransactionBdService bdService = new TransactionBdService(new AppDbContext());
//
//             // Создаем и добавляем новую транзакцию
//             await bdService.AddTransactionAsync(new TransactionModel()
//             {
//                 Id = Guid.NewGuid().ToString(),
//                 Amount = 10.5m,
//                 Sender = "Alice",
//                 Receiver = "Bob",
//                 Fee = 0.1m,
//                 Signature = "SignaturePlaceholder",  // Например, заглушка для подписи
//                 Timestamp = DateTime.UtcNow
//                 
//             });
//         }
//         catch (Exception ex)
//         {
//             // Логирование ошибки
//             Logger.Log($"Error adding transaction: {ex.Message}");
//         
//             // Если есть внутренняя ошибка, выводим ее
//             if (ex.InnerException != null)
//             {
//                 Logger.Log($"Inner exception: {ex.InnerException.Message}");
//             }
//         }
//     }
//     
//     // Метод для добавления тестовых данных
//     static void AddSampleData()
//     {
//         using (var context = new AppDbContext())
//         {
//             // Добавляем новый объект KnownPeersModel
//             var newKnownPeer = new KnownPeersModel
//             {
//                 Address = "192.168.1.1",
//                 Port = 8080,
//                 Type = "NodeTypeA",
//                 Status = NodeStatus.Active
//             };
//
//                 context.PeersList.Add(newKnownPeer);
//
//             // Сохраняем изменения в базе данных
//            context.SaveChanges();
//
//            Console.WriteLine("Sample data added.");
//         }
//     }
//
//     // Метод для чтения и вывода данных из базы данных
//     static void ReadData()
//     {
//         using (var context = new AppDbContext())
//         {
//             // Получаем все записи из таблицы PeersList
//             var knownPeers = context.PeersList.ToList();
//             Console.WriteLine("\nKnownPeersList:");
//             foreach (var peer in knownPeers)
//             {
//                 Console.WriteLine($"Id: {peer.Id}, Address: {peer.Address}, Port: {peer.Port}, Type: {peer.Type}, Status: {peer.Status}");
//             }
//         }
//     }
//
//     static void DeleteData()
//     {
//         using (var context = new AppDbContext())
//         {
//             // Получаем все объекты из таблицы PeersList
//             var peers = context.PeersList.ToList();
//
//             // Удаляем каждый объект
//             context.PeersList.RemoveRange(peers);
//
//             // Сохраняем изменения в базе данных
//             context.SaveChanges();
//         }
//     }
//
//     public static void addDataToDb()
//     {
//         PeerInfoService peersListService = new PeerInfoService(new AppDbContext());
//         
//         var newKnownPeer = new KnownPeersModel
//         {
//             Address = "192.168.1.1",
//             Port = 8080,
//             Type = "NodeTypeA",
//             Status = NodeStatus.Active
//         };
//         
//         peersListService.Add(newKnownPeer);
//         
//     }
//
//
//     public static void AddPeerInfo()
//     {
//         PeerInfoService peerInfoService = new PeerInfoService(new AppDbContext());
//
//         var peerInfoModel = new PeerInfoModel()
//         {
//             NodeId = Guid.NewGuid().ToString(),
//             IpAddress = "192.168.220.56",
//             Port = 8080,
//             LastSeen = DateTime.Now,
//             NodeType = "NodeTypeA",
//             Status = NodeStatus.Active,
//             SoftwareVersion = "1.0.0"
//         };
//         
//         IDbProcessor _dbProcessor = new DbProcessor();
//       //  peerInfoService.AddPeerInfoAsync(peerInfoModel);
//        // bool result = await _dbProcessor.ProssecService<bool>(new PeerInfoService(new AppDbContext()), CommandType.Add, new DbData(peerInfoModel));
//
//     }
}