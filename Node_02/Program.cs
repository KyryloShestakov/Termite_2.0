using System.Net.Sockets;
using System.Text;
using BlockchainLib;
using BlockchainLib.Addresses;
using DataLib.DB.SqlLite.Interfaces;
using DataLib.DB.SqlLite.Services.NetServices;
using EnumsLib;
using RRLib.Requests.NetRequests;
using ModelsLib.BlockchainLib;
using ModelsLib.NetworkModels;
using Newtonsoft.Json;
using RRLib;
using RRLib.Requests.BlockchainRequests;
using SecurityLib.Security;
using StorageLib.DB.Redis;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services;
using Utilities;
using JsonException = System.Text.Json.JsonException;
using Response = RRLib.Responses.Response;

namespace Node_02
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            IDbProcessor dbProcessor = new DbProcessor();
            AppDbContext appDbContext = new AppDbContext();
            AddressService addressService = new AddressService();
            TransactionService transactionService = new TransactionService();
            SecureConnectionManager secureConnectionManager = new SecureConnectionManager();
            PayLoad payLoad1 = new PayLoad();

            
            
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
           
           

           var knownPeers = new PeerInfoModel()
           {
            IpAddress = "192.168.220.56",
            Port = 8080,
            Status = NodeStatus.Inactive,
            NodeType = "Node",
            NodeId = "node1",
            LastSeen = DateTime.Now
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
                   KnownPeers = new List<PeerInfoModel>()
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
           List<BlockModel> blocks = new List<BlockModel>();
           blocks.Add(block);

           var blockRequest = new BlockRequest()
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
                   Blocks = blocks
               }
           };

           
           var TypeOfTransaction = "Unconfirmed";
           
           
           var data = addressService.GenerateAddress();
           var dataString = data.Data.ToString();
           string[] dataArray = dataString.Split(",");
           
           var data2 = addressService.GenerateAddress();
           var dataString2 = data2.Data.ToString();
           string[] dataArray2 = dataString2.Split(",");

          // Logger.Log(dataArray2[1]);
           string receiver = dataArray2[0].Substring(dataArray2[0].IndexOf("=") + 1).Trim();
           string receiver1 = receiver.Substring(0, receiver.Length - 2);
           
           
           var transaction = new TransactionModel
           {
               Id = Guid.NewGuid().ToString(),
               Amount = 1000m,
               Sender = "Nj5nVJNwssoo8XZi6aXmuwhuWRRNf22q5MCpo2LHMaChjrA93",
               Receiver = "29DckhJXKKvKYQfBdY9s6PvaMn4hcTcxKKL4u5oZCNhmwhfLSw",
               Fee = 0.1m,
               Signature = "Signed",
               Data = TypeOfTransaction,
               PublicKey = "4e1BUTgGBfqVWZ2YKwCRoKqQRtwDCZkBWRtkgtqgivUvauiNVPzQf3YfdEK6Tkh71f3X3tYAokwwooPtRQN68zz5yksmaUXpuki2Ns87L4YjruMpPHYHMDmzA8uEzADTnXpLSgz6zSXbShnA2vkuLpZjynrrzHD3c1wJjBR9TDeiQf5V51XawTxRw7xguQmMejRkWL634XjdN5wjSrnWqut3HzPiXj6fc67wJkdvrF1t9CYZHcZ2dpSpVBVvEZ3i5JaAaWDFvZYgQCH1ejfZuF39CuNoU2XHLozaQvEZZ6HET8HiZB6BXitEJhN6gLC3gfX5YrZYRJdyknJ1VpvNP1zLymUjz3ViJmTSJwpAM8R2znpS4"
           };

           string signature = transactionService.SignTransaction(transaction, dataArray[1]);
           transaction.Signature = signature;
           
           
           MyPrivatePeerInfoModel peerInfoModel = await 
               dbProcessor.ProcessService<MyPrivatePeerInfoModel>(new MyPrivatePeerInfoService(appDbContext),
                   CommandType.Get, new DbData(null, "default"));
           
           var request2 = new TransactionRequest()
           {
               RecipientId = "cf161569-00a0-4b9e-9670-a52c1f81e612",
               ProtocolVersion = "2.0",
               Route = new List<string> { "node1", "node2", "node3" },
               Ttl = 10,
               SenderId = peerInfoModel.NodeId,
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

           string jsonRequest1 = await EncryptRequest(request2);
           Logger.Log(jsonRequest1, LogLevel.Information, Source.Client);
           
           string serverIp = "192.168.17.99";
           string homeIp = "192.168.220.56";
           int serverPort = 8080;

           try
           {
               using (TcpClient client = new TcpClient())
               {
                   Logger.Log($"Connecting to the server {homeIp}", LogLevel.Information, Source.Client);
                   await client.ConnectAsync(homeIp, serverPort);

                   using (NetworkStream stream = client.GetStream())
                   {
                       byte[] dataToSend = Encoding.UTF8.GetBytes(jsonRequest1);
                       Logger.Log($"Sending a request: {jsonRequest1.Length} bytes", LogLevel.Information, Source.Client);
                       await stream.WriteAsync(dataToSend, 0, dataToSend.Length);
                       Logger.Log("Request sent.", LogLevel.Information, Source.Client);

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
               Logger.Log($"Error: {ex.Message}");
           }
        }

        public static async Task<string> EncryptRequest(Request request)
        {
            RedisService redisService = new RedisService();
            SecureConnectionManager secureConnectionManager = new SecureConnectionManager();
            string sessionkeystr = await redisService.GetStringAsync("cf161569-00a0-4b9e-9670-a52c1f81e612");
            byte[] sessionkeybytes = Convert.FromBase64String(sessionkeystr);
            if (sessionkeybytes.Length > 32)
            {
                Array.Resize(ref sessionkeybytes, 32); 
            }
            byte[] encryptedMessage = secureConnectionManager.EncryptMessage(request.PayLoad.Serialize(), sessionkeybytes); 
            request.PayLoad.EncryptedPayload = encryptedMessage;
            request.PayLoad.Transactions.Clear();
            string jsonRequest = request.Serialize();
            
           
            Logger.Log($"{jsonRequest}", LogLevel.Information, Source.Client);
            
            return jsonRequest;
        }

        public static async Task<string> DecryptRequest(Request request)
        {           
            SecureConnectionManager secureConnectionManager = new SecureConnectionManager();
            byte[] sessionkey = secureConnectionManager.GenerateSessionKey();    
            string encryptedMessage1 = secureConnectionManager.DecryptMessage(request.PayLoad.EncryptedPayload, sessionkey);
            PayLoad deserializedPayload = PayLoad.DeserializePayLoad(encryptedMessage1);
            
            request.PayLoad.EncryptedPayload = deserializedPayload.EncryptedPayload;
            return request.Serialize();
        }


        public static async Task<Response> ReadResponseAsync(Stream stream)
        {
            try
            {
                byte[] buffer = new byte[2024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    Logger.Log("Server did not send a response.");
                    return null;
                }
                
                byte[] responseData = new byte[bytesRead];
                Array.Copy(buffer, responseData, bytesRead);
                
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
