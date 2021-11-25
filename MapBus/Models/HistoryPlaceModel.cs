using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapBus.Models
{
    public class HistoryPlaceModel
    {
        public string name { get; set; }

        public string address { get; set; }

        public double lat { get; set; }

        public double lng { get; set; }

        public DateTime date_in { get; set; }
    }
}
