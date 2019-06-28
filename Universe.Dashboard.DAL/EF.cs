using System;
using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using MySql.Data.EntityFrameworkCore.Extensions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Universe.Dashboard.DAL
{
    public static class EF
    {
        public enum Family
        {
            SqlServer,
            Sqlite,
            MySql,
            PgSql,
        }
        
        public static readonly Implementation.MySQLTypes MySQL = new Implementation.MySQLTypes();  
        public static readonly Implementation.SqlServerTypes SqlServer = new Implementation.SqlServerTypes();  
        public static readonly Implementation.SqliteTypes Sqlite = new Implementation.SqliteTypes();
        public static readonly Implementation.PgSQLTypes PgSQL = new Implementation.PgSQLTypes();
        
        // Microsoft.EntityFrameworkCore.Sqlite
        // Microsoft.EntityFrameworkCore.SqlServer
        // MySql.Data.EntityFrameworkCore
        // Npgsql.EntityFrameworkCore.PostgreSQL

        public static Implementation.ICrossProviderTypes GetTypes(this Family family)
        {
            if (family == Family.Sqlite) return Sqlite;
            if (family == Family.SqlServer) return SqlServer;
            if (family == Family.MySql) return MySQL;
            if (family == Family.PgSql) return PgSQL;
            throw new ArgumentException($"Unknown provider family {family}", nameof(family));
        }
        
        public static Family GetFamily(string providerType)
        {
            if (providerType == null) throw new ArgumentNullException(nameof(providerType));
            var ignore = StringComparison.InvariantCultureIgnoreCase;
            if (providerType.EndsWith(".Sqlite", ignore)) return Family.Sqlite;
            if (providerType.EndsWith(".SqlServer", ignore)) return Family.SqlServer;
            if (providerType.StartsWith("MySql.", ignore)) return Family.MySql;
            if (providerType.EndsWith(".PostgreSQL", ignore)) return Family.PgSql;
            throw new ArgumentException($"Unknown provider {providerType}", nameof(providerType));
        }

        public static Implementation.ICrossProviderTypes GetTypes(string providerType)
        {
            if (providerType == null) throw new ArgumentNullException(nameof(providerType));
            var ignore = StringComparison.InvariantCultureIgnoreCase;
            if (providerType.EndsWith(".Sqlite", ignore)) return Sqlite;
            if (providerType.EndsWith(".SqlServer", ignore)) return SqlServer;
            if (providerType.StartsWith("MySql.", ignore)) return MySQL;
            if (providerType.EndsWith(".PostgreSQL", ignore)) return PgSQL;
            throw new ArgumentException($"Unknown provider {providerType}", nameof(providerType));
        }

        public static Implementation.ICrossProviderTypes GetTypes(this MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null) throw new ArgumentNullException(nameof(migrationBuilder));
            return GetTypes(migrationBuilder.ActiveProvider);
        }

        public static Implementation.ICrossProviderTypes GetTypes(this DatabaseFacade database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            return GetTypes(database.ProviderName);
        }

        public static Family GetFamily(this DatabaseFacade database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            return GetFamily(database.ProviderName);
        }

        public static OperationBuilder<AddColumnOperation> IsAutoIncrement(this OperationBuilder<AddColumnOperation> operation)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            operation.Annotation("Sqlite:Autoincrement", true);
            operation.Annotation("MySQL:AutoIncrement", true);
            operation.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);
            operation.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);
                
            return operation;
        }


        public static class Implementation
        {
            public interface ICrossProviderTypes
            {
                string Bool { get; } 
                string String { get; } 
                string Guid { get; } 
                string Json { get; } 
                string CurrentDateTime { get; }
                string DateTime { get; }
                string GetVersionString(IDbConnection connection);
            }
            
            public class PgSQLTypes : ICrossProviderTypes
            {
                public string Bool => "BOOLEAN";
                // public string String => "VARCHAR(21844)";
                public string String => "VARCHAR(10485760)"; // ?
                public string Guid => "uuid";
                public string Json => "TEXT";
                public string DateTime => "TIMESTAMP"; 
                public string CurrentDateTime => "(now() at time zone 'utc')";
                public string GetVersionString(IDbConnection connection)
                {
                    return connection.ExecuteScalar<string>("Select version();");
                }

            }

            public class MySQLTypes : ICrossProviderTypes
            {
                public string Bool => "TINYINT";
                // public string String => "VARCHAR(21844)";
                public string String => "VARCHAR(16383)";
                public string Guid => "BINARY(16)";
                public string Json => "LONGTEXT";
                public string DateTime => "TIMESTAMP"; 


                // Works for 5.6 onward
                // public string CurrentDateTime => "CURRENT_TIMESTAMP()";
                // Does it work for 5.1?
                public string CurrentDateTime => "CURRENT_TIMESTAMP";
                public string GetVersionString(IDbConnection connection)
                {
                    return connection.ExecuteScalar<string>("Select version();");
                }

            }

            public class SqlServerTypes : ICrossProviderTypes
            {
                public string Bool => "BIT";
                public string String => "NVARCHAR(4000)";
                public string Guid => "UNIQUEIDENTIFIER";
                public string Json => "NVARCHAR(MAX)";
                public string CurrentDateTime => "GetUtcDate()";
                public string DateTime => "DATETIME";
                public string GetVersionString(IDbConnection connection)
                {
                    var ret = connection.ExecuteScalar<string>("Select @@version;").Replace("\r", " ").Replace("\n", " ");
                    while (ret.IndexOf("  ", StringComparison.Ordinal) >= 0)
                        ret = ret.Replace("  ", " ");

                    return ret;
                }

            }

            public class SqliteTypes : ICrossProviderTypes
            {
                public string Bool => "TINYINT";
                public string String => "NVARCHAR(32767)";
                public string Guid => "BINARY(16)";
                public string Json => "TEXT";
                public string CurrentDateTime => "CURRENT_TIMESTAMP";
                public string DateTime => "DATETIME";
                public string GetVersionString(IDbConnection connection)
                {
                    return connection.ExecuteScalar<string>("Select sqlite_version();");
                }
                
            }

        }
        
        
    }
}