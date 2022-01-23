using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MTCG
{
    public class Credentials
    {
        [JsonInclude]
        public string username { get; set; }
        [JsonInclude]
        public string password { get; set; }
        [JsonInclude]
        public string access_token { get; set; }
    }

}
