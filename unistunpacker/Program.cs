using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace unistunpacker
{
    class Program
    {
        static void Main(string[] args)
        {
            for(int i = 0; i < args.Length; i++)
            {
                meth.dumpfiles(args[i]);
            }
        }
    }
}
