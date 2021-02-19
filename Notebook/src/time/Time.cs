using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notebook
{
    class Time
    {
        public static long getUnixTime() {
            return DateTimeOffset.Now.ToUnixTimeSeconds();
        }
    }
}
