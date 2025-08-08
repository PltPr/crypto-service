using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.PredictDto
{
    public class RequestStatusDto
    {
        public string? Symbol { get; set; }
        public string? Interval { get; set; }
    }
}