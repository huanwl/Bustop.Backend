using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapBus.Models
{
    public class ResultModel
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public object Data { get; set; }
    }
}
