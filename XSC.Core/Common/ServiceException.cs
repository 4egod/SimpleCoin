using System;

namespace XSC
{
    public class ServiceException : Exception
    {
        public ServiceException(string message) : base(message) { }
    }
}
