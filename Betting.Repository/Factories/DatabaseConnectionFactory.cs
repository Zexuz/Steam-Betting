using System;
using Betting.Repository.Impl;
using Betting.Repository.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Betting.Repository.Factories
{
    public class DatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public DatabaseConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDatabaseConnection GetDatabaseConnection(Database database)
        {
            switch (database)
            {
                case Database.Main:
                    return new DatabaseConnection(_configuration.GetConnectionString("Betting"));
                case Database.Settings:
                    return new DatabaseConnection(_configuration.GetConnectionString("Betting.Settings"));
                default:
                    throw new ArgumentOutOfRangeException(nameof(database), database, null);
            }
        }
    }
}