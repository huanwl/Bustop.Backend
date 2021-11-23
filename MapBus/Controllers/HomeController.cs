using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MapBus.Models;
using Npgsql;
using Dapper;

namespace MapBus.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            var connStr = "Server=ec2-18-208-97-23.compute-1.amazonaws.com;Port=5432;Database=dckhnn9qf2gb8n;User Id=jkttkauwxxqexe;Password=294e75fe05304438ff43d677fd7a68953f0b2f26e28524d71acd28b8b5f929dc;";
            using (var conn = new NpgsqlConnection(connStr))
            {
                var querySQL = "select account from public.\"User\";";
                var result = conn.Query<string>(querySQL).ToList();
                ViewBag.Account = result[0];
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
