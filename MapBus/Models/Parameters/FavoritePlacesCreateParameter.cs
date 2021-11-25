using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapBus.Models.Parameters
{
    public class FavoritePlacesCreateParameter
    {
        public string Token { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public double Lat { get; set; }

        public double Lng { get; set; }
    }
}
