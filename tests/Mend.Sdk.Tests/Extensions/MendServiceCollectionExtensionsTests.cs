using System.Collections.Generic;
using Mend.Sdk.Client;
using Mend.Sdk.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Mend.Sdk.Tests.Extensions;

public sealed class MendServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMendSdk_ResolvesIMendClientFromContainer()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Mend:BaseUrl"] = "https://api.mend.io",
                ["Mend:OrgUuid"] = "test-org",
                ["Mend:Email"] = "test@example.com",
                ["Mend:UserKey"] = "test-key"
            })
            .Build();

        services.AddMendSdk(config);

        using var provider = services.BuildServiceProvider();
        var client = provider.GetService<IMendClient>();

        Assert.NotNull(client);
    }
}
