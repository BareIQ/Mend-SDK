using Mend.Sdk.AiVulnerabilities;
using Mend.Sdk.Applications;
using Mend.Sdk.Auth;
using Mend.Sdk.Client;
using Mend.Sdk.CodeFindings;
using Mend.Sdk.Dependencies;
using Mend.Sdk.Http;
using Mend.Sdk.Options;
using Mend.Sdk.Projects;
using Mend.Sdk.ReportExports;
using Mend.Sdk.Reports;
using Mend.Sdk.Scans;
using Mend.Sdk.ZeroDayEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Mend.Sdk.Extensions;

public static class MendServiceCollectionExtensions
{
    public static IServiceCollection AddMendSdk(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MendOptions>(configuration.GetSection("Mend"));
        services.AddSingleton<IValidateOptions<MendOptions>, MendOptionsValidator>();
        services.AddHttpClient();
        services.AddSingleton<IMendHttpClient, MendHttpClient>();
        services.AddSingleton<IMendTokenManager, MendTokenManager>();
        services.AddSingleton<IMendClient, MendClient>();
        services.AddSingleton<IMendProjectsClient, MendProjectsClient>();
        services.AddSingleton<IMendApplicationsClient, MendApplicationsClient>();
        services.AddSingleton<IMendScansClient, MendScansClient>();
        services.AddSingleton<IMendDependenciesClient, MendDependenciesClient>();
        services.AddSingleton<IMendCodeFindingsClient, MendCodeFindingsClient>();
        services.AddSingleton<IMendAiVulnerabilitiesClient, MendAiVulnerabilitiesClient>();
        services.AddSingleton<IMendZeroDayEventsClient, MendZeroDayEventsClient>();
        services.AddSingleton<IMendReportsClient, MendReportsClient>();
        services.AddSingleton<IMendReportExportsClient, MendReportExportsClient>();
        return services;
    }
}
