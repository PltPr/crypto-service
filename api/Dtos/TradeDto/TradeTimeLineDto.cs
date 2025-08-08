using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.TradeDto
{
    public class TradeTimeLineDto
    {
        public DateTime Date { get; set; }
        public string Symbol { get; set; }
        public string Signal { get; set; }
        public string Side { get; set; }
        public decimal Saldo { get; set; }
    }
}