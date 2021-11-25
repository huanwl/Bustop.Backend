using Dapper;
using MapBus.Models;
using MapBus.Models.Parameters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapBus.Controllers.Api.v1
{
    public class FavoritePlaceApiController : BaseApiController
    {
        private readonly IConfiguration _configuration;

        public FavoritePlaceApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("/api/favorite_places/{token}")]
        public IActionResult Get(string token)
        {
            var result = new ResultModel();

            Guid userId;
            if (Guid.TryParse(token, out userId))
            {
                using (var conn = new NpgsqlConnection(_configuration["ConnectionString"]))
                {
                    var sql = new StringBuilder();
                    sql.AppendLine("select * from public.favorite_place ");
                    sql.AppendLine("where user_id = @UserId ");
                    sql.AppendLine("order by id desc; ");

                    var models = conn.Query<FavoritePlaceModel>(sql.ToString(), new 
                    { 
                        UserId = userId 
                    }).ToList();

                    result.Data = models;
                    result.Success = true;
                }
            }
            else
            {
                result.Success = false;
                result.Message = "Not login";
            }

            return CrossOrigin_Ok(result);
        }

        [HttpPost]
        [Route("/api/favorite_places")]
        public IActionResult Create([FromBody]FavoritePlacesCreateParameter parameter)
        {
            var result = new ResultModel();

            Guid userId;
            if (Guid.TryParse(parameter.Token, out userId))
            {
                using (var conn = new NpgsqlConnection(_configuration["ConnectionString"]))
                {
                    var sql = new StringBuilder();
                    sql.AppendLine("INSERT INTO public.favorite_place ");
                    sql.AppendLine("(\"name\", address, lat, lng, user_id, date_in) ");
                    sql.AppendLine("VALUES(@name, @address, @lat, @lng, @userid, @datein);");

                    var row = conn.Execute(sql.ToString(), new 
                    { 
                        name = parameter.Name,
                        address = parameter.Address,
                        lat = parameter.Lat,
                        lng = parameter.Lng,
                        userid = userId,
                        datein = DateTime.UtcNow
                    });

                    result.Success = row == 1;
                }
            }
            else
            {
                result.Success = false;
                result.Message = "Not login";
            }

            return CrossOrigin_Ok(result);
        }
    }
}
