using System;
using System.Net;

namespace Shared.Shared.Web.Exceptions
{
    public class NoneSuccessFullStatusCodeException : Exception
    {
        public NoneSuccessFullStatusCodeException(HttpStatusCode statusCode, string message) : base(
            $"Statuscode: {statusCode}, message: {message}")
        {
        }
    }
}