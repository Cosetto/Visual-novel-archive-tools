using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Silky
{
    class Program
    {
        static void Main(string[] args)
        {
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ");
				Console.WriteLine("         Unpack: Arc.exe -u <input file> <output folder>");
				Console.WriteLine("         Pack:   Arc.exe -p <input folder> <output file>");
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
            ArcARC ArcARC = new ArcARC();
            try
            {
                Directory.CreateDirectory(outputFile);
				
				List<Entry> entries = ArcARC.TryOpen(inputFile);
				if (entries == null || entries.Count == 0)
				{
					entries = ArcARC.TryOpenOLD(inputFile);
				}

				if (entries == null || entries.Count == 0)
				{
					throw new Exception("Unsupported file format or file is empty.");
				}

                foreach (Entry entry in ArcARC.TryOpen(inputFile))
                {
                    byte[] bytes = ArcARC.Unpack(entry);
                    System.IO.File.WriteAllBytes(Path.Combine(outputFile, entry.Name), bytes);
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
            ArcARC ArcARC = new ArcARC();
            try
            {
                ArcARC.Pack(inputFile, outputFile);
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
