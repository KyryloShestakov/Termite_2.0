using DataLib.DB.SqlLite.Services.NetServices; // Imports network-related database services
using ModelsLib.NetworkModels; // Imports network-related models
using StorageLib.DB.SqlLite; // Imports database storage services for SQLite

namespace Ter_Protocol_Lib.Requests; // Defines the namespace for organizing the protocol-related classes

/// <summary>
/// Represents the header of a protocol message, containing metadata such as sender, recipient, and timestamps.
/// </summary>
public class TerHeader
{
    /// <summary>
    /// The type of message being transmitted.
    /// </summary>
    public TerMessageType MessageType { get; set; }

    /// <summary>
    /// The protocol version.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// The sender's public key.
    /// </summary>
    public string PublicKey { get; set; }

    /// <summary>
    /// The sender's external IP address.
    /// </summary>
    public string MyIpAddress { get; set; }

    /// <summary>
    /// The recipient's unique identifier.
    /// </summary>
    public string RecipientId { get; set; }

    /// <summary>
    /// The recipient's IP address.
    /// </summary>
    public string RecipientIp { get; set; }

    /// <summary>
    /// The sender's unique identifier.
    /// </summary>
    public string SenderId { get; set; }

    /// <summary>
    /// The timestamp indicating when the message was created.
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// The method type associated with the message.
    /// </summary>
    public MethodType MethodType { get; set; }

    /// <summary>
    /// Helper class for retrieving external IP addresses.
    /// </summary>
    private IpHelper _ipHelper;

    /// <summary>
    /// Database processor for interacting with the SQLite database.
    /// </summary>
    private DbProcessor _dbProcessor;

    /// <summary>
    /// Constructs a TerHeader object with specified message type, recipient ID, and method type.
    /// Initializes the database processor and IP helper, sets default values, and retrieves necessary information asynchronously.
    /// </summary>
    /// <param name="type">The type of the message.</param>
    /// <param name="recipientId">The recipient's unique identifier.</param>
    /// <param name="methodType">The method type of the message.</param>
    public TerHeader(TerMessageType type, string recipientId, MethodType methodType)
    {
        _ipHelper = new IpHelper();
        _dbProcessor = new DbProcessor();

        MessageType = type;
        MethodType = methodType;
        Version = "1.0";
        Timestamp = DateTime.Now;
        RecipientId = recipientId;

        // Asynchronously fetch external IP, recipient's IP, and public key
        _ = InitializeAsync(recipientId);
    }
    
    private async Task InitializeAsync(string recipientId)
    {
        await Task.WhenAll(
            SetExterrnalIpAsync(),
            SetRecipientIpAsync(recipientId),
            SetPublicKeyAsync()
        );
    }

    /// <summary>
    /// Default constructor for TerHeader.
    /// </summary>
    public TerHeader() { }

    /// <summary>
    /// Retrieves the recipient's IP address from the database asynchronously.
    /// </summary>
    /// <param name="recipientId">The unique identifier of the recipient.</param>
    private async Task SetRecipientIpAsync(string recipientId)
    {
        if (string.IsNullOrEmpty(recipientId))
        {
            throw new ArgumentException("RecipientId cannot be null or empty", nameof(recipientId));
        }
        PeerInfoModel recipientPeerInfo = await _dbProcessor.ProcessService<PeerInfoModel>(
            new PeerInfoService(new AppDbContext()), 
            CommandType.Get, 
            new DbData(null, recipientId));

        if (recipientPeerInfo.IpAddress != null) RecipientIp = recipientPeerInfo.IpAddress;
    }

    /// <summary>
    /// Retrieves the sender's external IP address asynchronously.
    /// </summary>
    private async Task SetExterrnalIpAsync()
    {
        MyIpAddress = await _ipHelper.GetExternalAddress();
    }

    /// <summary>
    /// Retrieves the sender's public key and node ID from the database asynchronously.
    /// </summary>
    private async Task SetPublicKeyAsync()
    {
        MyPrivatePeerInfoModel myInfo = await _dbProcessor.ProcessService<MyPrivatePeerInfoModel>(
            new MyPrivatePeerInfoService(new AppDbContext()), 
            CommandType.Get, 
            new DbData(null, "default"));

        PublicKey = myInfo.PublicKey;
        SenderId = myInfo.NodeId;
    }
}
