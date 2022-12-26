using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GPX
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: ");
                Console.WriteLine("    Unpack: GPX.exe -u <input file> <output folder>");
                Console.WriteLine("    Pack:   GPX.exe -p <input folder> <output file>");
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

        static void Unpack(string inputFile, string outputFolder)
        {
            ArcGPX arcGPX = new ArcGPX();
            try
            {
                Directory.CreateDirectory(outputFolder);

                List<PackEntry> entries = arcGPX.TryOpen(inputFile);
                foreach (PackEntry entry in entries)
                {
                    byte[] bytes = arcGPX.Unpack(entry);
                    string filePath = Path.Combine(outputFolder, entry.Name);
                    string directory = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    File.WriteAllBytes(filePath, bytes);
                }

                // Create block.tmp
                File.WriteAllBytes(Path.Combine(outputFolder, "block.tmp"), arcGPX.fileHead);

                // Create FileList.txt
                using (StreamWriter writer = new StreamWriter(new FileStream(Path.Combine(outputFolder, "FileList.txt"), FileMode.Create), Encoding.UTF8))
                {
                    for (int i = 0; i < entries.Count; i++)
                    {
                        writer.WriteLine(entries[i].Name);
                        writer.Flush();
                        writer.WriteLine(entries[i].c1.ToString());
                        writer.Flush();
                        writer.WriteLine(entries[i].c2.ToString());
                        writer.Flush();
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

        static void Pack(string inputFolder, string outputFile)
        {
            ArcGPX arcGPX = new ArcGPX();
            try
            {
                arcGPX.Pack(inputFolder, outputFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            Console.WriteLine("Packed successfully!");
		}
	}
}
