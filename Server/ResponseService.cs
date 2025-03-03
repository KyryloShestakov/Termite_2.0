using System.Net.Sockets;
using Utilities;

namespace Server.Responses;

/// <summary>
/// Abstract base class for handling response services in the server.
/// Provides functionality to send data asynchronously over a TCP connection.
/// </summary>
public abstract class ResponseService
{
    /// <summary>
    /// Asynchronously writes data to the provided TCP client.
    /// Ensures that the client is not null, and handles exceptions that may occur during data transmission.
    /// </summary>
    /// <param name="client">The TCP client to send data to.</param>
    /// <param name="buffer">The data to be sent as a byte array.</param>
    /// <exception cref="ArgumentNullException">Thrown if the client is null.</exception>
    /// <exception cref="Exception">Handles any exceptions that occur during network communication.</exception>
    protected virtual async Task Write(TcpClient client, byte[] buffer)
    {
        // Ensure the client is not null before attempting to write
        ArgumentNullException.ThrowIfNull(client, nameof(client));

        try
        {
            // Get the network stream to send data
            using (NetworkStream stream = client.GetStream())
            {
                // Write data to the stream asynchronously
                await stream.WriteAsync(buffer, 0, buffer.Length);

                // Ensure the data is sent by flushing the stream
                await stream.FlushAsync();
            }
        }
        catch (Exception ex)
        {
            // Log any errors that occur during the sending of data
            Logger.Log($"Error when sending data: {ex.Message}", LogLevel.Error, Source.Server);
        }
    }
}