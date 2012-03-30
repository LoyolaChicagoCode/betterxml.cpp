using System;
using System.IO;


namespace BetterXml
{
    unsafe class Program
    {
        static void Main(string[] args)
        {
            string fileName;
            if (args.Length == 0)
            {
                //read from console input
                fileName = Path.GetTempFileName();
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    string line;
                    while ((line = Console.In.ReadLine()) != null)
                    {
                        sw.WriteLine(line);
                    }
                }
            }
            else
            {
                fileName = args[0];
            }

            using (Stream s = File.OpenRead(fileName))
            {
                ExpatWrap reader = new ExpatWrap();
                reader.InitParser(null);
                reader.Parse(s);
            }

#if DEBUG
          //  Console.ReadKey();
#endif


        }
    }
}
