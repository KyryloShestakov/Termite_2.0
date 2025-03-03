namespace BlockchainLib.Addresses;

public interface IAddressManager
{
    (string Address, string PrivateKey, string PublicKey) GenerateAddressWithKeys();
    Task<decimal> GetBalance(string address);
}
