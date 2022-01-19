using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
    public class Tradeoffer
    {
        public int tradeoffer_id { get; set; }
        public int sender_uid { get; set; }
        public int recipient_uid { get; set; }
        public int[] i_receive { get; set; }
        public int[] u_receive { get; set; }
        public string status { get; set; }
    }
}
