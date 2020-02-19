using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

[assembly: FunctionsStartup(typeof(AzureFunction.Broker.Startup))]
namespace AzureFunction.Broker
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // As we are working with interfaces we need additional type handling.
            // Newton Json.NET is used as the moment. Have yet to figure out how to use the newer 'System.Text.Json'
            // https://docs.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-3.0#add-newtonsoftjson-based-json-format-support
            //builder.Services
            //    .AddControllers()
            //    .AddNewtonsoftJson(options =>
            //    {
            //        options.SerializerSettings.TypeNameHandling = TypeNameHandling.All;
            //    });
        }
    }
}
