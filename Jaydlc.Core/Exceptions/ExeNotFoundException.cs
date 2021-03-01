using System.IO;

namespace Jaydlc.Core.Exceptions
{
    public class ExeNotFoundException : FileNotFoundException
    {
        public ExeNotFoundException(string exeName) : base("The specified executable was not found",
            exeName)
        {
        }
    }
}