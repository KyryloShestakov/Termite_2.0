using System.Net.Sockets;
using System.Text;
using RRLib.Requests.NetRequests;
using ModelsLib.BlockchainLib;
using ModelsLib.NetworkModels;
using Newtonsoft.Json;
using RRLib;
using RRLib.Requests.BlockchainRequests;
using SecurityLib.Security;
using StorageLib.DB.Redis;
using Utilities;
using JsonException = System.Text.Json.JsonException;
using Response = RRLib.Responses.Response;

namespace Node_02
{
    class Program
    {
        public static async Task Main(string[] args)
        {
           SecureConnectionManager secureConnectionManager = new SecureConnectionManager();
           byte[] publickey = secureConnectionManager.GetPublicKeyBytes();
           string publicKey = System.Convert.ToBase64String(publickey);
           ConnectionKey connectionKey = new ConnectionKey { Key = publicKey };
           

           var request = new KeyExchangeRequest()
           {
               RecipientId = "node1",
               ProtocolVersion = "1.0",
               Route = new List<string> { "node1", "node2", "node3" },
               Ttl = 10,
               SenderId = "client123",
               //TODO Мне надо сделать в таблице известных узлов их айди 
               PayLoad = new PayLoad
               {
                   PayloadObject = new Dictionary<string, object>
                   {
                       { "PublicKey", connectionKey }
                   }
               },
               RequestGroup = "Net",
               Method = "POST"
           };

           string jsonRequest = request.Serialize();
           
           
           PayLoad payLoad1 = new PayLoad();

           var knownPeers = new KnownPeersModel()
           {
            Address = "192.168.220.56",
            Port = 8080,
            Status = NodeStatus.Inactive,
            Type = "Node",
           };
           var knownPeers2 = new KnownPeersModel()
           {
               Address = "192.168.220.56",
               Port = 8080,
               Status = NodeStatus.Active,
               Type = "Node",
           };

           var request1 = new KnownPeersRequest()
           {
               RecipientId = "node2",
               ProtocolVersion = "1.0",
               Route = new List<string> { "node1", "node2", "node3" },
               Ttl = 10,
               SenderId = "client123",
               PayLoad = new PayLoad
               {
                   KnownPeers = new List<KnownPeersModel>()
                   {
                       knownPeers
                   }
               },
               RequestGroup = "Net",
               Method = "POST"
           };

           var transaction21 = new TransactionModel()
           {
               Id = Guid.NewGuid().ToString(),
               Amount = 10.5m,
               Sender = "Alice",
               Receiver = "Bob",
               Fee = 0.1m
           };
           
           string transactions = JsonConvert.SerializeObject(transaction21);
           
           var block = new BlockModel
           {
               Id = Guid.NewGuid().ToString(),
               Index = 1,
               Timestamp = DateTime.UtcNow,
               MerkleRoot = "someMerkleRoot",
               PreviousHash = "somePreviousHash",
               Hash = "someHash",
               Difficulty = 1,
               Nonce = "someNonce",
               Signature = "someSignature",
               Size = 1234,
               Transactions = transactions
           };

           
           var TypeOfTransaction = "Unconfirmed";

           var transaction = new TransactionModel
           {
               Id = Guid.NewGuid().ToString(),
               Amount = 30.5m,
               Sender = "Bob",
               Receiver = "Alice",
               Fee = 0.1m,
               Signature = "someSignature",
               Data = TypeOfTransaction
           };

           var request2 = new TransactionRequest()
           {
               RecipientId = "node3",
               ProtocolVersion = "2.0",
               Route = new List<string> { "node1", "node2", "node3" },
               Ttl = 10,
               SenderId = "client123",
               RequestGroup = "Blockchain",
               Method = "POST",
               PayLoad = new PayLoad
               {
                   Transactions = new List<TransactionModel>()
                   {
                       transaction
                   }
               }
           };
           
           RedisService redisService = new RedisService();
         // byte[] sessionkey = secureConnectionManager.GenerateSessionKey();    
           string sessionkeystr = await redisService.GetStringAsync("client123");
           byte[] sessionkeybytes = Convert.FromBase64String(sessionkeystr);
           if (sessionkeybytes.Length > 32)
           {
            //   Logger.Log($"Session key is larger than 32 bytes, truncating it to 256 bits (32 bytes).", LogLevel.Warning, Source.Secure);
            //   Logger.Log(sessionkeybytes.Length.ToString(), LogLevel.Warning, Source.Secure);
               Array.Resize(ref sessionkeybytes, 32); 
           }
           
           //Менять здесь номер запроса
           byte[] encryptedMessage = secureConnectionManager.EncryptMessage(request2.PayLoad.Serialize(), sessionkeybytes); 
           request2.PayLoad.EncryptedPayload = encryptedMessage;
           
           Logger.Log($"Encrypted message: {Convert.ToBase64String(encryptedMessage)}", LogLevel.Information, Source.Client);
          // Logger.Log($"{request2.PayLoad.Serialize()}");
          request2.PayLoad.Transactions.Clear();
           string jsonRequest1 = request2.Serialize();
           
           Logger.Log($"{jsonRequest1}", LogLevel.Information, Source.Client);
           
           /*Console.WriteLine($"---------- {jsonRequest1}");
           
           string encryptedMessage1 = secureConnectionManager.DecryptMessage(request1.PayLoad.EncryptedPayload, sessionkey);
           PayLoad deserializedPayload = PayLoad.DeserializePayLoad(encryptedMessage1);
           
           List<KnownPeersModel> knownPeers1 = deserializedPayload.KnownPeers;

           foreach (var knownPeersModel in knownPeers1)
           {
               Console.WriteLine($"---------- {knownPeersModel.Address}");

           }*/

           
           
           
           string serverIp = "192.168.220.56";
           int serverPort = 8080;

           try
           {
               using (TcpClient client = new TcpClient())
               {
                   Console.WriteLine("Подключение к серверу...");
                   await client.ConnectAsync(serverIp, serverPort);

                   using (NetworkStream stream = client.GetStream())
                   {
                       // Отправка данных
                       byte[] dataToSend = Encoding.UTF8.GetBytes(jsonRequest1);
                       Console.WriteLine($"Отправка запроса: {jsonRequest1}");
                       await stream.WriteAsync(dataToSend, 0, dataToSend.Length);
                       Console.WriteLine("Запрос отправлен.");

                       Response response = await ReadResponseAsync(stream);
                       
                        string jsonResponse = response.Message;
                        Logger.Log(jsonResponse, LogLevel.Information, Source.Client);
                       // Logger.Log(response.Data.ToString());
                        
                      /*  List<KnownPeersModel>? knownPeers1 = JsonConvert.DeserializeObject<List<KnownPeersModel>>(jsonResponse);
                        foreach (var peer in knownPeers1)
                        {
                            Logger.Log(peer.Address, LogLevel.Information, Source.Client);
                        }    */                    
                   }
               }
           }
           catch (Exception ex)
           {
               Console.WriteLine($"Ошибка: {ex.Message}");
           }
        }
        
        public static async Task<Response> ReadResponseAsync(Stream stream)
        {
            try
            {
                byte[] buffer = new byte[1024];

                // Чтение данных из потока
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    Console.WriteLine("Server did not send a response.");
                    return null;
                }

                // Извлечение только полученных байт
                byte[] responseData = new byte[bytesRead];
                Array.Copy(buffer, responseData, bytesRead);
                

                // Десериализация объекта Response из байт
                Response response = DeserializeFromBytes<Response>(responseData);
                return response;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON Parsing Error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading response: {ex.Message}");
                return null;
            }
        }


        private static T DeserializeFromBytes<T>(byte[] data)
        {
            string json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(json);
        }

    }
}
