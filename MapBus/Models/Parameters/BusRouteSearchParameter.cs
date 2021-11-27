using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapBus.Models.Parameters
{
    public class BusRouteSearchParameter
    {
        public string Token { get; set; }

        public double From_Lat { get; set; }

        public double From_Lng { get; set; }

        public double To_Lat { get; set; }

        public double To_Lng { get; set; }
    }
}
