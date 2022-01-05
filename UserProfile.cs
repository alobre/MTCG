using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
    public class UserProfile
    {
        public int uid { get; set; } = 0;
        public int elo { get; set; } = 100;
        public int[] deck { get; set; } = new int[4];
        public int wins { get; set; } = 0;
        public int losses { get; set; } = 0;
        public int draw { get; set; } = 0;

    }
}
