using Dapper;
using MapBus.Models;
using MapBus.Models.Parameters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapBus.Controllers.Api.v1
{
    public class UserApiController : BaseApiController
    {
        private readonly IConfiguration _configuration;

        public UserApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("/api/user/login")]
        public IActionResult Login([FromBody]LoginParameter parameter)
        {
            var result = new ResultModel();

            Guid userId;
            if (!Guid.TryParse(parameter.Token, out userId))
            {
                userId = Guid.NewGuid();
            }

            using (var conn = new NpgsqlConnection(_configuration["ConnectionString"]))
            {
                var sql = "insert into public.end_user (id, date_in) values(@id, @datein)";
                var row = conn.Execute(sql, new { id = userId, datein = DateTime.UtcNow });
                if (row == 1)
                {
                    result.Success = true;
                    result.Data = new
                    {
                        Token = userId.ToString().Replace("-", "")
                    };
                }
            }     

            return CrossOrigin_Ok(result);
        }
    }
}
