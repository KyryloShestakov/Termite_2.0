using RRLib.Responses;

namespace BlockchainLib.Addresses;

public class AddressService
{
    public AddressManager _AddressManager;
    ServerResponseService _serverResponseService;

    public AddressService()
    {
        _AddressManager = new AddressManager();
        _serverResponseService = new ServerResponseService();
    }

    public Response GenerateAddress()
    {
        (string Address, string PrivateKey) = _AddressManager.GenerateAddressWithKeys();
        var data = new { Address = Address, PrivateKey = PrivateKey };
        Response response = _serverResponseService.GetResponse(true, "The address was generated", data);
        return response;
    }
}