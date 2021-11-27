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
    public class BusRouteApiController : BaseApiController
    {
        private static object _sync = new object();
        private readonly IMemoryCache _cache;

        public BusRouteApiController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpGet]
        [Route("/api/bus_routes/initial/{cityName}")]
        public IActionResult Initial(string cityName)
        {
            List<RouteModel> routes;

            var cachekey = CacheKeyConst.GetRouteKey(cityName);
            if (!_cache.TryGetValue(cachekey, out routes))
            {
                lock (_sync)
                {
                    if (!_cache.TryGetValue(cachekey, out routes))
                    {
                        routes = GetData(cityName);

                        _cache.Set(cachekey, routes, new MemoryCacheEntryOptions
                        {
                            AbsoluteExpiration = DateTimeOffset.Now.AddHours(3)
                        });
                    }
                }
            }

            var result = new ResultModel
            {
                Success = true,
                Data = routes.Take(20)
            };
            return CrossOrigin_Ok(result);
        }

        private List<RouteModel> GetData(string key)
        {
            var routes = new List<RouteModel>();
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", _authorization);
                httpClient.DefaultRequestHeaders.Add("x-date", _xdate);

                var url = $"https://ptx.transportdata.tw/MOTC/v2/Bus/StopOfRoute/City/{key}?$top=2000&$format=JSON";
                var data = httpClient.GetStringAsync(url).Result;
                var jarray = JsonConvert.DeserializeObject<JArray>(data);
                foreach (var a in jarray)
                {
                    var route = new RouteModel
                    {
                        RouteUID = a["RouteUID"].ToString(),
                        RouteName = a["RouteName"]["Zh_tw"].ToString(),
                        SubRouteUID = a["SubRouteUID"].ToString(),
                        SubRouteName = a["SubRouteName"]["Zh_tw"].ToString(),
                        Direction = a["Direction"].Value<int>()
                    };

                    foreach (var b in a["Stops"])
                    {
                        var stop = new StopModel
                        {
                            StopUID = b["StopUID"].ToString(),
                            StopName = b["StopName"]["Zh_tw"].ToString(),
                            PositionLat = b["StopPosition"]["PositionLat"].Value<double>(),
                            PositionLng = b["StopPosition"]["PositionLon"].Value<double>(),
                            StopSequence = b["StopSequence"].Value<int>()
                        };
                        route.Stops.Add(stop);
                    }

                    routes.Add(route);
                }
            }
            return routes;
        }

        [HttpPost]
        [Route("/api/bus_routes/search")]
        public IActionResult Search([FromBody]BusRouteSearchParameter parameter)
        {
            var result = new ResultModel();

            Guid userId;
            if (!Guid.TryParse(parameter.Token, out userId))
            {
                result.Success = false;
                result.Message = "Not login";
                return CrossOrigin_Ok(result);
            }

            var routes = LoadRouteCache();
            var stations = LoadStationCache();
            if (routes == null || stations == null)
            {
                result.Success = false;
                result.Message = "Not initial";
                return CrossOrigin_Ok(result);
            }

            // 找出目的地和出發地周圍站點
            var nearFromRoutes = stations.FindAll(x => 
                x.PositionLat > parameter.From_Lat - 0.004 && 
                x.PositionLat < parameter.From_Lat + 0.004 && 
                x.PositionLng > parameter.From_Lng - 0.005 && 
                x.PositionLng < parameter.From_Lng + 0.005
                )
                .SelectMany(x => x.RouteStops)
                .ToList();
            var nearToRoutes = stations.FindAll(x =>
                x.PositionLat > parameter.To_Lat - 0.004 &&
                x.PositionLat < parameter.To_Lat + 0.004 &&
                x.PositionLng > parameter.To_Lng - 0.005 &&
                x.PositionLng < parameter.To_Lng + 0.005
                )
                .SelectMany(x => x.RouteStops)
                .ToList();

            // 計算距離並以近到遠排序
            nearFromRoutes.ForEach(x => {
                var sCoord = new GeoCoordinate(parameter.From_Lat, parameter.From_Lng);
                var eCoord = new GeoCoordinate(x.PositionLat, x.PositionLng);
                x.Distance = (int)sCoord.GetDistanceTo(eCoord);
            });
            nearToRoutes.ForEach(x => {
                var sCoord = new GeoCoordinate(parameter.From_Lat, parameter.From_Lng);
                var eCoord = new GeoCoordinate(x.PositionLat, x.PositionLng);
                x.Distance = (int)sCoord.GetDistanceTo(eCoord);
            });
            nearFromRoutes = nearFromRoutes.OrderBy(x => x.Distance).ToList();
            nearToRoutes = nearToRoutes.OrderBy(x => x.Distance).ToList();

            // 找出彼此相同路線的站點
            var sameRoutes = nearFromRoutes.FindAll(x => nearToRoutes.Any(y => y.RouteUID == x.RouteUID))
                                           .ConvertAll(x => x.RouteUID);
            nearFromRoutes = nearFromRoutes.FindAll(x => sameRoutes.Contains(x.RouteUID));
            nearToRoutes = nearToRoutes.FindAll(x => sameRoutes.Contains(x.RouteUID));

            var searchResults = new List<RouteSearchResultModel>();

            var keyRoutes = routes.FindAll(x => sameRoutes.Contains(x.RouteUID));
            foreach (var r in keyRoutes)
            {
                if (searchResults.Any(x => x.RouteUID == r.RouteUID))
                {
                    continue; // 子路線可能會重複
                }

                var fromStops = nearFromRoutes.FindAll(x => x.RouteUID == r.RouteUID)
                                              .OrderBy(x => x.Distance).ToList();
                var toStops = nearToRoutes.FindAll(x => x.RouteUID == r.RouteUID)
                                          .OrderBy(x => x.Distance).ToList();
                int firstStopDistance = 0;
                StopModel fromStop = null;
                foreach (var s in fromStops)
                {
                    fromStop = r.Stops.Find(x => x.StopUID == s.StopUID);
                    if (fromStop != null)
                    {
                        firstStopDistance = s.Distance;
                        break;
                    }
                }

                int lastStopDistance = 0;
                StopModel toStop = null;
                foreach (var s in toStops)
                {
                    toStop = r.Stops.Find(x => x.StopUID == s.StopUID);
                    if (toStop != null)
                    {
                        lastStopDistance = s.Distance;
                        break;
                    }
                }

                // 出發站站序小於目的地站序才會是正確方向
                if (fromStop != null && toStop != null &&
                    fromStop.StopSequence < toStop.StopSequence)
                {
                    // 間隔站數
                    var gapStopCount = toStop.StopSequence - fromStop.StopSequence;
                    var keyStops = r.Stops.FindAll(x => x.StopSequence >= fromStop.StopSequence &&
                                                        x.StopSequence <= toStop.StopSequence);
                    var searchResult = new RouteSearchResultModel
                    {
                        RouteUID = r.RouteUID,
                        RouteName = r.RouteName,
                        FirstStopDistance = firstStopDistance,
                        LastStopDistance = lastStopDistance,
                        GapStopCount = gapStopCount,
                        Stops = keyStops
                    };
                    searchResults.Add(searchResult);
                }
            }

            // 結果排序
            searchResults = searchResults.OrderBy(x => x.FirstStopDistance + x.LastStopDistance)
                                         .ThenBy(x => x.GapStopCount)
                                         .ToList();

            result.Success = true;
            result.Data = searchResults;
            return CrossOrigin_Ok(result);
        }

        private List<RouteModel> LoadRouteCache()
        {
            var result = new List<RouteModel>();
            foreach (var city in CityConst.EnglishNames)
            {
                List<RouteModel> routes;
                var cachekey = CacheKeyConst.GetRouteKey(city);
                if (_cache.TryGetValue(cachekey, out routes))
                {
                    result.AddRange(routes);
                }
            }
            return result;
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
