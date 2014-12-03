using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace SwModelReaderCli
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }
            var modelFile = args.Last();
            var outDir = string.Empty;
            var outPosition = Array.FindIndex(args, a => a.StartsWith("/out"));
            if (outPosition >= 0 && outPosition + 1 < args.Length)
            {
                outDir = args[outPosition + 1];
            }
           
                using (var reader =  SwModelReaderCore.SwModelReader.Open(modelFile))
                {
                    string[] streamNames;
                    reader.GetAvailableStreamNames(out streamNames);
                    foreach (var item in streamNames)
                    {
                        byte[] streamData;
                        reader.GetStream(item, out streamData);
                        if (!string.IsNullOrWhiteSpace(outDir))
                        {
                            string streamPath = Path.Combine(outDir, item.Replace('/', '\\'));
                            DirectoryInfo streamDir = Directory.GetParent(streamPath);
                            if (!streamDir.Exists)
                            {
                                streamDir.Create();
                            }
                            File.WriteAllBytes(streamPath, streamData);
                            Console.WriteLine("Stream {0} saved to File {0}", streamPath, streamPath);
                        }
                        else
                        {
                            Console.WriteLine("Stream {0} available");
                        }
                    }
                

            }

        }
    }
}
