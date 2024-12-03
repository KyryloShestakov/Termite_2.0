namespace BlockchainLib.Addresses;

public interface IAddressManager
{
    (string Address, string PrivateKey) GenerateAddressWithKeys();
    decimal GetBalance(string address);
}
