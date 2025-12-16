using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using System.Text;

public class Client : MonoBehaviour
{
    // broadcast address
    [SerializeField] private string host;
    [SerializeField] private int port = 3333;
    [SerializeField] private int do_you_send = 0;

    private readonly int CLIENT_SESSION_ID = 123;

    private UdpClient client;


    void Start()
    {
        host = GetLocalIPAddress();
        client = new UdpClient();
        
        //client.Connect(host, port);
        
        InvokeRepeating("sendText", 0f, 0.1f);
    }

    public string GetLocalIPAddress()
    {
        string hostName = Dns.GetHostName();
        IPHostEntry ipEntry = Dns.GetHostEntry(hostName);

        foreach (IPAddress ip in ipEntry.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        return "Local IP not found";
    }

    void Update()
    {
    }

    void sendText()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(host), port);

        string message = "hello" + GetLocalIPAddress();
        byte[] payload = Encoding.UTF8.GetBytes(message);

        //string message = "hello! Time: " + Time.time.ToString("F2");

        byte[] finalPacket = NetWorkStruct.makePayload(
            NetWorkStruct.NetMessageType.Text,
            CLIENT_SESSION_ID,
            payload
        );

        if (do_you_send > 0)
        {
            client.Send(finalPacket, finalPacket.Length, remoteEP);
        }
    }

    void OnApplicationQuit()
    {
        client.Close();
    }
}