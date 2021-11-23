using System;
using System.IO;
using System.IO.Compression;
using CommandLine;

namespace Decompression
{
    static class Program
    {
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts =>
                {
                    try
                    {
                        if (!Directory.Exists(opts.ExtractPath))
                        {
                            Directory.CreateDirectory(opts.ExtractPath);
                        }
                        else
                        {
                            Directory.Delete(opts.ExtractPath, true);
                        }
                        var watch = System.Diagnostics.Stopwatch.StartNew();

                        using (var archive = ZipFile.OpenRead(opts.ZipFile))
                        {
                            foreach (var entry in archive.Entries)
                            {
                                var destinationPath = Path.GetFullPath(Path.Combine(opts.ExtractPath, entry.FullName));
                                var dir = Path.GetDirectoryName(destinationPath);
                                if (!Directory.Exists(dir))
                                {
                                    Directory.CreateDirectory(dir);
                                }

                                // Skip folders
                                if (entry.FullName[^1] == '/')
                                {
                                    continue;
                                }

                                entry.ExtractToFile(destinationPath);
                            }
                        }


                        watch.Stop();
                        Console.WriteLine("Elapsed: {0}", watch.Elapsed);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
        }
    }

    public class Options
    {
        [Value(index: 0, Required = true, HelpText = "Zip path")]
        public string ZipFile { get; set; }

        [Option(shortName: 'e', longName: "extract", Required = true, HelpText = "Extraction path")]
        public string ExtractPath { get; set; }
    }
}