using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lilim
{
    class Program
    {
        static void Main(string[] args)
        {
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ");
				Console.WriteLine("         Unpack: Aos.exe -u <input file> <output folder>");
				Console.WriteLine("         Pack:   Aos.exe -p <input folder> <output file>");
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
            ArcAOS2 ArcAOS2 = new ArcAOS2();
            try
            {
                Directory.CreateDirectory(outputFile);

                foreach (Entry entry in ArcAOS2.TryOpen(inputFile))
                {
                    byte[] bytes = ArcAOS2.Unpack(entry);
                    System.IO.File.WriteAllBytes(Path.Combine(outputFile, entry.Name), bytes);
                }
				
				System.IO.File.WriteAllBytes(Path.Combine(outputFile, "block.tmp"), ArcAOS2.blockBuffer);
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
            ArcAOS2 ArcAOS2 = new ArcAOS2();
            try
            {
                ArcAOS2.Pack(inputFile, outputFile);
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
