using System;
using System.Net.Sockets;
using Client;
using ModelsLib.NetworkModels;
using Utilities;
using System.Threading.Tasks;

namespace ClientTest;

class Program
{
    
    public static void Main(string[] args)
    {
        Logger.Log("Starting...", LogLevel.Information, Source.Client);
        try
        {
            ClientTcp client = new ClientTcp();
            client.RunAsync();
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Logger.Log($"Error occurred: {ex.Message}", LogLevel.Error, Source.Client);
        }
    }
}