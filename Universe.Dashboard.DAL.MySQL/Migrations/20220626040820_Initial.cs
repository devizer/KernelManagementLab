using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

namespace Universe.Dashboard.DAL.MySQL.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DbInfo",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Version = table.Column<string>(type: "VARCHAR(20000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiskBenchmark",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Token = table.Column<string>(type: "VARCHAR(36)", nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    MountPath = table.Column<string>(type: "VARCHAR(20000)", nullable: true),
                    Args = table.Column<string>(type: "LONGTEXT", nullable: true),
                    Report = table.Column<string>(type: "LONGTEXT", nullable: true),
                    ErrorInfo = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiskBenchmark", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoryCopy",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "VARCHAR(20000)", nullable: true),
                    JsonBlob = table.Column<string>(type: "LONGTEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryCopy", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DbInfo");

            migrationBuilder.DropTable(
                name: "DiskBenchmark");

            migrationBuilder.DropTable(
                name: "HistoryCopy");
        }
    }
}
