namespace Mend.Sdk.Http;

internal sealed class ApiEnvelope<T>
{
    public T? Response { get; set; }
}
