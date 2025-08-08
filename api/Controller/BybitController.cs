using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Interfaces;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class BybitController : ControllerBase
    {
        private readonly IBybitService _bybitservice;
        private readonly IFlowService _flowService;
        public BybitController(IBybitService bybitService, IFlowService flowService)
        {
            _bybitservice = bybitService;
            _flowService = flowService;
        }
        [HttpGet]
        public async Task<IActionResult> GetBalance()
        {
            var response = await _bybitservice.GetBalanceAsync();

            return Ok(response);
        }
        [HttpGet("{symbol}")]
        public async Task<IActionResult> GetBalanceBySymbol(string? symbol)
        {
            symbol ??= "USDT";
            var result = await _bybitservice.GetCryptoBalance(symbol);

            return Ok(result);
        }
        [HttpPost("Buy")]
        public async Task<IActionResult> BuyCrypto([FromBody] string symbol, decimal amount)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var result = await _bybitservice.BuyAsync(symbol, amount);

            return Ok(result);
        }
        [HttpGet("CryptoPrice")]
        public async Task<IActionResult> GetCryptoPrice(string symbol)
        {
            var result = await _bybitservice.GetPriceForCrypto(symbol);
            return Ok(result);
        }

        [HttpGet("SellAll")]
        public async Task<IActionResult> SellAll(string symbol)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var result = await _bybitservice.SellAllAsync(symbol);
            return Ok(result);
        }

        [HttpGet("GetTotalEquity")]
        public async Task<IActionResult> GetTotalEquity()
        {
            var result = await _bybitservice.GetTotalEquity();
            return Ok(result);
        }
        [HttpGet("InitDb")]
        public async Task<IActionResult> InitDb(string symbol)
        {
            var result = await _flowService.InitDB(symbol);
            return Ok(result);
        }

        [HttpGet("GetBasePrecisionForSymbol")]
        public async Task<IActionResult> GetBasePrecision(string symbol)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var result = await _bybitservice.GetPrceisionAsync(symbol);

            return Ok(result);
        }
    }
}