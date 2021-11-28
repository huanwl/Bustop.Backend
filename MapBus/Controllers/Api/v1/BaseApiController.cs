using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MapBus.Controllers.Api.v1
{
    public class BaseApiController : ControllerBase
    {
        protected string _xdate;
        protected string _authorization;

        public BaseApiController()
        {
            var culture = CultureInfo.CreateSpecificCulture("en-US");
            _xdate = DateTime.UtcNow.ToString("ddd, dd MMM yyy HH':'mm':'ss 'GMT'", culture);
            
            var signature = HMACSHA1Text("x-date: " + _xdate, "PlXdZfp5n649oSRJlZPcqeltGrU");
            _authorization = $"hmac username=\"e8d701ab4e6342d7a4d626ced03c5549\", algorithm=\"hmac-sha1\", headers=\"x-date\", signature=\"{signature}\"";
        }

        public OkObjectResult CrossOrigin_Ok(object val)
        {
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            Response.Headers.Add("Access-Control-Allow-Headers", "X-Requested-With, Content-Type, Accept");
            return Ok(val);
        }

        public string HMACSHA1Text(string EncryptText, string EncryptKey)
        {
            HMACSHA1 hmacsha1 = new HMACSHA1();
            hmacsha1.Key = Encoding.ASCII.GetBytes(EncryptKey);
            byte[] dataBuffer = Encoding.ASCII.GetBytes(EncryptText);
            byte[] hashBytes = hmacsha1.ComputeHash(dataBuffer);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
