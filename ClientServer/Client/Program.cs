using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    internal class Program
    {
        static async Task Main()
        {
            if (!IsServerRunning())
            {
                Process.Start("ServerApp.exe");
                await Task.Delay(1000);
            }

            while (true)
            {
                Console.Write("Введіть команду ");
                string cmd = Console.ReadLine();

                if (cmd == "exit") break;

                string response = await SendRequestAsync(cmd);
                Console.WriteLine($"Відповідь {response}\n");
            }
        }

        static async Task<string> SendRequestAsync(string message)
        {
            using var client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 5000);
            using var stream = client.GetStream();

            byte[] data = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(data, 0, data.Length);

            var buffer = new byte[1024];
            int bytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytes);
        }

        static bool IsServerRunning()
        {
            foreach (var p in Process.GetProcessesByName("ServerApp"))
                return true;
            return false;
        }
    }
}
