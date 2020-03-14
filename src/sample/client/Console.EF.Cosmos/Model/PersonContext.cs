using Microsoft.EntityFrameworkCore;

namespace Console.EF.Cosmos.Model
{
    public class PersonContext : DBContextBase
    {
        public PersonContext(
            string resourceTokenBrokerUrl,
            string resourceToken,
            string dbName) : base(resourceTokenBrokerUrl, resourceToken, dbName)
        {

        }
    }
}
