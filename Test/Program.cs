using SecurityLib.Security;
using StorageLib.DB.Redis;
using Ter_Protocol_Lib.Requests;
using Newtonsoft.Json;

using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Создаем запрос на транзакцию
            Guid guid = new Guid("ab90ee9b-babe-4bcc-a41f-4bd0fee16d3b");
            TransactionRequest transactionRequest = new TransactionRequest();
            transactionRequest.Id = guid;
            // transactionRequest.Transactions.Add(new TransactionModel() { Id = "2deceededwed", Amount = 100 });
            // transactionRequest.Transactions.Add(new TransactionModel() { Id = "3deceededwed", Amount = 100 });
            
            // Получаем ключ сессии из Redis
            RedisService redisService = new RedisService();
            string sessionKey = redisService.GetStringAsync("cf161569-00a0-4b9e-9670-a52c1f81e612").Result;
            byte[] sessionKeyBytes = Convert.FromBase64String(sessionKey);
            if (sessionKeyBytes.Length > 32) Array.Resize(ref sessionKeyBytes, 32);
            
            // Шифруем данные транзакции
            SecureConnectionManager secureConnectionManager = new SecureConnectionManager();
            string serializedTransactions = JsonConvert.SerializeObject(transactionRequest);
            byte[] encryptedMessage = secureConnectionManager.EncryptMessage(serializedTransactions, sessionKeyBytes);
            string encryptedMessageBase64 = Convert.ToBase64String(encryptedMessage);

            // Создаем протокол с зашифрованными данными
            TerProtocol<string> terProtocol = new TerProtocol<string>(
                new TerHeader(TerMessageType.Transaction, "88ddefb0-e5e1-41ab-858c-7677cad59218", MethodType.Post),
                new TerPayload<string>(encryptedMessageBase64)
            );

            // Сериализуем протокол
            string json = terProtocol.Serialize();
            Console.WriteLine("Serialized JSON:");
            Console.WriteLine(json);

            // Десериализуем JSON
            Console.WriteLine("Deserialized JSON:");
            
            var terRequest = TerProtocol<string>.Deserialize(json);
            string decryptRequest = DecryptRequest(terRequest.Payload);
            if (terRequest is not null)
            {
                Console.WriteLine($"MessageType: {terRequest.Header.MessageType}");
                
                var obj = RequestSerializer.DeserializeData(terRequest.Header.MessageType, decryptRequest);
               
                switch (obj)
                {
                    case TransactionRequest transaction:
                        TerProtocol<object> tr = new TerProtocol<object>(new TerHeader(),
                            new TerPayload<object>(transaction));
                        tr.Header = terRequest.Header;
                        
                        Console.WriteLine(JsonSerializer.Serialize(tr.Payload.Data));
                        TransactionRequest txs = (TransactionRequest)tr.Payload.Data;
                        Console.WriteLine(txs.Id);
                        break;
                }
            }
        }

        public static string DecryptRequest(TerPayload<string> request)
        {
            RedisService redisService = new RedisService();
            SecureConnectionManager secureConnectionManager = new SecureConnectionManager();

            try
            {
                if (true)
                {
                    string sessionKey = redisService.GetStringAsync("cf161569-00a0-4b9e-9670-a52c1f81e612").Result;
                    byte[] sessionKeyBytes = Convert.FromBase64String(sessionKey);
                    if (sessionKeyBytes.Length > 32) Array.Resize(ref sessionKeyBytes, 32);
                    string decryptedPayload = secureConnectionManager.DecryptMessage(Convert.FromBase64String(request.Data), sessionKeyBytes);
                    Console.WriteLine($"Decrypted: {decryptedPayload}");
                    return decryptedPayload;
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
