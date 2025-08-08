using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace api.Interfaces
{
    public interface IBybitService
    {
        Task<JsonElement> GetBalanceAsync();
        Task<decimal> GetTotalEquity();
        Task<long> GetTimestampAsync();
        string GenerateSignature(string queryString, string secret);
        Task<JsonElement> GetCryptoBalance(string symbol = "USDT");
        Task<decimal> GetPriceForCrypto(string symbol);
        Task<JsonElement> BuyAsync(string symbol, decimal amount);
        Task<JsonElement> SellAllAsync(string symbol);
        Task<decimal> GetPrceisionAsync(string symbol);
    }
}