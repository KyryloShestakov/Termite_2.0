namespace BlockchainLib.Addresses;

public interface IAddressManager
{
    (string Address, string PrivateKey) GenerateAddressWithKeys();
    Task<decimal> GetBalance(string address);
}
