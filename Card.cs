using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
    class Card
    {
        public int cid { get; set; }
        public string card_type { get; set; }
        public string name { get; set; }
        public string element { get; set; }
        public int damage { get; set; }
    }
}
