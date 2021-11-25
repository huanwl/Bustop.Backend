using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapBus.Controllers.Api.v1
{
    public class BaseApiController : ControllerBase
    {
        public OkObjectResult CrossOrigin_Ok(object val)
        {
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST");
            Response.Headers.Add("Access-Control-Allow-Headers", "X-Requested-With, Content-Type, Accept");
            return Ok(val);
        }
    }
}
