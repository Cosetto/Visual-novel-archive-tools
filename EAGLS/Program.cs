using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace EAGLS
{
    class Program
    {
        static void Main(string[] args)
        {
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
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

            ArcPak ArcPak = new ArcPak();
            try
            {
                ArcPak.Pack(packPath);
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
