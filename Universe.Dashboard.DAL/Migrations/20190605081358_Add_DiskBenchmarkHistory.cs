using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Universe.Dashboard.DAL.Migrations
{
    public partial class Add_DiskBenchmarkHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiskBenchmark",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    MountPath = table.Column<string>(nullable: true),
                    Args = table.Column<string>(nullable: true),
                    Report = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiskBenchmark", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiskBenchmark");
        }
    }
}
