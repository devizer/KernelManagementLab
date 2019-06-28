using System.Collections.Generic;

namespace Universe.Dashboard.DAL.Tests.MultiProvider
{
    public interface IProvider4Tests
    {
        List<string> GetServerConnectionStrings();
        string CreateDatabase(string serverConnectionString, string dbName);
        void DropDatabase(string serverConnectionString, string dbName);
    }
}