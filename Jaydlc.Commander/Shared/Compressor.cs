using System.IO;
using Jaydlc.Commander.Client;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;

namespace Jaydlc.Commander.Shared
{
    public class Compressor
    {
        private readonly string _inputFolder;

        public Compressor(string inputFolder)
        {
            this._inputFolder = inputFolder;
        }

        public string CompressTo(string destination)
        {
            using var archive = TarArchive.Create();
            const string? fileExtension = ".tar.gz";

            var baseName = Path.GetFileName(this._inputFolder);

            string outputFile = !destination.Contains(fileExtension)
                ? Path.Join(destination, baseName + fileExtension)
                : destination;

            archive.AddAllFromDirectory(this._inputFolder!);
            archive.SaveTo(outputFile, CommanderClient.CompressionOptions);

            return outputFile;
        }
    }
}