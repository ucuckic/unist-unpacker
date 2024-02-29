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

            bool firstfol = false;

            string buildfiledir = "";
            string outputdir = "";
            string binfile = "";
            bool pack = false;
            bool uni2 = true;
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i];
                    //Console.WriteLine(args.Length+" "+arg);
                    switch (arg)
                    {
                        case "-printname":
                            Console.WriteLine( meth.print_fname(args[i + 1]) );
                            break;
                        case "-includein":
                            firstfol = true;
                            break;
                        case "-input":
                            inputfolders.Add(@args[i + 1]);
                            //inputdir = @args[i + 1];
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
                                try
                                {
                                    FileAttributes attr = File.GetAttributes(@args[b]);
                                

                                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                                    {
                                        Console.WriteLine("invalid input - folder");
                                        continue;
                                    }


                                    meth.dumpfiles(args[b],uni2);
                                }
                                catch
                                {
                                    Console.WriteLine("invalid file input");
                                }
                            }
                            break;
                    }
                }

                if(pack) meth.buildfiles(inputfolders, outputdir, binfile, firstfol,uni2);
            }
            else
            {
                //Console.WriteLine("Syntax: -input MS.Pal|Pal List.txt|Pal.png|, -basepal UNI.Pal, -color Pal to replace, -output output file");
                return;
            }
        }
    }
}
