using BlockchainLib;
using BlockchainLib.Addresses;
using ModelsLib.BlockchainLib;
using RRLib.Responses;
using StorageLib.DB.Redis;
using Utilities;

public class Program
{
    public static async Task Main(string[] args)
    {
        BlockChainCore blockChain = new BlockChainCore();

        Timer timer = new Timer(TimerCallback, blockChain, 0, 60000);
        TransactionManager transactionManager = new TransactionManager();
        
        
        Console.WriteLine("Blockchain started. Press Enter to exit.");
        Console.ReadLine();
    }
    
    
    private static void TimerCallback(object state)
    {
        try
        {
            BlockChainCore blockChain = (BlockChainCore)state;
            blockChain.StartBlockchain();
            Logger.Log($"Blockchain processed", LogLevel.Information, Source.Blockchain);
        }
        catch (Exception ex)
        {
            Logger.Log($"Error in blockchain processing: {ex.Message}");
        }
    }
}