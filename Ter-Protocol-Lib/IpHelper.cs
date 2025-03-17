using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Utilities;

namespace Ter_Protocol_Lib;

public class IpHelper
{
    private const string StunServer = "stun.l.google.com";
    private const int StunPort = 19302;
    private const int MagicCookie = 0x2112A442;
    
    public async Task<string> GetExternalAddress()
    {
        try
        {
            using (var client = new UdpClient())
            {
                client.Connect(StunServer, StunPort);
                
                byte[] stunRequest = CreateStunBindingRequest();
                client.Send(stunRequest, stunRequest.Length);
                
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, StunPort);
                byte[] response = client.Receive(ref remoteEP);
                
                return ParseStunResponse(response);
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"Error: {ex.Message}", LogLevel.Error, Source.Protocol);
            return "Error";
        }
    }

    private static byte[] CreateStunBindingRequest()
    {
        byte[] transactionId = new byte[12];
        new Random().NextBytes(transactionId);

        byte[] request = new byte[20];
        request[0] = 0x00; request[1] = 0x01;
        request[2] = 0x00; request[3] = 0x00;
        request[4] = 0x21; request[5] = 0x12;
        request[6] = 0xA4; request[7] = 0x42;
        
        Buffer.BlockCopy(transactionId, 0, request, 8, 12);
        return request;
    }

    private static string ParseStunResponse(byte[] response)
    {
        int index = 20;

        while (index < response.Length)
        {
            int type = (response[index] << 8) | response[index + 1];
            int length = (response[index + 2] << 8) | response[index + 3];
            index += 4;

            if (type == 0x0020)
            {
                int family = response[index + 1];
                int xorPort = ((response[index + 2] << 8) | response[index + 3]) ^ (MagicCookie >> 16);
                int xorIP = BitConverter.ToInt32(response, index + 4) ^ MagicCookie;

                IPAddress externalIP = new IPAddress(BitConverter.GetBytes(xorIP));
                string ip = externalIP.ToString() + ":" + xorIP.ToString();
                return ip;
            }

            index += length;
        }

        return "";
    }
    public string GetLocalIPAddress()
    {
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && ni.OperationalStatus == OperationalStatus.Up)
            {
                foreach (UnicastIPAddressInformation address in ni.GetIPProperties().UnicastAddresses)
                {
                    if (address.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return address.Address.ToString();
                    }
                }
            }
        }
        return "IP address not found.";
    }
    
    
}