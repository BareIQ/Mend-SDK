using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Mend.Tool.Config;

internal static class MendCliConfig
{
    public static string ConfigPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".mendcli", "config.json");

    public static IConfiguration Load()
    {
        return new ConfigurationBuilder()
            .AddJsonFile(ConfigPath, optional: false)
            .Build();
    }

    public static IConfiguration BuildInMemory(string baseUrl, string orgUuid, string email, string userKey)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>
            {
                ["Mend:BaseUrl"]  = baseUrl,
                ["Mend:OrgUuid"]  = orgUuid,
                ["Mend:Email"]    = email,
                ["Mend:UserKey"]  = userKey,
            })
            .Build();
    }
}
