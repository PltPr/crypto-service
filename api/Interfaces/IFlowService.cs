using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.TradeDto;
using api.Models;

namespace api.Interfaces
{
    public interface IFlowService
    {
        Task<TradeTimeLine> Flow(string symbol);
        Task<TradeTimeLine> SaveFlowToDb(TradeTimeLine model);
        Task<TradeTimeLine> InitDB(string symbol);
        Task<bool> IsInvested(string symbol);
        Task<TradeSummary> SummaryTransaction(string symbol, string side);
    }
}