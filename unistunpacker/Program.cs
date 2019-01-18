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
            string buildfiledir = "";
            string outputdir = "";
            string binfile = "";
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i];
                    //Console.WriteLine(args.Length+" "+arg);
                    switch (arg)
                    {
                        case "-pack":
                            Console.WriteLine("it be like it do");

                            buildfiledir = @args[i + 1];
                            outputdir = @args[i + 2];
                            binfile = @args[i + 3];

                            meth.buildfiles(buildfiledir,outputdir,binfile);

                            i += 3;
                            break;
                        case "-input":
                            //buildfiledir = @args[i + 1];
                            break;
                        case "-output":
                            //outputdir = @args[i + 1];
                            break;
                        default:
                            for (int b = 0; b < args.Length; b++)
                            {
                                meth.dumpfiles(File.ReadAllBytes(args[b]));
                            }
                            break;
                    }
                }
            }
            else
            {
                //Console.WriteLine("Syntax: -input MS.Pal|Pal List.txt|Pal.png|, -basepal UNI.Pal, -color Pal to replace, -output output file");
                return;
            }
        }
    }
}
