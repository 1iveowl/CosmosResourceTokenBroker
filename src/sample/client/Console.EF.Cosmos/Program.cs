using System.Threading.Tasks;
using Console.EF.Cosmos.Client;
using Console.EF.Cosmos.Service;


namespace Console.EF.Cosmos
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var authService = new B2CAuthServiceMock();

            var client = new CosmosTokenClientEF(authService, authService.ResourceTokenBrokerUrl);

            

        }

    }
}
