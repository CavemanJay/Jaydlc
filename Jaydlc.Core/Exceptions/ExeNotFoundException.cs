using System;
using System.IO;

namespace Jaydlc.Core.Exceptions
{
    public class ExeNotFoundException : FileNotFoundException
    {
        private const string DefaultMessage =
            "The specified executable was not found";

        public ExeNotFoundException(string exeName) : base(
            DefaultMessage, exeName
        )
        {
        }

        public ExeNotFoundException(string exeName, Exception innerException) :
            base(DefaultMessage, exeName, innerException)
        {
        }
    }
}