using System.IO;
using System.IO.Compression;

namespace Legends_of_Callasia_Extractor
{
    class Program
    {
        static void Main(string[] args)
        {
            BinaryReader br = new(File.OpenRead(args[0]));

            if (new string(br.ReadChars(4)) != "CRAS")
                throw new System.Exception("This is not a Legends of Callasia arc file.");

            int version = br.ReadInt32();//1
            int fileCount = br.ReadInt32();
            int tableStart = br.ReadInt32();
            int tableSizeCompressed = br.ReadInt32();
            int tableSizeUncompressed = br.ReadInt32();
            int dataStart = br.ReadInt32();
            int dataSizeCompressed = br.ReadInt32();
            int dataSizeUncompressed = br.ReadInt32();

            br.BaseStream.Position = dataStart;
            MemoryStream fileData = new();
            BinaryWriter bw = new(fileData);
            bw.Write(br.ReadBytes(dataSizeCompressed));

            br.BaseStream.Position = tableStart;
            MemoryStream fileTable = new();
            br.ReadInt16();
            using (var ds = new DeflateStream(new MemoryStream(br.ReadBytes(tableSizeCompressed - 2)), CompressionMode.Decompress))
                ds.CopyTo(fileTable);

            br.Close();

            BinaryReader data = new(fileData);
            BinaryReader table = new(fileTable);
            table.BaseStream.Position = 0;

            string path = Path.GetDirectoryName(args[0]) + "\\" + Path.GetFileNameWithoutExtension(args[0]) + "\\";
            for (int i = 0; i < fileCount; i++)
            {
                string name = new string(table.ReadChars(256)).TrimEnd('\0');
                int start = table.ReadInt32() - dataStart;
                int sizeCompressed = table.ReadInt32();
                table.ReadInt32();
                int sizeUncompressed = table.ReadInt32();
                int isCompressed = table.ReadInt32();

                data.BaseStream.Position = start;
                Directory.CreateDirectory(path + Path.GetDirectoryName(name));
                if (isCompressed == 1)
                {
                    data.ReadInt16();
                    using (var ds = new DeflateStream(new MemoryStream(data.ReadBytes(sizeCompressed - 2)), CompressionMode.Decompress))
                        ds.CopyTo(File.Create(path + name));
                }
                else if (isCompressed == 2)
                {
                    bw = new(File.Create(path + name));
                    bw.Write(data.ReadBytes(sizeUncompressed));
                    bw.Close();
                }
            }
        }
    }
}
