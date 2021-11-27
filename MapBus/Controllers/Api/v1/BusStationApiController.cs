using MapBus.Constants;
using MapBus.Models;
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
    public class BusStationApiController : BaseApiController
    {
        private static object _sync = new object();
        private readonly IMemoryCache _cache;

        public BusStationApiController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpGet]
        [Route("/api/bus_stations/initial/{cityName}")]
        public IActionResult Initial(string cityName)
        {
            List<StationModel> stations;

            var cachekey = CacheKeyConst.GetStationKey(cityName);
            if (!_cache.TryGetValue(cachekey, out stations))
            {
                lock (_sync)
                {
                    if (!_cache.TryGetValue(cachekey, out stations))
                    {
                        stations = GetData(cityName);

                        _cache.Set(cachekey, stations, new MemoryCacheEntryOptions
                        {
                            AbsoluteExpiration = DateTimeOffset.Now.AddHours(3)
                        });
                    }
                }
            }

            var result = new ResultModel
            {
                Success = true,
                Data = stations.Take(20)
            };
            return CrossOrigin_Ok(result);
        }

        private List<StationModel> GetData(string key)
        {
            var stations = new List<StationModel>();
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", _authorization);
                httpClient.DefaultRequestHeaders.Add("x-date", _xdate);

                var url = $"https://ptx.transportdata.tw/MOTC/v2/Bus/Station/City/{key}?$top=7000&$format=JSON";
                var data = httpClient.GetStringAsync(url).Result;
                var jarray = JsonConvert.DeserializeObject<JArray>(data);
                foreach (var a in jarray)
                {
                    var station = new StationModel
                    {
                        StationUID = a["StationUID"].ToString(),
                        StationName = a["StationName"]["Zh_tw"].ToString(),
                        PositionLat = a["StationPosition"]["PositionLat"].Value<double>(),
                        PositionLng = a["StationPosition"]["PositionLon"].Value<double>()
                    };

                    foreach (var b in a["Stops"])
                    {
                        var routeUID = b["RouteUID"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(routeUID))
                        {
                            station.RouteStops.Add(new RouteStopModel
                            {
                                RouteUID = b["RouteUID"].ToString(),
                                StopUID = b["StopUID"].ToString(),
                                StopName = b["StopName"]["Zh_tw"].ToString(),
                                PositionLat = station.PositionLat,
                                PositionLng = station.PositionLng
                            });
                        }
                    }

                    stations.Add(station);
                }
            }
            return stations;
        }
    }
}
