using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapBus.Models.Parameters
{
    public class BusStopSearchParameter
    {
        public string Token { get; set; }

        public double Lat { get; set; }

        public double Lng { get; set; }
    }
}
