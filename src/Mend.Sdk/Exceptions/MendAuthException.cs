using System;

namespace Mend.Sdk.Exceptions;

public sealed class MendAuthException : MendException
{
    public string EndpointPath { get; }

    public MendAuthException(string endpointPath)
        : base($"Authentication failed for endpoint: {endpointPath}")
    {
        EndpointPath = endpointPath;
    }

    public MendAuthException(string endpointPath, Exception innerException)
        : base($"Authentication failed for endpoint: {endpointPath}", innerException)
    {
        EndpointPath = endpointPath;
    }
}
