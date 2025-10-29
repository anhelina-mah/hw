using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Client
{
    static char[,] map;
    static (int x, int y) blue = (1, 1);
    static (int x, int y) red = (1, 1);
    static int width = 20, height = 10;
    static int scoreBlue = 0;
    static bool running = true;

    static void Main()
    {
        try
        {
            TcpClient client = new TcpClient("127.0.0.1", 5000);
            NetworkStream stream = client.GetStream();

            string level = ReceiveMap(stream);
            LoadMap(level);

            Thread recv = new Thread(() => ReceiveServer(stream));
            recv.Start();

            DateTime start = DateTime.Now;
            while (running && (DateTime.Now - start).TotalSeconds < 60)
            {
                DrawMap();
                if (Console.KeyAvailable)
                {
                    ConsoleKey key = Console.ReadKey(true).Key;
                    Move(key);
                    string coords = $"{blue.x} {blue.y}";
                    byte[] data = Encoding.UTF8.GetBytes(coords);
                    stream.Write(data, 0, data.Length);
                }
                Thread.Sleep(100);
            }

            Console.WriteLine("Game over");
            client.Close();
        }
        catch
        {
            Console.WriteLine("Error");
        }
    }

    static void Move(ConsoleKey key)
    {
        (int nx, int ny) = blue;
        switch (key)
        {
            case ConsoleKey.LeftArrow: nx--; break;
            case ConsoleKey.RightArrow: nx++; break;
            case ConsoleKey.UpArrow: ny--; break;
            case ConsoleKey.DownArrow: ny++; break;
        }
        if (nx >= 0 && ny >= 0 && nx < width && ny < height && map[ny, nx] != '#')
        {
            if (map[ny, nx] == '$') scoreBlue++;
            blue = (nx, ny);
        }
    }

    static void DrawMap()
    {
        Console.Clear();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x == blue.x && y == blue.y) Console.ForegroundColor = ConsoleColor.Cyan;
                else if (x == red.x && y == red.y) Console.ForegroundColor = ConsoleColor.Red;
                else Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(map[y, x]);
            }
            Console.WriteLine();
        }
        Console.ResetColor();
        Console.WriteLine($"Blue: {scoreBlue}");
    }

    static string ReceiveMap(NetworkStream s)
    {
        byte[] buf = new byte[1024];
        int len = s.Read(buf, 0, buf.Length);
        return Encoding.UTF8.GetString(buf, 0, len);
    }

    static void LoadMap(string str)
    {
        string[] lines = str.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        height = lines.Length;
        width = lines[0].Length;
        map = new char[height, width];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                map[y, x] = lines[y][x];
    }

    static void ReceiveServer(NetworkStream s)
    {
        byte[] buf = new byte[256];
        while (running)
        {
            if (s.DataAvailable)
            {
                int len = s.Read(buf, 0, buf.Length);
                string msg = Encoding.UTF8.GetString(buf, 0, len);
                if (msg.Contains("wins") || msg.Contains("Draw"))
                {
                    Console.Clear();
                    Console.WriteLine(msg);
                    running = false;
                    break;
                }
            }
            Thread.Sleep(100);
        }
    }
}
