﻿using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using WB.Services.Infrastructure.Health;

namespace WB.Services.Scheduler.Storage
{
    internal class DbHealthCheck : IHealthCheck
    {
        private readonly IConfiguration configuration;

        public DbHealthCheck(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<bool> CheckAsync()
        {
            using (var connection = new NpgsqlConnection(this.configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.QueryAsync("SELECT version();");
                return true;
            }
        }

        public string Name => "Database Connection";
    }
}