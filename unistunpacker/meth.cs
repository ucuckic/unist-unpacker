using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace unistunpacker
{
    class filemetadata
    {
        public string path = "";

        public bool isfolder = false;

        public string infolder = "";

        public int size1 = 0; //if the entry is a folder this is the number of files
        public int size2 = 0;
        public int pos = 0; //if the entry is a folder this is the offset of the first file in that folder

        public int findex = 0;
    }

    class meth
    {
        public static void dumpfiles(byte[] listfile)
        {
            string workingdir = Directory.GetCurrentDirectory();

            List<filemetadata> flist = new List<filemetadata>();
            List<filemetadata> folderlist = new List<filemetadata>();

            int numfolders = 0;
            int numfiles = 0;
            string archivename = "";

            using (MemoryStream filestream = new MemoryStream(listfile))
            {
                using (BinaryReader streamread = new BinaryReader(filestream))
                {
                    numfolders = streamread.ReadInt32();
                    numfiles = streamread.ReadInt32();
                    int unk = streamread.ReadInt32();

                    Console.WriteLine("posis " + streamread.BaseStream.Position);

                    byte[] strprocess = streamread.ReadBytes(52);


                    archivename = Encoding.ASCII.GetString(strprocess).Replace("\0","");

                    Console.WriteLine("arcname " + archivename);

                    Console.WriteLine("posis " + streamread.BaseStream.Position);


                    //archivename = streamread.ReadString(24);


                    for (int i = 0; i < numfolders; i++)
                    {
                        //Console.WriteLine("fposst " + streamread.BaseStream.Position);
                        Console.WriteLine("numfolders "+numfolders);
                        filemetadata metadata = new filemetadata();

                        metadata.isfolder = true;

                        metadata.size1 = streamread.ReadInt32();
                        metadata.size2 = streamread.ReadInt32();
                        metadata.pos = streamread.ReadInt32();

                        Console.WriteLine("fpos " + streamread.BaseStream.Position +" and "+metadata.size1);

                        string path = Encoding.ASCII.GetString(streamread.ReadBytes(116)).Replace("\0", "");
                        //metadata.findex = streamread.ReadInt32();
                        if(i == numfolders - 1) metadata.findex = streamread.ReadInt32();

                        //Console.WriteLine("fsize1 " + metadata.size1);

                        //Console.WriteLine("fsize2 " + metadata.size2);

                        //Console.WriteLine("fpos " + metadata.pos);

                        Console.WriteLine("fpath "+path);

                        Console.WriteLine("lastfpos " + streamread.BaseStream.Position);

                        metadata.path = path;

                        folderlist.Add(metadata);
                    }

                    for (int i = 0, b = 0, c = 0; i < numfiles; i++, c++)
                    {
                        filemetadata metadata = new filemetadata();
                        metadata.size1 = streamread.ReadInt32();
                        metadata.size2 = streamread.ReadInt32();
                        metadata.pos = streamread.ReadInt32();

                        Console.WriteLine("bvar "+b);

                        Console.WriteLine("mypos " + streamread.BaseStream.Position);

                        string path = Encoding.ASCII.GetString(streamread.ReadBytes(52)).Replace("\0", "");

                        Console.WriteLine(path);

                        metadata.path = path;

                        if (c >= folderlist[b].size1 )
                        {
                            Console.WriteLine(folderlist[b].size1+" numfl "+i);
                            b++;
                            c = 0;
                        }

                        metadata.infolder = folderlist[b].path;

                        flist.Add(metadata);
                    }
                }
            }

            string[] br = new string[2];

            br[0] = archivename;

            //Console.WriteLine(Path.Combine(workingdir, "d"));
            //File.WriteAllLines(Path.Combine(workingdir, "debugout@"+"test.txt"), br);
            Console.WriteLine(Path.Combine(workingdir, archivename));

            byte[] archive = File.ReadAllBytes(Path.Combine(workingdir,archivename));

            using (MemoryStream filestream = new MemoryStream(archive))
            {
                using (BinaryReader readfile = new BinaryReader(filestream))
                {
                    foreach(filemetadata folder in folderlist)
                    {
                        //string outpath = Path.Combine(workingdir, @"output", @folder.path);

                        //new FileInfo(outpath).Directory.Create();

                        //Console.WriteLine(outpath);
                    }

                    foreach(filemetadata file in flist)
                    {
                        if (file.isfolder == false)
                        {
                            readfile.BaseStream.Position = file.pos;

                            byte[] fbytes = readfile.ReadBytes(file.size1);
                            string outpath = Path.Combine(workingdir, @"output", @file.infolder, @file.path);

                            new FileInfo(outpath).Directory.Create();

                            File.WriteAllBytes(outpath,fbytes);

                            Console.WriteLine(outpath);
                        }
                        
                    }
                }
            }

            return;
        }
    }
}
