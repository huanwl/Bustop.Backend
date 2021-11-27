using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapBus.Models
{
    public class RouteModel
    {
        public string RouteUID { get; set; }

        public string RouteName { get; set; }

        public string SubRouteUID { get; set; }

        public string SubRouteName { get; set; }

        public int Direction { get; set; }

        public List<StopModel> Stops { get; set; }

        public RouteModel()
        {
            Stops = new List<StopModel>();
        }
    }
}
