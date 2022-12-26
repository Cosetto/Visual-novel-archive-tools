using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Escu
{
    class Program
    {
        static void Main(string[] args)
        {
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ");
				Console.WriteLine("         Unpack: Bin.exe -u <input file> <output folder>");
				Console.WriteLine("         Pack:   Bin.exe -p <input folder> <output file>");
                return;
            }

            string operation = args[0];
            string inputFile = args[1];
            string outputFile = args[2];

            if (operation == "-u")
            {
                Unpack(inputFile, outputFile);
            }
            else if (operation == "-p")
            {
                Pack(inputFile, outputFile);
            }
            else
            {
                Console.WriteLine("Invalid operation. Valid operations are 'unpack' and 'pack'.");
            }
        }

		static void Unpack(string inputFile, string outputFile)
		{
			ArcBIN ArcBIN = new ArcBIN();
			try
			{
				Directory.CreateDirectory(outputFile);
	
				List<Entry> entries = ArcBIN.TryOpen(inputFile);
				foreach (Entry entry in entries)
				{
					byte[] bytes = ArcBIN.Unpack(entry);
					System.IO.File.WriteAllBytes(Path.Combine(outputFile, entry.Name), bytes);
				}

				using (StreamWriter streamWriter = new StreamWriter(new FileStream(Path.Combine(outputFile, "FileList.txt"), FileMode.Create), Encoding.UTF8))
				{
					for (int i = 0; i < entries.Count; i++)
					{
						streamWriter.WriteLine(entries[i].Name);
						streamWriter.Flush();
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return;
			}
			Console.WriteLine("Unpacked successfully!");
		}

        static void Pack(string inputFile, string outputFile)
        {
            ArcBIN ArcBIN = new ArcBIN();
            try
            {
                ArcBIN.Pack(inputFile, outputFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            Console.WriteLine("Pack successfully!");
        }
    }
}
