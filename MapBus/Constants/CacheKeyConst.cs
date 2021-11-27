using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapBus.Constants
{
    public static class CacheKeyConst
    {
        public static string GetStationKey(string subkey)
        {
            return $"_stations_{subkey}".ToLower();
        }

        public static string GetRouteKey(string subkey)
        {
            return $"_routes_{subkey}".ToLower();
        }
    }
}
