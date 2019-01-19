using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace unistunpacker
{
    class filearc
    {
        public string binaryfiledir = "";

        public List<filemetadata> filelist = new List<filemetadata>();
        public List<filemetadata> folderlist = new List<filemetadata>();

        public int numfolders = 0;
        public int numfiles = 0;
        public int unk = 0;
        public string archivename = "";


        public byte[] ToArray()
        {
            List<byte[]> arraylist = new List<byte[]>();

            byte[] header = new byte[64];

            byte[] dirstrbyte = Encoding.UTF8.GetBytes(binaryfiledir);

            using (MemoryStream filestream = new MemoryStream(header))
            {
                using (BinaryWriter write = new BinaryWriter(filestream))
                {
                    write.Write(numfolders);
                    write.Write(numfiles);
                    write.Write(unk);

                    write.Write(dirstrbyte);

                    arraylist.Add(header);
                }

                filestream.Dispose();
            }

            foreach (filemetadata folder in folderlist)
            {
                byte[] entry = new byte[128];
                byte[] folderpath = Encoding.UTF8.GetBytes(folder.path);

                using (MemoryStream filestream = new MemoryStream(entry))
                {
                    using (BinaryWriter write = new BinaryWriter(filestream))
                    {
                        write.Write(folder.size1);
                        write.Write(folder.size2);
                        write.Write(folder.pos);
                        write.Write(folderpath);

                        arraylist.Add(entry);
                    }

                    filestream.Dispose();
                }
            }

            foreach (filemetadata file in filelist)
            {
                byte[] entry = new byte[64];
                byte[] filepath = Encoding.UTF8.GetBytes(file.path);

                using (MemoryStream filestream = new MemoryStream(entry))
                {
                    using (BinaryWriter write = new BinaryWriter(filestream))
                    {
                        write.Write(file.something);

                        write.Write(file.size1);
                        write.Write(file.size2);
                        write.Write(file.pos);
                        write.Write(filepath);

                        Console.WriteLine(file.path);

                        arraylist.Add(entry);
                    }
                }
            }

            byte[] construct = arraylist.SelectMany(a => a).ToArray();

            return construct;
        }

        public byte[] DataArray()
        {
            byte[] construct = new byte[0];

            return construct;
        }
    }

    class filemetadata
    {
        public string path = "";

        public bool isfolder = false;

        public string infolder = "";

        public int size1 = 0; //if the entry is a folder this is the number of files
        public int size2 = 0; //if the entry is a folder this is the total number of files incrementally
        public int pos = 0; //if the entry is a folder this is the combined filesize of all contained files

        public int something = 0;

        public int findex = 0;

        public byte[] fdata = new byte[0];
    }

    class meth
    {
        public static void createfile(string outputdir, byte[] file, string binfile)
        {
            new FileInfo(outputdir).Directory.Create();

            System.IO.File.WriteAllBytes(outputdir, file);

            Console.WriteLine("saved to " + outputdir);
        }

        public static void buildfiles(string @inputdir, string @outputdir, string binfile)
        {
            string workingdir = Directory.GetCurrentDirectory();

            string pathname = new DirectoryInfo(inputdir).Name;

            int filepos = 0;

            //workingdir = Path.Combine(workingdir, inputdir);

            filearc newfile = new filearc();

            newfile.binaryfiledir = binfile;
            
            filemetadata rootfolder = new filemetadata();
            rootfolder.size1 = Directory.GetFiles(inputdir).Length;
            rootfolder.path = inputdir;
            
            //newfile.folderlist.Add(rootfolder);

            Console.WriteLine(inputdir + " in ");

            Directory.CreateDirectory(inputdir);

            string[] allfiles = Directory.GetFiles(inputdir, "*.*", SearchOption.AllDirectories);
            string[] allfolders = Directory.GetDirectories(inputdir, "*.*", SearchOption.AllDirectories);

            for (int z = 0; z < allfolders.Length; z++)
            {
                //newfile.numfolders++;
            }

           //newfile.numfolders++;

            newfile.numfiles = allfiles.Length;

            for (int i = 0, j = 0; i < allfolders.Length; i++)
            {
                string folpathstr = allfolders[i].Substring(inputdir.IndexOf(pathname)+pathname.Length+1,allfolders[i].Length-inputdir.IndexOf(pathname)-(pathname.Length+1));
                if(i==0)
                {
                    //j += rootfolder.size1;
                }

                Console.WriteLine(" fo " + allfolders[i]);

                filemetadata folder = new filemetadata();

                folder.path = folpathstr;

                folder.size1 = Directory.GetFiles(allfolders[i]).Length;

                Console.WriteLine(allfolders[i]+" len "+folder.size1);

                folder.size2 = j;

                Console.WriteLine(" fo " + allfolders[i]);

                if(folder.size1 > 0 ) newfile.folderlist.Add(folder);

                j += folder.size1;

                Console.WriteLine("folder size " + folder.size1 + " folder size2 " + folder.size2 + " folder path " + folder.path + " what " + newfile.folderlist.Count);
            }

            int fpos = 0;

            foreach(filemetadata folder in newfile.folderlist)
            {
                folder.pos = fpos;

                Console.WriteLine("1");

                Console.WriteLine("should be  "+ inputdir+" potentially also "+ folder.path+" literaly me "+ Path.Combine(inputdir, folder.path));

                string[] folfiles = Directory.GetFiles(Path.Combine(inputdir,folder.path));

                Console.WriteLine("2 len"+folfiles.Length);

                for (int i = 0; i < folfiles.Length; i++)
                {
                    //Console.WriteLine(i + " dir " + allfiles[i]);

                    filemetadata file = new filemetadata();

                    file.fdata = File.ReadAllBytes(folfiles[i]);

                    file.size1 = file.fdata.Length;
                    file.size2 = file.fdata.Length;

                    file.path = Path.GetFileName(folfiles[i]);

                    file.infolder = Path.GetDirectoryName(folfiles[i]);

                    file.pos = filepos;

                    //Console.WriteLine("file path " + file.path + " file folder " + file.infolder);

                    newfile.filelist.Add(file);

                    filepos += file.size1;

                    fpos += file.size1;
                }
            }

            newfile.numfolders = newfile.folderlist.Count;


            //Console.WriteLine("past for " + "files " + allfiles.Length + " folders " + allfolders.Length + " rootfolder " + rootfolder.path);


            new FileInfo(outputdir).Directory.Create();
            System.IO.File.WriteAllBytes(outputdir, newfile.ToArray());
            Console.WriteLine("saved to " + outputdir);
            using (FileStream outstream = new FileStream(newfile.binaryfiledir, FileMode.OpenOrCreate))
            {
                using (BinaryWriter outfile = new BinaryWriter(outstream))
                {
                    foreach (filemetadata file in newfile.filelist)
                    {
                        outfile.Write(file.fdata);
                        outfile.Flush();
                    }
                }
            }

        }
        public static void dumpfiles(byte[] listfile)
        {
            string workingdir = Directory.GetCurrentDirectory();

            filearc newfile = new filearc();

            using (MemoryStream filestream = new MemoryStream(listfile))
            {
                using (BinaryReader streamread = new BinaryReader(filestream))
                {
                    newfile.numfolders = streamread.ReadInt32();
                    newfile.numfiles = streamread.ReadInt32();
                    int unk = streamread.ReadInt32();

                    Console.WriteLine("posis " + streamread.BaseStream.Position);

                    byte[] strprocess = streamread.ReadBytes(52);


                    newfile.archivename = Encoding.UTF8.GetString(strprocess).TrimEnd('\0');

                    Console.WriteLine("arcname " + newfile.archivename);

                    Console.WriteLine("posis " + streamread.BaseStream.Position);


                    //archivename = streamread.ReadString(24);


                    for (int i = 0; i < newfile.numfolders; i++)
                    {
                        //Console.WriteLine("fposst " + streamread.BaseStream.Position);
                        Console.WriteLine("numfolders "+ newfile.numfolders);
                        filemetadata metadata = new filemetadata();

                        metadata.isfolder = true;

                        metadata.size1 = streamread.ReadInt32();
                        metadata.size2 = streamread.ReadInt32();
                        metadata.pos = streamread.ReadInt32();

                        Console.WriteLine("fpos " + streamread.BaseStream.Position +" and "+metadata.size1);

                        string path = Encoding.UTF8.GetString(streamread.ReadBytes(116)).TrimEnd('\0');
                        //metadata.findex = streamread.ReadInt32();
                        if (i == newfile.numfolders - 1) metadata.findex = streamread.ReadInt32();

                        //Console.WriteLine("fsize1 " + metadata.size1);

                        //Console.WriteLine("fsize2 " + metadata.size2);

                        //Console.WriteLine("fpos " + metadata.pos);

                        Console.WriteLine("fpath "+path);

                        Console.WriteLine("lastfpos " + streamread.BaseStream.Position);

                        metadata.path = path;

                        newfile.folderlist.Add(metadata);
                    }
                    foreach(filemetadata folder in newfile.folderlist)
                    {
                        for (int i = 0; i < folder.size1; i++)
                        {
                            filemetadata metadata = new filemetadata();
                            metadata.size1 = streamread.ReadInt32();
                            metadata.size2 = streamread.ReadInt32();
                            metadata.pos = streamread.ReadInt32();

                            Console.WriteLine("mypos " + streamread.BaseStream.Position);

                            string path = Encoding.UTF8.GetString(streamread.ReadBytes(52)).TrimEnd('\0');

                            Console.WriteLine(path);

                            metadata.path = path;

                            if (newfile.folderlist.Count > 0)
                            {
                                metadata.infolder = folder.path;
                            }

                            newfile.filelist.Add(metadata);

                            Console.WriteLine("forfin");
                        }
                    }
                }
            }

            //Console.WriteLine(Path.Combine(workingdir, "d"));
           // File.WriteAllLines(Path.Combine(workingdir, "debugout@"+"test.txt"), br);
            Console.WriteLine(Path.Combine(workingdir, newfile.archivename));

            byte[] archive = File.ReadAllBytes(Path.Combine(workingdir, newfile.archivename));

            using (MemoryStream filestream = new MemoryStream(archive))
            {
                using (BinaryReader readfile = new BinaryReader(filestream))
                {
                    foreach(filemetadata folder in newfile.folderlist)
                    {
                        string outpath = Path.Combine(workingdir, @"output", @folder.path);

                        new FileInfo(@outpath).Directory.Create();

                        Console.WriteLine(outpath+" folnum "+newfile.folderlist.Count+" foldpath "+folder.path);
                    }

                    foreach(filemetadata file in newfile.filelist)
                    {
                        if (file.isfolder == false)
                        {
                            readfile.BaseStream.Position = file.pos;

                            byte[] fbytes = readfile.ReadBytes(file.size1);
                            string outpath = Path.Combine(workingdir, @"output", @file.infolder, @file.path);

                            new FileInfo(outpath).Directory.Create();

                            File.WriteAllBytes(outpath,fbytes);

                            Console.WriteLine("infolder "+file.infolder+" filepath "+file.path+" outpath "+outpath);
                        }
                    }
                }
            }

            return;
        }
    }
}
