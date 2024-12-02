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
            // Log message only if the log level is greater than or equal to the minimum level
            if (level >= MinimumLogLevel)
            {
                Console.WriteLine($" [{source}] [{level}] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
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
    Blockchain
}
