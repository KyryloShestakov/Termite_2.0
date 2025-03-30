using BlockchainLib;
using BlockchainLib.Addresses;
using BlockchainLib.Blocks;
using ModelsLib.BlockchainLib;
using RRLib.Responses;
using StorageLib.DB.Redis;
using Utilities;

public class Program
{
    public static async Task Main(string[] args)
    {
        BlockChainCore blockChain = new BlockChainCore();

        Timer timer = new Timer(TimerCallback, blockChain, 0, 10000);
        
        Console.WriteLine("Blockchain started. Press Enter to exit.");
        Console.ReadLine();
    }
    
    
    private static void TimerCallback(object state)
    {
        try
        {
            BlockBuilder blockBuilder = new BlockBuilder();
            blockBuilder.StartBuilding();
            Logger.Log($"Blockchain processed", LogLevel.Information, Source.Blockchain);
        }
        catch (Exception ex)
        {
            Logger.Log($"Error in blockchain processing: {ex.Message}");
        }
    }
}