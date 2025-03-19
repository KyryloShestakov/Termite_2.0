namespace Ter_Protocol_Lib.Requests;

public enum TerMessageType
{
    Handshake = 0x01,
    Block = 0x02,
    Transaction = 0x03,
    PeerDiscovery = 0x04,
    Consensus = 0x05,
    SyncRequest = 0x06,
    SyncResponse = 0x07,
    KeyExchange = 0x08,
    KnownPeers = 0x09,
    PeerInfo = 0x0a,
    Empty = 0x0b,
}