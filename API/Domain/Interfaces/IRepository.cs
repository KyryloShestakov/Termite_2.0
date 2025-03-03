using ModelsLib.BlockchainLib;
using RRLib.Responses;

namespace API.Application.Interfaces;

public interface IRepository
{
    Task<Response> GetAddressAndPrivateKey();
    Task<Response> AddTransaction(TransactionModel transaction);
    Task<Response> GetTransaction(string transactionId);
    Task<Response> GetTransactionsByAddress(string address);
}