using ModelsLib.NetworkModels;
using RRLib;
using RRLib.Requests.NetRequests;

namespace CommonRequestsLibrary;

public class Test
{
    
    static KnownPeersModel _knownPeersModel;
    public Request initialRequest = new InitialConnectionRequest();
    public Request request = new PeerInfoRequest();


    public void Start()
    {
        PeerInfoModel peer = new PeerInfoModel();
        
         PayLoad payLoad2 = new PayLoad();
         PayLoad payLoad1 = new PayLoad();

         peer.IpAddress = "127.0.0.1";
         
         ConnectionKey publicKey = new ConnectionKey();
         publicKey.Key = "publicKey";
         
         payLoad2.PayloadObject.Add("PublicKey", publicKey);
         payLoad1.PayloadObject.Add("PeerInfo", peer);

         request.PayLoad = payLoad2;

         
         initialRequest.PayLoad = payLoad2;
         string json = initialRequest.Serialize();
         InitialConnectionRequest newRequest = InitialConnectionRequest.Deserialize(json);
         ConnectionKey newKey = newRequest.GetPublicKey();
         
         Console.WriteLine(newKey.Key);
         
         
          string json2 = request.Serialize();
          //Console.WriteLine(json2);
          
          Request newRequest2 = Request.Deserialize(json2);
          
         Console.WriteLine(newRequest2.RequestType); 
         Console.WriteLine(newRequest2.PayLoad.PayloadObject["PublicKey"]);
          
          
        //  PeerInfoRequest peerInfoRequest = PeerInfoRequest.Deserialize(json2);
          
        //  PeerInfoModel peerInfoModel = peerInfoRequest.GetPeerInfo();

       //   Console.WriteLine(peerInfoModel.IpAddress);


    }
}