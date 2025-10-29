using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

class Server
{
    static char[,] map;
    static (int x, int y) red = (1, 1);
    static (int x, int y) blue = (1, 1);
    static int width = 20, height = 10;
    static int scoreRed = 0, scoreBlue = 0;
    static bool running = true;

    static void Main()
    {
        map = GenerateMap(width, height);
        TcpListener listener = new TcpListener(IPAddress.Any, 5000);
        listener.Start();
        TcpClient client = listener.AcceptTcpClient();
        NetworkStream stream = client.GetStream();

        SendMap(stream);
        Thread timerThread = new Thread(() =>
        {
            Thread.Sleep(60000);
            running = false;
        });
        timerThread.Start();

        Thread recv = new Thread(() => ReceiveClient(stream));
        recv.Start();

        while (running)
        {
            DrawMap();
            if (Console.KeyAvailable)
            {
                ConsoleKey key = Console.ReadKey(true).Key;
                Move(ref red, key);
            }
            Thread.Sleep(100);
        }

        string result = scoreRed > scoreBlue ? "Server wins" :
                        scoreRed < scoreBlue ? "Client wins" : "Draw";
        Console.WriteLine(result);
        byte[] msg = Encoding.UTF8.GetBytes(result);
        stream.Write(msg, 0, msg.Length);
        client.Close();
        listener.Stop();
    }

    static void ReceiveClient(NetworkStream s)
    {
        byte[] buf = new byte[256];
        while (running)
        {
            if (s.DataAvailable)
            {
                int len = s.Read(buf, 0, buf.Length);
                string data = Encoding.UTF8.GetString(buf, 0, len);
                string[] parts = data.Split(' ');
                if (parts.Length == 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
                {
                    if (map[y, x] != '#')
                    {
                        if (map[y, x] == '$') scoreBlue++;
                        blue = (x, y);
                    }
                }
            }
            Thread.Sleep(50);
        }
    }

    static void Move(ref (int x, int y) pos, ConsoleKey key)
    {
        (int nx, int ny) = pos;
        switch (key)
        {
            case ConsoleKey.LeftArrow: nx--; break;
            case ConsoleKey.RightArrow: nx++; break;
            case ConsoleKey.UpArrow: ny--; break;
            case ConsoleKey.DownArrow: ny++; break;
        }
        if (nx >= 0 && ny >= 0 && nx < width && ny < height && map[ny, nx] != '#')
        {
            if (map[ny, nx] == '$') scoreRed++;
            pos = (nx, ny);
        }
    }

    static void DrawMap()
    {
        Console.Clear();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x == red.x && y == red.y) Console.ForegroundColor = ConsoleColor.Red;
                else if (x == blue.x && y == blue.y) Console.ForegroundColor = ConsoleColor.Cyan;
                else Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(map[y, x]);
            }
            Console.WriteLine();
        }
        Console.ResetColor();
        Console.WriteLine($"Red {scoreRed} | Blue {scoreBlue}");
    }

    static char[,] GenerateMap(int w, int h)
    {
        var r = new Random();
        var m = new char[h, w];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                m[y, x] = (x == 0 || y == 0 || x == w - 1 || y == h - 1 || r.Next(0, 10) == 0) ? '#' : ' ';
        for (int i = 0; i < 5; i++)
            m[r.Next(1, h - 1), r.Next(1, w - 1)] = '$';
        m[h - 2, w - 2] = 'F';
        return m;
    }

    static void SendMap(NetworkStream s)
    {
        StringBuilder sb = new();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++) sb.Append(map[y, x]);
            sb.Append('\n');
        }
        byte[] data = Encoding.UTF8.GetBytes(sb.ToString());
        s.Write(data, 0, data.Length);
    }
}
