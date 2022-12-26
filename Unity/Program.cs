using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Unity
{
    class Program
    {
        static void Main(string[] args)
        {
            string packPath;
            if (args.Length > 0)
            {
                packPath = args[0];
            }
            else
            {
                Console.WriteLine("Please input the pack path in the command line or drag a folder onto the executable.");
                return;
            }

            ArcBIN ArcBIN = new ArcBIN();
            try
            {
                ArcBIN.Pack(packPath);
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
