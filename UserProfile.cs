using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
    public class UserProfile
    {
        public int elo { get; set; } = 100;
        public int[] deck { get; set; } = new int[4];
    }
}
