using System;
using System.IO;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;

namespace Jaydlc.Commander.Client
{
    public class Compressor
    {
        private readonly string _inputFolder;

        public Compressor(string inputFolder)
        {
            this._inputFolder = inputFolder;
        }

        public string CompressTo(string destinationFolder)
        {
            using var archive = TarArchive.Create();

            var baseName = Path.GetFileName(this._inputFolder);
            var outputFile = Path.Join(destinationFolder, baseName + ".tar.gz");

            Console.WriteLine("Writing archive to " + outputFile);

            archive.AddAllFromDirectory(this._inputFolder!);
            archive.SaveTo(outputFile, CommanderClient.CompressionOptions);

            return outputFile;
        }
    }
}