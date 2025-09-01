using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StravaUtilities.TestHarness.Tests;
using System.Reflection;

namespace StravaUtilities.TestHarness;

internal static class Startup
{
    internal static void RegisterServices(ServiceCollection serviceCollection)
    {
        serviceCollection.Configure<StravaUtilitiesTestHarnessOptions>(
            new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false)
            .Build()
            .GetSection(nameof(StravaUtilitiesTestHarnessOptions))
        );

        serviceCollection.AddSingleton(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<StravaUtilitiesTestHarnessOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.KeyVaultUri))
            {
                throw new OptionsValidationException(nameof(StravaUtilitiesTestHarnessOptions)
                    , typeof(StravaUtilitiesTestHarnessOptions)
                    , [$"{nameof(StravaUtilitiesTestHarnessOptions.KeyVaultUri)} is not found or not valid in settings file"]);
            }

            // FYI for local testing - need to run the "az login" cmd for this to work
            return new SecretClient(new Uri(options.KeyVaultUri), new DefaultAzureCredential());
        });

        serviceCollection.AddScoped<IStravaApiTokenStorer, StravaApiTokenKeyVaultStorer>();

        serviceCollection.AddScoped(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<StravaUtilitiesTestHarnessOptions>>().Value;
            var stravaTokenStorer = serviceProvider.GetRequiredService<IStravaApiTokenStorer>();
            return new StravaApiClient(options.StravaApiClientId, options.StravaApiClientSecret, stravaTokenStorer);
        });

        AddTestClasses(serviceCollection);

        serviceCollection.AddScoped<ManualTestRunner>();
    }

    private static void AddTestClasses(ServiceCollection serviceCollection)
    {
        var testClasses = GetClassesImplementingInterface<IStravaUtilitiesTest>();
        foreach (var testClass in testClasses)
            serviceCollection.AddScoped(testClass);
    }

    private static List<Type> GetClassesImplementingInterface<T>()
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(T).IsAssignableFrom(t) && t.IsClass)
            .ToList();
    }
}
