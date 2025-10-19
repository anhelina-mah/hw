using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace ServerApp
{
    internal class Program
    {
        static async Task Main()
        {
            var listener = new TcpListener(IPAddress.Loopback, 5000);
            listener.Start();

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClient(client));
            }
        }

        static async Task HandleClient(TcpClient client)
        {
            using var stream = client.GetStream();
            var buffer = new byte[1024];
            int bytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            string request = Encoding.UTF8.GetString(buffer, 0, bytes);

            string response = await ProcessRequestAsync(request.Trim());
            byte[] data = Encoding.UTF8.GetBytes(response);
            await stream.WriteAsync(data, 0, data.Length);

            client.Close();
        }

        static async Task<string> ProcessRequestAsync(string request)
        {
            switch (request.ToLower())
            {
                case "time":
                    return DateTime.Now.ToString("HH:mm:ss");
                case "date":
                    return DateTime.Now.ToString("dd.MM.yyyy");
                case "eur":
                    return await GetCurrencyRate("EUR", "UAH");
                case "btc":
                    return await GetBitcoinRate();
                default:
                    if (request.StartsWith("weather "))
                    {
                        string city = request.Substring(8);
                        return await GetWeather(city);
                    }
                    return "error";
            }
        }

        static async Task<string> GetCurrencyRate(string from, string to)
        {
            var client = new RestClient($"https://api.exchangerate.host/latest?base={from}&symbols={to}");
            var response = await client.ExecuteAsync(new RestRequest());
            if (!response.IsSuccessful) return "error";
            JObject json = JObject.Parse(response.Content);
            double rate = json["rates"][to].Value<double>();
            return $"Курс {from}/{to}: {rate:F2}";
        }

        static async Task<string> GetBitcoinRate()
        {
            var client = new RestClient("https://api.coindesk.com/v1/bpi/currentprice/UAH.json");
            var response = await client.ExecuteAsync(new RestRequest());
            if (!response.IsSuccessful) return "error";
            JObject json = JObject.Parse(response.Content);
            double rate = json["bpi"]["UAH"]["rate_float"].Value<double>();
            return $"Курс BTC/UAH: {rate:F0} грн";
        }

        static async Task<string> GetWeather(string city)
        {
            string apiKey = "YOUR_API_KEY";
            var client = new RestClient($"https://api.openweathermap.org/data/2.5/weather?q={city}&units=metric&lang=ua&appid={apiKey}");
            var response = await client.ExecuteAsync(new RestRequest());
            if (!response.IsSuccessful) return "error";

            JObject json = JObject.Parse(response.Content);
            string cond = json["weather"][0]["description"].ToString();
            double temp = json["main"]["temp"].Value<double>();
            double wind = json["wind"]["speed"].Value<double>();
            int hum = json["main"]["humidity"].Value<int>();
            return $"Погода у {city}: {cond}, {temp}°C, вітер {wind} мс, вологість {hum}%";
        }
    }
}
