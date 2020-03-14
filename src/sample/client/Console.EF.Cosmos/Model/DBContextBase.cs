using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Console.EF.Cosmos.Model
{
    public abstract class DBContextBase : DbContext
    {
        private readonly string _resourceTokenBrokerUrl;
        private readonly string _resourceToken;
        private readonly string _dbName;

        public DBContextBase(
            string resourceTokenBrokerUrl,
            string resourceToken,
            string dbName)
        {
            _resourceTokenBrokerUrl = resourceTokenBrokerUrl;
            _resourceToken = resourceToken;
            _dbName = dbName;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseCosmos(_resourceTokenBrokerUrl, _resourceToken, _dbName);
            base.OnConfiguring(optionsBuilder);
        }

    }
}
