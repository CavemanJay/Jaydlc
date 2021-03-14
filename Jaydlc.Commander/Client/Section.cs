using System;
using System.Linq;

namespace Jaydlc.Commander.Client
{
    /// <summary>
    /// Helper class to divide operations into sections.
    /// Writes the header on instantiation and writes the footer when disposed
    /// </summary>
    public class Section : IDisposable
    {
        private readonly string _sectionName;
        private readonly bool _newLine;
        private int _headerLength;

        public Section(string sectionName, bool newLine = true)
        {
            this._sectionName = sectionName;
            this._newLine = newLine;

            this.WriteHeader();
        }

        private void WriteHeader()
        {
            var message = $"############## {this._sectionName} ##############";

            Console.WriteLine(message);

            this._headerLength = message.Length;
        }

        private void WriteFooter()
        {
            var footer =
                string.Concat(Enumerable.Repeat('#', this._headerLength));

            Console.WriteLine(footer);
        }

        void IDisposable.Dispose()
        {
            this.WriteFooter();
            if (this._newLine)
                Console.WriteLine();
        }
    }
}