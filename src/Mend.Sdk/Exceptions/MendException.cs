using System;

namespace Mend.Sdk.Exceptions;

public class MendException : Exception
{
    public MendException() { }
    public MendException(string message) : base(message) { }
    public MendException(string message, Exception innerException) : base(message, innerException) { }
}
