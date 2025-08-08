using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Interfaces;

namespace api.Services
{
    public class TradeService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public TradeService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {

                    var _predictService = scope.ServiceProvider.GetRequiredService<IPredictService>();
                    var _bybitservice = scope.ServiceProvider.GetRequiredService<IBybitService>();
                    var _flowService = scope.ServiceProvider.GetRequiredService<IFlowService>();
                    try
                    {
                        await _flowService.Flow("PENGUUSDT");
                        await _flowService.Flow("SOLUSDT");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"error: {ex}");
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(7), stoppingToken);
            }
        }
    }
}