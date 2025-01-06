using Client;
using Server;

namespace CoreLib;

public class Node
{
    public Node()
    {
    }
    
    public void Start()
    {
        while (true)
        {
            TcpServer tcpServer = new TcpServer();
            tcpServer.StartAsync();
            
        }
        
    }
    
    

}