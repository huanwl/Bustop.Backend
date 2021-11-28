using GeoCoordinatePortable;
using MapBus.Constants;
using MapBus.Models;
using MapBus.Models.Parameters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace MapBus.Controllers.Api.v1
{
    public class BusStopApiController : BaseApiController
    {
        private static object _sync = new object();
        private readonly IMemoryCache _cache;

        public BusStopApiController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpPost]
        [Route("/api/bus_stops/search")]
        public IActionResult Search([FromForm]BusStopSearchParameter parameter)
        {
            var result = new ResultModel();

            Guid userId;
            if (!Guid.TryParse(parameter.Token, out userId))
            {
                result.Success = false;
                result.Message = "Not login";
                return CrossOrigin_Ok(result);
            }

            var stations = LoadStationCache();
            if (stations == null)
            {
                result.Success = false;
                result.Message = "Not initial";
                return CrossOrigin_Ok(result);
            }

            // 找出周圍站點
            var nearRoutes = stations.FindAll(x => 
                x.PositionLat > parameter.Lat - 0.004 && 
                x.PositionLat < parameter.Lat + 0.004 && 
                x.PositionLng > parameter.Lng - 0.005 && 
                x.PositionLng < parameter.Lng + 0.005
                )
                .SelectMany(x => x.RouteStops)
                .ToList();

            // 計算距離並以近到遠排序
            nearRoutes.ForEach(x => {
                var sCoord = new GeoCoordinate(parameter.Lat, parameter.Lng);
                var eCoord = new GeoCoordinate(x.PositionLat, x.PositionLng);
                x.Distance = (int)sCoord.GetDistanceTo(eCoord);
            });
            nearRoutes = nearRoutes.OrderBy(x => x.Distance).ToList();

            result.Success = true;
            result.Data = nearRoutes;
            return CrossOrigin_Ok(result);
        }

        private List<StationModel> LoadStationCache()
        {
            var result = new List<StationModel>();
            foreach (var city in CityConst.EnglishNames)
            {
                List<StationModel> stations;
                var cachekey = CacheKeyConst.GetStationKey(city);
                if (_cache.TryGetValue(cachekey, out stations))
                {
                    result.AddRange(stations);
                }
            }
            return result;
        }
    }
}
