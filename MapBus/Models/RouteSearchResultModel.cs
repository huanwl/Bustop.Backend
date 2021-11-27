using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapBus.Models
{
    public class RouteSearchResultModel
    {
        public string RouteUID { get; set; }

        public string RouteName { get; set; }

        public int GapStopCount { get; set; }

        public int FirstStopDistance { get; set; }

        public int LastStopDistance { get; set; }

        public List<StopModel> Stops { get; set; }
    }
}
