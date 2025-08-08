using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.TradeDto;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace api.Services
{
    public class FlowService : IFlowService
    {
        private readonly IBybitService _bybitService;
        private readonly IPredictService _predictService;
        private readonly ApplicationDBContext _context;
        public FlowService(IBybitService bybitService, IPredictService predictService, ApplicationDBContext context)
        {
            _bybitService = bybitService;
            _predictService = predictService;
            _context = context;
        }
        public async Task<TradeTimeLine> Flow(string symbol)
        {
            string side = null;
            string signal = await _predictService.GetStatusForCrypto(symbol, "15 minutes");
            if (signal == null || string.IsNullOrEmpty(signal))
                throw new Exception("Signal error");


            var isInvested = await IsInvested(symbol);

            switch (signal)
            {
                case "Strong Sell":
                    if (isInvested)
                    {
                        //await _bybitService.SellAllAsync(symbol);
                        side = "Sell";
                        await SummaryTransaction(symbol, side);
                        isInvested = false;
                    }
                    else
                    {
                        side = "Waiting/Sell";
                    }
                    break;

                case "Sell":
                    if (isInvested)
                    {
                        //await _bybitService.SellAllAsync(symbol);
                        side = "Sell";
                        await SummaryTransaction(symbol, side);
                        isInvested = false;
                    }
                    else
                    {
                        side = "Waiting/Sell";
                    }
                    break;

                case "Neutral":
                    side = "Hold";
                    break;

                case "Buy":
                    if (isInvested)
                    {
                        side = "Waiting/Buy";
                    }
                    else
                    {
                        //await _bybitService.BuyAsync(symbol, 7);
                        side = "Buy";
                        await SummaryTransaction(symbol, side);
                        isInvested = true;
                    }
                    break;

                case "Strong Buy":
                    if (isInvested)
                    {
                        side = "Waiting/Buy";
                    }
                    else
                    {
                        //await _bybitService.BuyAsync(symbol, 7);
                        side = "Buy";
                        isInvested = true;
                        await SummaryTransaction(symbol, side);
                        
                    }
                    break;
            }
            var saldo = await _bybitService.GetTotalEquity();

            var model = new TradeTimeLine
            {
                Date = DateTime.UtcNow,
                Symbol = symbol,
                Signal = signal,
                IsInvested=isInvested,
                Side = side,
                Saldo = saldo
            };

            await SaveFlowToDb(model);
            return model;
        }

        public async Task<TradeTimeLine> InitDB(string symbol)
        {
            var model = new TradeTimeLine
            {
                Date = DateTime.UtcNow,
                Symbol = symbol,
                Signal = "Init",
                IsInvested=false,
                Side = "Init",
                Saldo = await _bybitService.GetTotalEquity()
            };
            await _context.TradeTimeLines.AddAsync(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<bool> IsInvested(string symbol)
        {
            var lastAction = await _context.TradeTimeLines.Where(x=>x.Symbol==symbol).OrderByDescending(x => x.Date).FirstAsync();
            if (lastAction.IsInvested==false) return false;
            return true;

        }

        public async Task<TradeTimeLine> SaveFlowToDb(TradeTimeLine model)
        {
            await _context.TradeTimeLines.AddAsync(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<TradeSummary> SummaryTransaction(string symbol, string side)
        {
            var price = await _bybitService.GetPriceForCrypto(symbol);

            var summary = new TradeSummary
            {
                Date = DateTime.UtcNow,
                Symbol = symbol,
                Side = side,
                Price = price
            };
            await _context.TradeSummaries.AddAsync(summary);
            await _context.SaveChangesAsync();

            return summary;
        }
    }
}