using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace MTCG
{
    public class Body
    {
        [JsonInclude]
        public int[] deck { get; set; }
        [JsonInclude]
        public int tradeoffer_id { get; set; }
        [JsonInclude]
        public int recipient_uid { get; set; }
        [JsonInclude]
        public int[] i_receive { get; set; }
        [JsonInclude]
        public int[] u_receive { get; set; }
        [JsonInclude]
        public string action { get; set; }
    }
}
