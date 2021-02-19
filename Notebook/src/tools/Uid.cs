using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notebook
{
    class Uid
    {
        public static string get() {
            Guid guid = Guid.NewGuid();
            return guid.ToString();
        }
    }
}
