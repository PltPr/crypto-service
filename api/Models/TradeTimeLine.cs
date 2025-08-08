using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class TradeTimeLine
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Symbol { get; set; }
        public string Signal { get; set; }
        public bool IsInvested { get; set; }
        public string Side { get; set; }
        public decimal Saldo { get; set; }
    }
}