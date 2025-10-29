using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

class Server
{
    static Dictionary<string, IPEndPoint> clients = new();
    static UdpClient multicast = new UdpClient();
    static UdpClient broadcast = new UdpClient();

    static IPAddress newsGroup = IPAddress.Parse("239.0.0.1");
    static IPAddress announcementsGroup = IPAddress.Parse("239.0.0.2");
    static IPAddress techGroup = IPAddress.Parse("239.0.0.3");
    static int multicastPort = 5000;
    static int broadcastPort = 5001;

    static void Main()
    {
        multicast.JoinMulticastGroup(newsGroup);
        multicast.JoinMulticastGroup(announcementsGroup);
        multicast.JoinMulticastGroup(techGroup);
        broadcast.EnableBroadcast = true;

        Console.WriteLine("Server started");

        while (true)
        {
            Console.WriteLine("\n1 News 2 Announcements 3 Tech 4 Broadcast 5 Remove client");
            string cmd = Console.ReadLine();
            if (cmd == "5")
            {
                Console.Write("Enter client IP to remove ");
                string ip = Console.ReadLine();
                clients.Remove(ip);
                continue;
            }

            Console.Write("Message ");
            string msg = Console.ReadLine();
            byte[] data = Encoding.UTF8.GetBytes(msg);

            switch (cmd)
            {
                case "1": multicast.Send(data, data.Length, new IPEndPoint(newsGroup, multicastPort)); break;
                case "2": multicast.Send(data, data.Length, new IPEndPoint(announcementsGroup, multicastPort)); break;
                case "3": multicast.Send(data, data.Length, new IPEndPoint(techGroup, multicastPort)); break;
                case "4": broadcast.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, broadcastPort)); break;
            }
        }
    }
}
