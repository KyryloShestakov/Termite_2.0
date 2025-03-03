namespace Server.Requests
{
    /// <summary>
    /// Represents a TCP request received by the server.
    /// Encapsulates the raw message and provides utility methods for processing it.
    /// </summary>
    public class TcpRequest
    {
        /// <summary>
        /// The raw message content of the request.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpRequest"/> class with a given message.
        /// </summary>
        /// <param name="message">The raw string message from the TCP request.</param>
        public TcpRequest(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Provides a string representation of the TCP request, including the raw message content.
        /// </summary>
        /// <returns>A formatted string representing the request.</returns>
        public override string ToString()
        {
            return "Request: " + Message;
        }
    }
}