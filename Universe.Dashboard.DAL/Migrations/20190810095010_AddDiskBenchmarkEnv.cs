using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Universe.Dashboard.DAL.MultiProvider;

namespace Universe.Dashboard.DAL.Migrations
{
    public partial class AddDiskBenchmarkEnv : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var types = migrationBuilder.GetFamily().GetTypes();

            migrationBuilder.AddColumn<string>(
                name: "Environment",
                table: "W3Top_DiskBenchmark",
                type: types.Json,
                nullable: true);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "Environment",
                table: "W3Top_DiskBenchmark");

        }
    }
}
