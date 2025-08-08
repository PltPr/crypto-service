using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Interfaces
{
    public interface IPredictService
    {
        Task<string?> GetStatusForCrypto(string? symbol = "BTCUSDT", string? interval = "1 hour");
    }
}