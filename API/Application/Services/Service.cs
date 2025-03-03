using API.Application.Interfaces;
using API.Infrastructure.Database;
using BlockchainLib.Addresses;
using ModelsLib.BlockchainLib;
using RRLib.Responses;

namespace API.Application.Services;
public class Service
{
    private readonly IRepository _repository;
    private readonly AddressService _addressService;
    public Service(IRepository repository)
    {
        _repository = repository;
        _addressService = new AddressService();
    }

    public async Task<Response> GetAddressAndPrivateKey()
    {
        return await _repository.GetAddressAndPrivateKey();
    }

    public async Task PostTransaction(TransactionModel transactionModel)
    {
        await _repository.AddTransaction(transactionModel);
    }

    public async Task<Response> GetBalance(string address)
    {
        return await _addressService.GetBalance(address);;
    }

    public async Task<Response> GetTransaction(string transactionId)
    {
        return await _repository.GetTransaction(transactionId);
    }
    public async Task<Response> GetTransactionById(string address)
    {
        return await _repository.GetTransactionsByAddress(address);
    }

}