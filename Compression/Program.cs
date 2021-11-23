using System;
using System.IO.Compression;
using CommandLine;

namespace Compression
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
                        var watch = System.Diagnostics.Stopwatch.StartNew();
                        ZipFile.CreateFromDirectory(opts.Folder, opts.ZipFile);
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
        [Value(index: 0, Required = true, HelpText = "Folder path")]
        public string Folder { get; set; }

        [Option(shortName: 'z', longName: "zip", Required = true, HelpText = "Result zip file")]
        public string ZipFile { get; set; }
    }
}