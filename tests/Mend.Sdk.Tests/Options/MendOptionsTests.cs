using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Mend.Sdk.Options;
using Xunit;

namespace Mend.Sdk.Tests.Options;

public sealed class MendOptionsTests
{
    [Fact]
    public void Binding_FromInMemoryConfig_PopulatesAllFields()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Mend:BaseUrl"] = "https://api.mend.io",
                ["Mend:OrgUuid"] = "test-org-uuid",
                ["Mend:Email"] = "test@example.com",
                ["Mend:UserKey"] = "test-user-key"
            })
            .Build();

        var options = new MendOptions();
        config.GetSection("Mend").Bind(options);

        Assert.Equal("https://api.mend.io", options.BaseUrl);
        Assert.Equal("test-org-uuid", options.OrgUuid);
        Assert.Equal("test@example.com", options.Email);
        Assert.Equal("test-user-key", options.UserKey);
    }

    [Fact]
    public void Validator_WhenAllFieldsPresent_ReturnsSuccess()
    {
        var validator = new MendOptionsValidator();
        var options = new MendOptions
        {
            BaseUrl = "https://api.mend.io",
            OrgUuid = "org-uuid",
            Email = "user@example.com",
            UserKey = "user-key"
        };

        var result = validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData("BaseUrl", "Mend:BaseUrl")]
    [InlineData("OrgUuid", "Mend:OrgUuid")]
    [InlineData("Email", "Mend:Email")]
    [InlineData("UserKey", "Mend:UserKey")]
    public void Validator_WhenRequiredFieldMissing_FailsWithDescriptiveMessage(
        string propertyName, string expectedConfigKey)
    {
        var validator = new MendOptionsValidator();
        var options = new MendOptions
        {
            BaseUrl = propertyName == "BaseUrl" ? string.Empty : "https://api.mend.io",
            OrgUuid = propertyName == "OrgUuid" ? string.Empty : "org-uuid",
            Email = propertyName == "Email" ? string.Empty : "user@example.com",
            UserKey = propertyName == "UserKey" ? string.Empty : "user-key"
        };

        var result = validator.Validate(null, options);

        Assert.False(result.Succeeded);
        Assert.Contains(expectedConfigKey, result.FailureMessage ?? string.Empty);
    }
}
