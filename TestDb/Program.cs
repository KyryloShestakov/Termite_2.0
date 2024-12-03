

using ModelsLib.BlockchainLib;
using ModelsLib.NetworkModels;
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
       
       AddTransaction();
       
        ReadData();
    }

    public static async Task AddTransaction()
    {
        try
        {
            // Создаем экземпляр сервиса
            TransactionBdService bdService = new TransactionBdService(new AppDbContext());

            // Создаем и добавляем новую транзакцию
            await bdService.AddTransactionAsync(new TransactionModel()
            {
                Id = Guid.NewGuid().ToString(),
                Amount = 10.5m,
                Sender = "Alice",
                Receiver = "Bob",
                Fee = 0.1m,
                Signature = "SignaturePlaceholder",  // Например, заглушка для подписи
                Timestamp = DateTime.UtcNow
                
            });
        }
        catch (Exception ex)
        {
            // Логирование ошибки
            Logger.Log($"Error adding transaction: {ex.Message}");
        
            // Если есть внутренняя ошибка, выводим ее
            if (ex.InnerException != null)
            {
                Logger.Log($"Inner exception: {ex.InnerException.Message}");
            }
        }
    }

    

    // Метод для добавления тестовых данных
    static void AddSampleData()
    {
        using (var context = new AppDbContext())
        {
            // Добавляем новый объект KnownPeersModel
            var newKnownPeer = new KnownPeersModel
            {
                Address = "192.168.1.1",
                Port = 8080,
                Type = "NodeTypeA",
                Status = NodeStatus.Active
            };

                context.PeersList.Add(newKnownPeer);

            // Сохраняем изменения в базе данных
           context.SaveChanges();

           Console.WriteLine("Sample data added.");
        }
    }

    // Метод для чтения и вывода данных из базы данных
    static void ReadData()
    {
        using (var context = new AppDbContext())
        {
            // Получаем все записи из таблицы PeersList
            var knownPeers = context.PeersList.ToList();
            Console.WriteLine("\nKnownPeersList:");
            foreach (var peer in knownPeers)
            {
                Console.WriteLine($"Id: {peer.Id}, Address: {peer.Address}, Port: {peer.Port}, Type: {peer.Type}, Status: {peer.Status}");
            }
        }
    }

    static void DeleteData()
    {
        using (var context = new AppDbContext())
        {
            // Получаем все объекты из таблицы PeersList
            var peers = context.PeersList.ToList();

            // Удаляем каждый объект
            context.PeersList.RemoveRange(peers);

            // Сохраняем изменения в базе данных
            context.SaveChanges();
        }
    }

    public static void addDataToDb()
    {
        PeersListService peersListService = new PeersListService(new AppDbContext());
        
        var newKnownPeer = new KnownPeersModel
        {
            Address = "192.168.1.1",
            Port = 8080,
            Type = "NodeTypeA",
            Status = NodeStatus.Active
        };
        
        peersListService.AddPeerAsync(newKnownPeer);
        
    }
}