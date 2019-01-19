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
            List<string> inputfolders = new List<string>();
            string inputdir = "";

            string buildfiledir = "";
            string outputdir = "";
            string binfile = "";
            bool pack = false;
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i];
                    //Console.WriteLine(args.Length+" "+arg);
                    switch (arg)
                    {
                        case "-input":
                            //inputfolders.Add(@args[i + 1]);
                            inputdir = @args[i + 1];
                            i++;
                            break;
                        case "-output":
                            outputdir = @args[i + 1];
                            binfile = @args[i + 2];
                            i+=2;
                            break;
                        case "-pack":
                            Console.WriteLine("it be like it do");
                            pack = true;
                            break;
                        default:
                            for (int b = 0; b < args.Length; b++)
                            {
                                meth.dumpfiles(File.ReadAllBytes(args[b]));
                            }
                            break;
                    }
                }

                if(pack) meth.buildfiles(inputdir, outputdir, binfile);
            }
            else
            {
                //Console.WriteLine("Syntax: -input MS.Pal|Pal List.txt|Pal.png|, -basepal UNI.Pal, -color Pal to replace, -output output file");
                return;
            }
        }
    }
}
