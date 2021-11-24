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
                        var invalidFileNameChars = Path.GetInvalidFileNameChars();
                        var invalidFilePathChars = Path.GetInvalidPathChars();

                        using (var archive = ZipFile.OpenRead(opts.ZipFile))
                        {
                            foreach (var entry in archive.Entries)
                            {
                                var zippedDir = entry.FullName.AsSpan()[.. (entry.FullName.Length - entry.Name.Length)].ToString();
                                var d = string.Join("_", zippedDir.Split(invalidFilePathChars));
                                var f = string.Join("_", entry.Name.Split(invalidFileNameChars));
                                var full = Path.Combine(d, f);
                                
                                var destinationPath = Path.GetFullPath(Path.Combine(opts.ExtractPath, full));
                                var dir = Path.GetDirectoryName(destinationPath);
                                if (dir != null && !Directory.Exists(dir))
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