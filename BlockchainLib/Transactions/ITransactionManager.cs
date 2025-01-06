namespace BlockchainLib;

public interface ITransactionManager
{
    Task<Transaction> CreateTransaction(string senderAddress, string recipientAddress, decimal amount, decimal fee, string signature, Object data);
    bool ValidateTransaction(Transaction transactionModel);

}