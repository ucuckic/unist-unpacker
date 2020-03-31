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
        private int incrementalpos = 0;

        public int getallsize()
        {
            Console.WriteLine("INCREMENTAL " + incrementalpos);
            return incrementalpos;
        }

        public byte[] ToArray()
        {
            List<byte[]> arraylist = new List<byte[]>();

            byte[] header = new byte[64];

            byte[] dirstrbyte = Encoding.UTF8.GetBytes(new DirectoryInfo(binaryfiledir).Name);

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

            uint fpos = 0;
            for (int i = 0; i < filelist.Count; i++)
            {

                filemetadata file = filelist[i];

                byte[] entry = new byte[64];
                byte[] filepath = Encoding.UTF8.GetBytes(file.path);

                if(i > 0)
                {
                    file.pos = fpos;
                }

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

                fpos += file.size1;
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

        public uint size1 = 0; //if the entry is a folder this is the number of files
        public uint size2 = 0; //if the entry is a folder this is the total number of files incrementally
        public uint pos = 0; //if the entry is a folder this is the combined filesize of all contained files

        public uint something = 0;

        public uint findex = 0;

        public byte[] fdata = new byte[0];

        public string associated_indir = "";

        public List<string> folfiles = new List<string>();
    }

    class meth
    {
        public static void createfile(string outputdir, byte[] file, string binfile)
        {
            new FileInfo(outputdir).Directory.Create();

            System.IO.File.WriteAllBytes(outputdir, file);

            Console.WriteLine("saved to " + outputdir);
        }

        public static void buildfiles(List<String> @inputdirlist, string @outputdir, string binfile, bool firstfol = false)
        {
            string workingdir = Directory.GetCurrentDirectory();

            filearc newfile = new filearc();

            newfile.binaryfiledir = binfile;

            uint filepos = 0;
            uint fpos = 0;
            uint j = 0; //filecnt related
            //for (int indirs = 0; indirs < inputdirlist.Count; indirs++)
            foreach (string inputdir in inputdirlist)
            {
                string pathname = new DirectoryInfo(inputdir).Name;

                Console.WriteLine(inputdir + " in ");

                Directory.CreateDirectory(inputdir);

                string[] allfiles = Directory.GetFiles(inputdir, "*.*", SearchOption.AllDirectories);
                string[] allfolders = Directory.GetDirectories(inputdir, "*.*", SearchOption.AllDirectories);

                newfile.numfiles += allfiles.Length;

                for (int i = 0; i < allfolders.Length; i++)
                {
                    string folpathstr = (firstfol) ? allfolders[i].Substring(inputdir.IndexOf(pathname), allfolders[i].Length - inputdir.IndexOf(pathname)) : allfolders[i].Substring(inputdir.IndexOf(pathname) + pathname.Length + 1, allfolders[i].Length - inputdir.IndexOf(pathname) - (pathname.Length + 1));

                    Console.WriteLine(" fo " + allfolders[i]+" fopathstr "+folpathstr);

                    filemetadata folder = new filemetadata();

                    folder.path = folpathstr;

                    folder.size1 = Convert.ToUInt32(Directory.GetFiles(allfolders[i]).Length);

                    Console.WriteLine(allfolders[i] + " len " + folder.size1);

                    folder.size2 = j;

                    Console.WriteLine(" fo " + allfolders[i]);
                    
                    folder.associated_indir = inputdir;

                    string usepath = (firstfol)? Path.Combine(Directory.GetParent(folder.associated_indir).FullName, folder.path) : Path.Combine(folder.associated_indir, folder.path);

                    Console.WriteLine("asspath " + usepath + " and path " + folder.path+" usepath "+usepath);

                    //System.Environment.Exit(0);

                    string[] folfiles = Directory.GetFiles(usepath);

                    if (folder.size1 > 0)
                    {
                        int existindex = newfile.folderlist.FindIndex(item => item.path == folder.path);

                        Console.WriteLine("existi "+ existindex);

                        //this shit doesnt even work
                        if (existindex > -1)
                        {
                            //merge folder contents

                            newfile.folderlist[existindex].size1 += folder.size1;

                            newfile.folderlist[existindex].folfiles.AddRange(folfiles.ToList());
                        }
                        else
                        {
                            folder.folfiles.AddRange(folfiles.ToList());

                            newfile.folderlist.Add(folder);
                        }
                    }


                    j += folder.size1;

                    Console.WriteLine("folder size " + folder.size1 + " folder size2 " + folder.size2 + " folder path " + folder.path + " what " + newfile.folderlist.Count);
                }

                newfile.numfolders = newfile.folderlist.Count;

            }


            foreach (filemetadata folder in newfile.folderlist)
            {
                string[] folfiles = folder.folfiles.ToArray();

                folder.pos = fpos;

                Console.WriteLine("pos "+folder.pos);

                Console.WriteLine("should be \\" + folder.associated_indir + "\\ potentially also \\" + folder.path + "\\ literaly me \\" + Path.Combine(folder.associated_indir, folder.path));

                Console.WriteLine("2 len" + folfiles.Length);

                string pathname = new DirectoryInfo(folder.associated_indir).Name;

                for (int i = 0; i < folfiles.Length; i++)
                {
                    //Console.WriteLine(i + " dir " + allfiles[i]);

                    filemetadata file = new filemetadata();

                    file.fdata = File.ReadAllBytes(folfiles[i]);

                    file.size1 = Convert.ToUInt32(file.fdata.Length);
                    file.size2 = Convert.ToUInt32(file.fdata.Length);

                    file.path = Path.GetFileName(folfiles[i]);

                    file.infolder = (firstfol) ? folfiles[i].Substring(folder.associated_indir.IndexOf(pathname) + pathname.Length + 1, folfiles[i].Length - folder.associated_indir.IndexOf(pathname) - (pathname.Length + 1)) : folfiles[i].Substring(folder.associated_indir.IndexOf(pathname) + pathname.Length + 1, folfiles[i].Length - folder.associated_indir.IndexOf(pathname) - (pathname.Length + 1));

                    Console.WriteLine(" her u o po "+newfile.filelist.Count);

                    Console.WriteLine("file path " + file.path + " file folder " + file.infolder+" fofiles len "+folfiles.Length);

                    int existindex = newfile.filelist.FindIndex(item => item.path == file.path && item.infolder == file.infolder);

                    Console.WriteLine("existi file " + existindex+" path "+file.path+" inf "+ file.infolder);
                    Console.WriteLine("fpathmaster "+ folfiles[i]+" len "+file.fdata.Length);

                    //this shit doesnt even work
                    if (existindex > -1)
                    {
                        //merge folder contents

                        //newfile.folderlist[existindex].size1 += folder.size1;

                        //newfile.folderlist[existindex].folfiles.AddRange(folfiles.ToList());
                        newfile.filelist[existindex] = file;
                        folder.size1 -= 1;
                    }
                    else
                    {
                        newfile.filelist.Add(file);
                    }

                }
            }
            //Console.WriteLine("past for " + "files " + allfiles.Length + " folders " + allfolders.Length + " rootfolder " + rootfolder.path);


            new FileInfo(outputdir).Directory.Create();
            System.IO.File.WriteAllBytes(outputdir, newfile.ToArray());
            Console.WriteLine("saved to " + outputdir);
            using (FileStream outstream = new FileStream(newfile.binaryfiledir, FileMode.Create))
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
        public static void dumpfiles(string listfile)
        {
            string workingdir = Directory.GetCurrentDirectory();

            filearc newfile = new filearc();

            using (FileStream filestream = File.OpenRead(listfile))
            {
                using (BinaryReader streamread = new BinaryReader(filestream))
                {
                    newfile.numfolders = streamread.ReadInt32();
                    newfile.numfiles = streamread.ReadInt32();
                    int unk = streamread.ReadInt32();
                    Console.WriteLine("posis " + streamread.BaseStream.Position);

                    byte[] strprocess = streamread.ReadBytes(52);
                    newfile.archivename = Encoding.UTF8.GetString(strprocess).TrimEnd('\0');

                    try
                    {
                        if (!File.Exists(Path.Combine(workingdir, newfile.archivename)))
                        {
                            Console.WriteLine("binary file " + newfile.archivename + " not found");
                            return;
                        }
                    }
                    catch(ArgumentException)
                    {
                        Console.WriteLine("binary file path invalid");
                        return;
                    }

                    Console.WriteLine("arcname " + newfile.archivename);

                    Console.WriteLine("posis " + streamread.BaseStream.Position);


                    //archivename = streamread.ReadString(24);


                    for (int i = 0; i < newfile.numfolders; i++)
                    {
                        //Console.WriteLine("fposst " + streamread.BaseStream.Position);
                        Console.WriteLine("numfolders "+ newfile.numfolders);
                        filemetadata metadata = new filemetadata();

                        metadata.isfolder = true;

                        metadata.size1 = streamread.ReadUInt32();
                        metadata.size2 = streamread.ReadUInt32();
                        metadata.pos = streamread.ReadUInt32();

                        Console.WriteLine("fpos " + streamread.BaseStream.Position +" and "+metadata.size1);

                        string path = Encoding.UTF8.GetString(streamread.ReadBytes(116)).TrimEnd('\0');
                        //metadata.findex = streamread.ReadInt32();
                        if (i == newfile.numfolders - 1) metadata.findex = streamread.ReadUInt32();

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
                            metadata.size1 = streamread.ReadUInt32();
                            metadata.size2 = streamread.ReadUInt32();
                            metadata.pos = streamread.ReadUInt32();

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

            //byte[] archive = File.ReadAllBytes(Path.Combine(workingdir, newfile.archivename));

            using (FileStream filestream = File.OpenRead(Path.Combine(workingdir, newfile.archivename)))
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

                            byte[] fbytes = readfile.ReadBytes( Convert.ToInt32(file.size1) );
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
