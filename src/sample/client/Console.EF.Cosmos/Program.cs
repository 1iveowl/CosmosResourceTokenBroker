using System.Linq;
using System.Threading.Tasks;
using Console.EF.Cosmos.Client;
using Console.EF.Cosmos.Model;
using Console.EF.Cosmos.Service;
using CosmosResourceToken.Core.Model;
using Microsoft.EntityFrameworkCore;


namespace Console.EF.Cosmos
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var authService = new B2CAuthServiceMock();

            await using var personContext = 
                await new CosmosTokenDbContext<PersonContext>(authService, authService.ResourceTokenBrokerUrl)
                    .GetDbContextAsync(PermissionModeKind.UserReadWrite);

            //personContext.Add(new Person
            //{
            //    FirstName = "Test",
            //    LastName = "Tester",
            //    PartitionKey = "pk",
            //    Id = "Test2"
            //});

            //await personContext.SaveChangesAsync();


            var y = personContext.Persons.WithPartitionKey("user-08ca2146-5355-4d97-883f-1f9f170dcab1")
                .Where(x => x.id == "Test2")
                .ToList();

            var t = personContext.Persons.AsNoTracking().ToList();
            
            
            var person = await personContext.Persons.FindAsync(new object[] { "Test2" });

            //var query = personContext.Persons
            //        .Where(x => x.PartitionKey == "pk")
            //        .Where(x =>  x.Id == "Test2")
            //        .AsAsyncEnumerable()
            //        .GetAsyncEnumerator();
            
            //while (await query.MoveNextAsync())
            //{
            //    var t = query.Current;
            //}
            
            //var person = await personContext.Persons.FindAsync("Test1");
            
            System.Console.ReadLine();

        }

    }
}
