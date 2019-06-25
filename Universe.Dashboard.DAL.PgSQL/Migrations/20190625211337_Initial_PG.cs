using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Universe.Dashboard.DAL.PgSQL.Migrations
{
    public partial class Initial_PG : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "W3Top_DbInfo",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Version = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_W3Top_DbInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "W3Top_DiskBenchmark",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Token = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    MountPath = table.Column<string>(nullable: true),
                    Args = table.Column<string>(nullable: true),
                    Report = table.Column<string>(nullable: true),
                    ErrorInfo = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_W3Top_DiskBenchmark", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "W3Top_HistoryCopy",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Key = table.Column<string>(nullable: true),
                    JsonBlob = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_W3Top_HistoryCopy", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "W3Top_DbInfo");

            migrationBuilder.DropTable(
                name: "W3Top_DiskBenchmark");

            migrationBuilder.DropTable(
                name: "W3Top_HistoryCopy");
        }
    }
}
