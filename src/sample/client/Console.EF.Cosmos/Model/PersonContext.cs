using System;
using Console.EF.Cosmos.Client;
using Microsoft.EntityFrameworkCore;

namespace Console.EF.Cosmos.Model
{
    [Preserve(AllMembers = true)]
    public class PersonContext : TokenClientDbContextBase
    {
        public DbSet<Person> Persons { get; set; }

        public PersonContext(
            string resourceTokenBrokerUrl,
            string resourceToken,
            string dbName,
            string partitionKey) : base(resourceTokenBrokerUrl, resourceToken, dbName, partitionKey)
        {

        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>().HasPartitionKey(o => o.PartitionKey);

            modelBuilder.HasDefaultContainer("mycollection");

            modelBuilder.Entity<Person>().HasNoDiscriminator();

            base.OnModelCreating(modelBuilder);
        }

        public PersonContext() { }
    }
}
