using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using RestSharp;


namespace ConsoleApp1
{
    internal class Program
    {
        static void Main()
        {
            string baseCurrency = "USD";
            string[] currencies = { "EUR", "UAH" };
            string url = $"https://api.exchangerate.host/latest?base={baseCurrency}&symbols={string.Join(",", currencies)}";

            var client = new RestClient(url);
            var request = new RestRequest();
            var response = client.Execute(request);

            if (response.IsSuccessful)
            {
                JObject json = JObject.Parse(response.Content);
                Console.WriteLine($"Курси валют вiдносно {baseCurrency}");

                foreach (var currency in currencies)
                {
                    double rate = json["rates"][currency].Value<double>();
                    Console.WriteLine($"{currency}: {rate:F2}");
                }
            }
            else
            {
                Console.WriteLine("error");
            }

            Console.ReadLine();
        }
    }
}
