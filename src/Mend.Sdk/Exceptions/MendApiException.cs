using System;
using System.Net;

namespace Mend.Sdk.Exceptions;

public sealed class MendApiException : MendException
{
    public HttpStatusCode StatusCode { get; }
    public string ResponseBody { get; }

    public MendApiException(HttpStatusCode statusCode, string responseBody)
        : base($"API request failed with status {(int)statusCode} ({statusCode}): {responseBody}")
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public MendApiException(HttpStatusCode statusCode, string responseBody, Exception innerException)
        : base($"API request failed with status {(int)statusCode} ({statusCode}): {responseBody}", innerException)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}
