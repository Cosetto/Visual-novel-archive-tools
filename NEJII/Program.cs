using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NEJII
{
    class Program
    {
        static void Main(string[] args)
        {
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ");
				Console.WriteLine("         Unpack: CDT.exe -u <input file> <output folder>");
				Console.WriteLine("         Pack:   CDT.exe -p <input folder> <output file>");
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
            ArcCDT ArcCDT = new ArcCDT();
            try
            {
                Directory.CreateDirectory(outputFile);

                foreach (Entry entry in ArcCDT.TryOpen(inputFile))
                {
                    byte[] bytes = ArcCDT.Unpack(entry);
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
            ArcCDT ArcCDT = new ArcCDT();
            try
            {
                ArcCDT.Pack(inputFile, outputFile);
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
