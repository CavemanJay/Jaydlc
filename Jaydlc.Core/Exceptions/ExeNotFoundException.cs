using System.IO;

namespace Jaydlc.Core.Exceptions
{
    public class ExeNotFoundException : FileNotFoundException
    {
        public ExeNotFoundException(string exeName) : base("The specified exe was not found",
            exeName)
        {
        }
    }
}