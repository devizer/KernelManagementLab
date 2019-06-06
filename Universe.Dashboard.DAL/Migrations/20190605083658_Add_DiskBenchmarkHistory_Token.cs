using Microsoft.EntityFrameworkCore.Migrations;

namespace Universe.Dashboard.DAL.Migrations
{
    public partial class Add_DiskBenchmarkHistory_Token : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "DiskBenchmark",
                nullable: true);

            migrationBuilder.Sql("Create Unique Index IX_DiskBenchmark_Token On DiskBenchmark(Token);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Token",
                table: "DiskBenchmark");
        }
    }
}
