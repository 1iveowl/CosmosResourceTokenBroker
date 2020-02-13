using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(AzureFunction.Broker.Startup))]
namespace AzureFunction.Broker
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Not used in this sample.
            // Could be used for dependency injection at as described here: https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection
        }
    }
}
