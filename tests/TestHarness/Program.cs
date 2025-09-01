using Microsoft.Extensions.DependencyInjection;
using StravaUtilities.TestHarness;

var serviceCollection = new ServiceCollection();

Startup.RegisterServices(serviceCollection);

var serviceProvider = serviceCollection.BuildServiceProvider();

var manualTestRunner = serviceProvider.GetRequiredService<ManualTestRunner>();
await manualTestRunner.Run().ConfigureAwait(false);
