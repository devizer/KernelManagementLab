using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Universe.Dashboard.DAL.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var types = migrationBuilder.GetFamily().GetTypes();
            
            
            migrationBuilder.CreateTable(
                name: "W3Top_DbInfo",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false).IsAutoIncrement(),
                    Version = table.Column<string>(type: types.String, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_W3Top_DbInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "W3Top_DiskBenchmark",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false).IsAutoIncrement(),
                    Token = table.Column<Guid>(type: types.Guid, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false, type: types.DateTime, defaultValueSql: types.CurrentDateTime),
                    MountPath = table.Column<string>(type: types.String, nullable: true),
                    Args = table.Column<string>(type: types.Json, nullable: true),
                    Report = table.Column<string>(type: types.Json, nullable: true),
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
                    Id = table.Column<int>(nullable: false).IsAutoIncrement(),
                    Key = table.Column<string>(type: types.String, nullable: true),
                    JsonBlob = table.Column<string>(type: types.Json, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_W3Top_HistoryCopy", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_W3Top_DiskBenchmark_Token",
                table: "W3Top_DiskBenchmark",
                column: "Token",
                unique: true);
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
