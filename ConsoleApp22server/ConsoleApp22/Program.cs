using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Client
{
    static void Main()
    {
        Console.WriteLine("1 News 2 Announcements 3 Tech ");
        string[] subs = Console.ReadLine().Split(',');

        UdpClient client = new UdpClient(5000);
        foreach (var s in subs)
        {
            switch (s.Trim())
            {
                case "1": client.JoinMulticastGroup(IPAddress.Parse("239.0.0.1")); break;
                case "2": client.JoinMulticastGroup(IPAddress.Parse("239.0.0.2")); break;
                case "3": client.JoinMulticastGroup(IPAddress.Parse("239.0.0.3")); break;
            }
        }

        UdpClient broadcastClient = new UdpClient(5001);
        IPEndPoint any = new IPEndPoint(IPAddress.Any, 0);

        while (true)
        {
            if (client.Available > 0)
            {
                byte[] data = client.Receive(ref any);
                Console.WriteLine("[MULTICAST] " + Encoding.UTF8.GetString(data));
            }
            if (broadcastClient.Available > 0)
            {
                byte[] data = broadcastClient.Receive(ref any);
                Console.WriteLine("[BROADCAST] " + Encoding.UTF8.GetString(data));
            }
        }
    }
}
