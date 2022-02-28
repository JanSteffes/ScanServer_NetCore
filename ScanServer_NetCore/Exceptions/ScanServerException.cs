using System;

namespace ScanServer_NetCore.Exceptions
{
    /// <summary>
    /// Exception to handle logical errors while processing (e.g. all handled cases)
    /// </summary>
    public class ScanServerException : Exception
    {
        public ScanServerException() : base()
        {

        }

        public ScanServerException(string message) : base(message)
        {

        }

        public ScanServerException(string message, Exception innerException) : base(message, innerException)
        {

        }


    }
}
