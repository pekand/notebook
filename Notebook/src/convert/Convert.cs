using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notebook
{
    class Convert
    {
        public static long StringToLong(string value, long defaultValue = 0) {
            long result = 0;

            if (!long.TryParse(value, out result))
            {
                return result;
            }

            return defaultValue;
        }
        public static int StringToInt(string value, int defaultValue = 0)
        {
            int result = 0;

            if (Int32.TryParse(value, out result))
            {
                return result;
            }

            return defaultValue;
        }

    }
}
