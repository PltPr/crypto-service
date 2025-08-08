using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using api.Interfaces;

namespace api.Services
{
    public class BybitService : IBybitService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _client;
        public BybitService(IConfiguration config, IHttpClientFactory client)
        {
            _config = config;
            _client = client;

        }

        public async Task<JsonElement> BuyAsync(string symbol, decimal amount)
        {
            var apiKey = _config["Bybit:ApiKey"];
            var apiSecret = _config["Bybit:ApiSecret"];
            var client = _client.CreateClient();
            client.BaseAddress = new Uri("https://api.bybit.com");

            decimal price = await GetPriceForCrypto(symbol);
            if (price <= 0)
                throw new Exception($"Nieprawidlowa cena krypto: {symbol}");

            decimal qty = Math.Round(amount / price, 3);

            string amountbody = amount.ToString(CultureInfo.InvariantCulture);

            var timestamp = (await GetTimestampAsync()).ToString();

            var body = new Dictionary<string, object>
            {
                {"category","spot"},
                {"symbol",symbol},
                {"side","buy"},
                {"orderType","Market"},
                {"marketUnit", "quoteCoin"},
                {"qty",amountbody},
                {"timestamp",timestamp},
                {"api_key",apiKey}
            };


            var sorted = body
            .OrderBy(kv => kv.Key)
            .Select(kv => $"{kv.Key}={kv.Value}")
            .ToArray();

            var queryString = string.Join("&", sorted);

            var signature = GenerateSignature(queryString, apiSecret);
            body.Add("sign", signature);

            var content = new StringContent(JsonSerializer.Serialize(body));
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await client.PostAsync("/v5/order/create", content);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.Clone();
        }

        public string GenerateSignature(string queryString, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(queryString));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        public async Task<JsonElement> GetBalanceAsync()
        {
            var apiKey = _config["Bybit:ApiKey"];
            var apiSecret = _config["Bybit:ApiSecret"];

            var client = _client.CreateClient();
            client.BaseAddress = new Uri("https://api.bybit.com");
            var endpoint = "/v5/account/wallet-balance";

            var timestamp = (await GetTimestampAsync()).ToString();

            var queryString = $"accountType=UNIFIED&api_key={apiKey}&timestamp={timestamp}";

            var signature = GenerateSignature(queryString, apiSecret);

            var fullQuery = $"{queryString}&sign={signature}";

            var url = endpoint + "?" + fullQuery;

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();

            using var jsonDoc = JsonDocument.Parse(jsonString);
            return jsonDoc.RootElement.Clone();
        }

        public async Task<JsonElement> GetCryptoBalance(string? symbol = "USDT")
        {
            symbol ??= "USDT";

            var apiKey = _config["Bybit:ApiKey"];
            var apiSecret = _config["Bybit:ApiSecret"];

            var client = _client.CreateClient();
            client.BaseAddress = new Uri("https://api.bybit.com");

            var timestamp = (await GetTimestampAsync()).ToString();
            var recvWindow = "10000";

            var balanceQuery = $"accountType=UNIFIED&api_key={apiKey}&coin={symbol}&recvWindow={recvWindow}&timestamp={timestamp}";

            var balanceSign = GenerateSignature(balanceQuery, apiSecret);
            var endpoint = "/v5/account/wallet-balance";

            var url = $"{endpoint}?{balanceQuery}&sign={balanceSign}";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(jsonString);

            var root = jsonDoc.RootElement;

            if (root.GetProperty("retCode").GetInt32() != 0)
                throw new Exception("Api returner error: " + root.GetProperty("retMsg").GetString());

            var balances = root.GetProperty("result").GetProperty("list")[0].GetProperty("coin")[0].GetProperty("walletBalance").Clone();

            return balances;
        }

        public async Task<decimal> GetPrceisionAsync(string symbol)
        {
            var client = _client.CreateClient();
            string url = $"https://api.bybit.com/v5/market/instruments-info?category=spot&symbol={symbol}";

            var response = await client.GetStringAsync(url);
            
            var json = JsonDocument.Parse(response);

            var precisionStr = json.RootElement
            .GetProperty("result")
            .GetProperty("list")[0]
            .GetProperty("lotSizeFilter")
            .GetProperty("basePrecision")
            .ToString();

            decimal basePrecision = decimal.Parse(precisionStr, CultureInfo.InvariantCulture);
            
            return basePrecision;
        }

        public async Task<decimal> GetPriceForCrypto(string symbol)
        {
            var apiKey = _config["Bybit:ApiKey"];

            var client = _client.CreateClient();
            client.BaseAddress = new Uri("https://api.bybit.com");
            var url = $"/v5/market/tickers?category=spot&symbol={symbol}";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var priceString = root
            .GetProperty("result")
            .GetProperty("list")[0]
            .GetProperty("lastPrice")
            .GetString();

            decimal.TryParse(priceString, CultureInfo.InvariantCulture, out var price);

            return price;

        }

        public async Task<long> GetTimestampAsync()
        {
            var client = _client.CreateClient();
            client.BaseAddress = new Uri("https://api.bybit.com");

            var response = await client.GetAsync("/v5/market/time");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            using var json = JsonDocument.Parse(content);
            var ts = json.RootElement.GetProperty("time").GetInt64();
            return ts;
        }

        public async Task<decimal> GetTotalEquity()
        {
            var apiKey = _config["Bybit:ApiKey"];
            var apiSecret = _config["Bybit:ApiSecret"];

            var client = _client.CreateClient();
            client.BaseAddress = new Uri("https://api.bybit.com");
            var endpoint = "/v5/account/wallet-balance";

            var timestamp = (await GetTimestampAsync()).ToString();

            var queryString = $"accountType=UNIFIED&api_key={apiKey}&timestamp={timestamp}";

            var signature = GenerateSignature(queryString, apiSecret);

            var fullQuery = $"{queryString}&sign={signature}";

            var url = endpoint + "?" + fullQuery;

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();

            using var jsonDoc = JsonDocument.Parse(jsonString);

            var root = jsonDoc.RootElement;

            var totalEquity = root
                .GetProperty("result")
                .GetProperty("list")[0]
                .GetProperty("totalEquity")
                .GetString();

            var result = decimal.Parse(totalEquity, System.Globalization.CultureInfo.InvariantCulture);
            
            return result;
        }

        public async Task<JsonElement> SellAllAsync(string symbol)
        {
            var apiKey = _config["Bybit:ApiKey"];
            var apiSecret = _config["Bybit:ApiSecret"];
            var client = _client.CreateClient();
            client.BaseAddress = new Uri("https://api.bybit.com");

            decimal precision = await GetPrceisionAsync(symbol);
            var prec = 1 / precision;

            var balanceElement = await GetCryptoBalance(symbol.Replace("USDT", ""));
            decimal rawQty = decimal.Parse(balanceElement.GetString(), CultureInfo.InvariantCulture);
            decimal qty = Math.Floor(rawQty * prec)/prec;

            


            if (qty <= 0)
                throw new Exception("Brak środków do sprzedazy");

            
            Console.WriteLine(qty.ToString(CultureInfo.InvariantCulture));

            var timestamp = (await GetTimestampAsync()).ToString();
            

            var body = new Dictionary<string, object>
            {
                { "category", "spot" },
                { "symbol", symbol },
                { "side", "Sell" },
                { "orderType", "Market" },
                {"marketUnit","baseCoin"},
                { "qty", qty.ToString("0.000", CultureInfo.InvariantCulture) },
                { "timestamp", timestamp },
                { "api_key", apiKey }
            };

            var sorted = body
                .OrderBy(kv => kv.Key)
                .Select(kv => $"{kv.Key}={kv.Value}")
                .ToArray();

            var queryString = string.Join("&", sorted);
            var signature = GenerateSignature(queryString, apiSecret);
            body.Add("sign", signature);

            var content = new StringContent(JsonSerializer.Serialize(body));
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await client.PostAsync("/v5/order/create", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.Clone();
        }
    }
}