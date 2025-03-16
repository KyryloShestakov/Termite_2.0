using API.Application.Interfaces;
using BlockchainLib;
using BlockchainLib.Addresses;
using BlockchainLib.Validator;
using DataLib.DB.SqlLite.Interfaces;
using ModelsLib;
using ModelsLib.BlockchainLib;
using RRLib.Responses;
using StorageLib.DB.Redis;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;
using Utilities;
using LogLevel = Utilities.LogLevel;

namespace API.Infrastructure.Database;

public class Repository : IRepository
{
    private IDbProcessor _dbProcessor;
    private ServerResponseService _serverResponseService;
    private readonly AddressService _addressService;
    private TransactionValidator _validator;
    private BlockManager _blockManager;
    private TransactionService _transactionService;


    public Repository()
    {
        _dbProcessor = new DbProcessor();
        _addressService = new AddressService();
        _validator = new TransactionValidator();
        _serverResponseService = new ServerResponseService();
        _blockManager = new BlockManager();
        _transactionService = new TransactionService();
    }

    public async Task<Response> GetAddressAndPrivateKey()
    {
        Response response = _addressService.GenerateAddress();
        return response;
    }

    public async Task<Response> AddTransaction(TransactionModel transaction)
    {
        try
        {
            Response isValid = await _validator.Validate(transaction);
            // if (isValid.Status != "Success") 
            //     return _serverResponseService.GetResponse(false, $"{isValid.Message}", transaction);

            Logger.Log($"Transaction is valid {transaction.Id}", LogLevel.Information, Source.API);

            List<IModel> transactionsList = await _dbProcessor.ProcessService<List<IModel>>(
                new TransactionBdService(new AppDbContext()), CommandType.GetAll);

            List<TransactionModel> transactionModels = transactionsList.Cast<TransactionModel>().ToList();
            bool transactionExists = transactionModels
                .Any(t => ((TransactionModel)t).Id == transaction.Id);

            if (transactionExists)
                return _serverResponseService.GetResponse(false, "Transaction already exists", transaction);

            bool isAdded = await _dbProcessor.ProcessService<bool>(
                new TransactionBdService(new AppDbContext()), CommandType.Add, new DbData(transaction, transaction.Id));

            if (!isAdded) 
                return _serverResponseService.GetResponse(false, "Transaction wasn't added", transaction);

            return _serverResponseService.GetResponse(true, "Transaction added successfully", transaction);
        }
        catch (Exception e)
        {
            Logger.Log(e.Message, LogLevel.Error, Source.API);
            throw;
        }
    }


    public async Task<Response> GetTransaction(string transactionId)
    {
        try
        {
         List<IModel> models = await _dbProcessor.ProcessService<List<IModel>>(new BlocksBdService(new AppDbContext()), CommandType.GetAll);
         List<BlockModel> blocks = models.Cast<BlockModel>().ToList();
         
         var transaction = blocks
             .SelectMany(block => _blockManager.GetTransactionsFromBlock(block))
             .FirstOrDefault(t => t.Id == transactionId);
            
         return transaction != null
             ? _serverResponseService.GetResponse(true, "Transaction was found", transaction)
             : _serverResponseService.GetResponse(false, "Transaction wasn't found", null);
        }
        catch (Exception e)
        {
            Logger.Log($"Error: {e.Message}", LogLevel.Error, Source.API);
            throw;
        }
    }
    
    public async Task<Response> GetTransactionsByAddress(string address)
    {
        try
        {
            List<IModel> models = await _dbProcessor.ProcessService<List<IModel>>(new BlocksBdService(new AppDbContext()), CommandType.GetAll);
            List<BlockModel> blocks = models.Cast<BlockModel>().ToList();
         
            var transactions = blocks
                .SelectMany(block => _blockManager.GetTransactionsFromBlock(block))
                .Where(t => t.Sender == address || t.Receiver == address)
                .ToList();
            
            return transactions != null
                ? _serverResponseService.GetResponse(true, "Transactions was found", transactions)
                : _serverResponseService.GetResponse(false, "Transactions wasn't found", null);
        }
        catch (Exception e)
        {
            Logger.Log($"Error: {e.Message}", LogLevel.Error, Source.API);
            throw;
        }
    }

}