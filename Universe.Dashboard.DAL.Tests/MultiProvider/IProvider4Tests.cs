using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Universe.Dashboard.DAL.Tests.MultiProvider
{
    public interface IProvider4Tests
    {
        List<string> GetServerConnectionStrings();
        string CreateDatabase(string serverConnectionString, string dbName);
        void DropDatabase(string serverConnectionString, string dbName);

        void ApplyDbContextOptions(DbContextOptionsBuilder optionsBuilder, string connectionString);

        void Migrate(DbContext db);

        string GetServerName(string connectionString);
        
        string EnvVarName { get; }
    }    
}