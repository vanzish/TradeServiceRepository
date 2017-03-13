using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeService.Models
{
    public class TradeRecordsWithHeader
    {
        public TradeRecordsWithHeader()
        {
            TradeRecords = new List<TradeRecord>();
        }
        public Header Header { get; set; }
        public List<TradeRecord> TradeRecords { get; set; }
    }
}
