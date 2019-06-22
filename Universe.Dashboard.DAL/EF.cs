using System;
using Dapper;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace Universe.Dashboard.DAL
{
    public static class EF
    {
        public enum Family
        {
            SqlServer,
            Sqlite,
            MySql,
        }
        
        public static readonly Implementation.MySQLTypes MySQL = new Implementation.MySQLTypes();  
        public static readonly Implementation.SqlServerTypes SqlServer = new Implementation.SqlServerTypes();  
        public static readonly Implementation.SqliteTypes Sqlite = new Implementation.SqliteTypes();
        
        // Microsoft.EntityFrameworkCore.Sqlite
        // Microsoft.EntityFrameworkCore.SqlServer
        // MySql.Data.EntityFrameworkCore

        public static Implementation.ICrossProviderTypes GetTypes(this Family family)
        {
            if (family == Family.Sqlite) return Sqlite;
            if (family == Family.SqlServer) return SqlServer;
            if (family == Family.MySql) return MySQL;
            throw new ArgumentException($"Unknown provider family {family}", nameof(family));
        }
        
        public static Implementation.ICrossProviderTypes GetTypes(this MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder == null) throw new ArgumentNullException(nameof(migrationBuilder));
            var providerType = migrationBuilder.ActiveProvider;
            var ignore = StringComparison.InvariantCultureIgnoreCase;
            if (providerType.EndsWith(".Sqlite", ignore)) return Sqlite;
            if (providerType.EndsWith(".SqlServer", ignore)) return SqlServer;
            if (providerType.StartsWith("MySql.", ignore)) return MySQL;
            throw new ArgumentException($"Unknown provider {providerType}", nameof(migrationBuilder));
        }

        public static OperationBuilder<AddColumnOperation> IsAutoIncrement(this OperationBuilder<AddColumnOperation> operation)
            
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            operation.Annotation("Sqlite:Autoincrement", true);
            operation.Annotation("MySQL:AutoIncrement", true);
            operation.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);
                
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
            }
            
            public class MySQLTypes : ICrossProviderTypes
            {
                public string Bool => "TINYINT";
                // public string String => "VARCHAR(21844)";
                public string String => "VARCHAR(16383)";
                public string Guid => "BINARY(16)";
                public string Json => "LONGTEXT";
            }

            public class SqlServerTypes : ICrossProviderTypes
            {
                public string Bool => "BIT";
                public string String => "NVARCHAR(4000)";
                public string Guid => "UNIQUEIDENTIFIER";
                public string Json => "NVARCHAR(MAX)";
            }

            public class SqliteTypes : ICrossProviderTypes
            {
                public string Bool => "TINYINT";
                public string String => "NVARCHAR(32767)";
                public string Guid => "BINARY(16)";
                public string Json => "TEXT";
            }

        }
        
        
    }
}