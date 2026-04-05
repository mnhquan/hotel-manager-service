using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerResourcesSyncFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "so_du_vi",
                table: "doi_tac",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "tai_khoan_ngan_hang_doi_tac",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    doi_tac_id = table.Column<int>(type: "int", nullable: false),
                    ten_ngan_hang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    so_tai_khoan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    chu_tai_khoan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    la_tai_khoan_chinh = table.Column<bool>(type: "bit", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tai_khoan_ngan_hang_doi_tac", x => x.id);
                    table.ForeignKey(
                        name: "FK_tai_khoan_ngan_hang_doi_tac_doi_tac_doi_tac_id",
                        column: x => x.doi_tac_id,
                        principalTable: "doi_tac",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tai_khoan_ngan_hang_doi_tac_doi_tac_id",
                table: "tai_khoan_ngan_hang_doi_tac",
                column: "doi_tac_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_phong_co_so_luu_tru_co_so_id",
                table: "phong");

            migrationBuilder.DropTable(
                name: "anh_phong");

            migrationBuilder.DropTable(
                name: "co_so_luu_tru");

            migrationBuilder.DropTable(
                name: "tai_khoan_ngan_hang_doi_tac");

            migrationBuilder.DropTable(
                name: "thanh_toan_doi_tac");

            migrationBuilder.DropTable(
                name: "doi_tac");

            migrationBuilder.DropIndex(
                name: "IX_phong_co_so_id",
                table: "phong");
        }
    }
}
