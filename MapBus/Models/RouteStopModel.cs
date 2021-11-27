using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapBus.Models
{
    public class RouteStopModel
    {
        public string RouteUID { get; set; }

        public string RouteName { get; set; }

        public string StopUID { get; set; }

        public string StopName { get; set; }

        public double PositionLat { get; set; }

        public double PositionLng { get; set; }

        public int Distance { get; set; }

        public int Seconds
        {
            get
            {
                return (Distance * 100) / 70;
            }
        }
    }
}
