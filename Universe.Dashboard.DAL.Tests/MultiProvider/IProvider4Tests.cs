using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Universe.Dashboard.DAL.MultiProvider;

namespace Universe.Dashboard.DAL.Tests.MultiProvider
{
    public interface IProvider4Tests
    {
        string EnvVarName { get; }
        IProvider4Runtime Provider4Runtime { get; }
        List<string> GetServerConnectionStrings();
        string CreateDatabase(string serverConnectionString, string dbName);
        void DropDatabase(string serverConnectionString, string dbName);

        // TODO: Move to IProvider4Runtime
        string GetServerName(string connectionString);
        
    }    
}