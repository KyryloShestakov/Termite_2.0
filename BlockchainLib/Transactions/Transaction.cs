using System.Text;
using BlockchainLib.SmartContracts;
using Microsoft.EntityFrameworkCore;
using ModelsLib.BlockchainLib;
using Newtonsoft.Json;

namespace BlockchainLib
{


    public class Transaction
    {
        public string Id { get; set; }
        public string BlockId { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Fee { get; set; }
        public string Signature { get; set; }
        public Object? Data { get; set; } = null;
        public SmartContract? Contract { get; set; } = null;

        public Transaction(string sender, string receiver, decimal amount, decimal fee, string signature, Object data)
        {
            Id = Guid.NewGuid().ToString();
            Sender = sender;
            Receiver = receiver;
            Amount = amount;
            Timestamp = DateTime.UtcNow;
            Fee = fee;
            Signature = signature;
            Data = data;
        }

        public Transaction() { }

        public override string ToString()
        {
            return $"{Sender} -> {Receiver}: {Amount} at {Timestamp}";
        }

        public int CalculateSize()
        {
            int size = 0;

            size += Encoding.UTF8.GetByteCount(Sender);
            size += Encoding.UTF8.GetByteCount(Receiver);
            size += sizeof(decimal);
            size += sizeof(decimal);
            size += Encoding.UTF8.GetByteCount(Signature);
            size += sizeof(long);

            if (Data != null)
            {
                size += Encoding.UTF8.GetByteCount(Data.ToString());
            }

            if (Contract != null)
            {
                // size += Contract.CalculateSize();
            }

            return size;
        }

        public static TransactionModel ToModel(Transaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            return new TransactionModel
            {
                Id = transaction.Id,
                Sender = transaction.Sender,
                Receiver = transaction.Receiver,
                Amount = transaction.Amount,
                Timestamp = transaction.Timestamp,
                Fee = transaction.Fee,
                Signature = transaction.Signature,
                BlockId = transaction.BlockId,
                Data = transaction.Data,
                Contract = null
            };
        }

        public static Transaction ToEntity(TransactionModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            return new Transaction
            {
                Id = model.Id,
                Sender = model.Sender,
                Receiver = model.Receiver,
                Amount = model.Amount,
                Timestamp = model.Timestamp,
                Fee = model.Fee,
                Signature = model.Signature,
                BlockId = model.BlockId,
                Data = model.Data,
                Contract = null
            };
        }

        public static string Serialize(Transaction model)
        {
            return JsonConvert.SerializeObject(model, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            });
        }

    }
}