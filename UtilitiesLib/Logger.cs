using System.Net;
using System.Net.Sockets;

namespace Utilities;

public class Logger
{
    /// <summary>
    /// Logs a general message with a timestamp to the console.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static void Log(string message)
    {
        try
        {
            Console.WriteLine($"{DateTime.Now} : {message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging message: {ex.Message}");
        }
    }

    /// <summary>
    /// Logs a TcpClient connection message including the client's IP address and port.
    /// </summary>
    /// <param name="client">The TcpClient instance.</param>
    /// <param name="level">The log level.</param>
    /// <param name="source">The source of the log message.</param>
    public static void Log(TcpClient client, LogLevel level, Source source)
    {
        try
        {
            // Attempt to get the remote endpoint and log client connection information
            IPEndPoint endPoint = (IPEndPoint)client.Client.RemoteEndPoint;
            Console.WriteLine($" [{source}] [{level}] The client is connected {endPoint.Address}:{endPoint.Port}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging TcpClient connection: {ex.Message}");
        }
    }

    public static LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;
    public static Source Source { get; set; } = Source.Unknown;

    /// <summary>
    /// Logs a message with the specified log level and source, and only if the log level is at or above the minimum threshold.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="level">The log level.</param>
    /// <param name="source">The source of the log message.</param>
    public static void Log(string message, LogLevel level, Source source)
    {
        try
        {
            string colorCode = level switch
            {
                LogLevel.Information => "\u001b[32m",  
                LogLevel.Warning => "\u001b[33m",      
                LogLevel.Error => "\u001b[31m",        
                _ => "\u001b[0m"                       
            };

            string sourceColorCode = "\u001b[34m";

            if (level >= MinimumLogLevel)
            {
                Console.WriteLine($"{sourceColorCode}[{source}\u001b[0m] {colorCode}[{level}]{colorCode} [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\u001b[0m");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging message with level {level}: {ex.Message}");
        }
    }


    public static void Log(string message, LogLevel level, Source source, string messageType = null)
    {
        try
        {
            // Log message only if the log level is greater than or equal to the minimum level
            if (level >= MinimumLogLevel)
            {
                Console.WriteLine($" [{source}] [{level}] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{messageType}] {message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging message with level {level}: {ex.Message}");
        }
    }
}

public enum LogLevel
{
    Information,
    Warning,
    Error
}

public enum Source
{
    Server,
    Client,
    Unknown,
    Secure,
    Storage,
    Blockchain,
    App,
    Validator,
    PeerCore,
    API,
    Protocol,
}
