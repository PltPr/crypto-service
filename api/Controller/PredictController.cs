using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.PredictDto;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class PredictController : ControllerBase
    {
        private readonly IPredictService _predictService;
        public PredictController(IPredictService predictService)
        {
            _predictService = predictService;
        }

        [HttpPost]
        public async Task<IActionResult> GetStatusForCrypto([FromBody] RequestStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

        
            var result = await _predictService.GetStatusForCrypto(dto.Symbol, dto.Interval);

            return Ok(result);
        }
    }
}