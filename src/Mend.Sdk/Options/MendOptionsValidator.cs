using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Mend.Sdk.Options;

public sealed class MendOptionsValidator : IValidateOptions<MendOptions>
{
    public ValidateOptionsResult Validate(string? name, MendOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            failures.Add("Mend:BaseUrl is required and must not be empty.");
        if (string.IsNullOrWhiteSpace(options.OrgUuid))
            failures.Add("Mend:OrgUuid is required and must not be empty.");
        if (string.IsNullOrWhiteSpace(options.Email))
            failures.Add("Mend:Email is required and must not be empty.");
        if (string.IsNullOrWhiteSpace(options.UserKey))
            failures.Add("Mend:UserKey is required and must not be empty.");

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
