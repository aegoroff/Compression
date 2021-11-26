using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
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

                        var invalidFileNameChars = Path.GetInvalidFileNameChars().ToDictionary(x => x, x => $"0x{Convert.ToInt32(x):x2}");
                        var invalidFilePathChars = Path.GetInvalidPathChars().ToDictionary(x => x, x => $"0x{Convert.ToInt32(x):x2}");

                        using (var archive = ZipFile.OpenRead(opts.ZipFile))
                        {
                            foreach (var entry in archive.Entries)
                            {
                                var dirSpan = entry.FullName.AsSpan()[.. (entry.FullName.Length - entry.Name.Length)];
                                var d = ReplaceInvalidChars(dirSpan, invalidFilePathChars);
                                var f = ReplaceInvalidChars(entry.Name.AsSpan(), invalidFileNameChars);

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

        private static string ReplaceInvalidChars(ReadOnlySpan<char> s, IReadOnlyDictionary<char, string> invalids)
        {
            var result = new StringBuilder();
            foreach (var c in s)
            {
                if (invalids.TryGetValue(c, out var replacement))
                {
                    result.Append(replacement);
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
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