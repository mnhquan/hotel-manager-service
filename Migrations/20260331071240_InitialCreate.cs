using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nguoi_dung",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ho_ten = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    so_dien_thoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cccd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ten_dang_nhap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mat_khau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    anh_dai_dien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dia_chi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lan_cuoi_hoat_dong = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nguoi_dung", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "phong",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    co_so_id = table.Column<int>(type: "int", nullable: false),
                    ten_phong = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ma_phong = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    loai_phong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    suc_chua = table.Column<int>(type: "int", nullable: false),
                    so_giuong = table.Column<int>(type: "int", nullable: true),
                    gia_co_ban = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    dien_tich = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    duoc_duyet = table.Column<bool>(type: "bit", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phong", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vai_tro",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_vai_tro = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vai_tro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "danh_gia",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nguoi_dung_id = table.Column<int>(type: "int", nullable: false),
                    phong_id = table.Column<int>(type: "int", nullable: false),
                    dat_phong_id = table.Column<int>(type: "int", nullable: true),
                    so_sao = table.Column<int>(type: "int", nullable: false),
                    binh_luan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_danh_gia", x => x.id);
                    table.ForeignKey(
                        name: "FK_danh_gia_nguoi_dung_nguoi_dung_id",
                        column: x => x.nguoi_dung_id,
                        principalTable: "nguoi_dung",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_danh_gia_phong_phong_id",
                        column: x => x.phong_id,
                        principalTable: "phong",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dat_phong",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nguoi_dung_id = table.Column<int>(type: "int", nullable: false),
                    phong_id = table.Column<int>(type: "int", nullable: false),
                    ngay_nhan_phong = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ngay_tra_phong = table.Column<DateTime>(type: "datetime2", nullable: false),
                    so_dem = table.Column<int>(type: "int", nullable: false),
                    so_nguoi = table.Column<int>(type: "int", nullable: false),
                    gio_nhan_phong = table.Column<TimeSpan>(type: "time", nullable: true),
                    gio_tra_phong = table.Column<TimeSpan>(type: "time", nullable: true),
                    gia_moi_dem = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    phi_don_dep = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    phi_dich_vu = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    thue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    tong_tien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    dat_coc = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dat_phong", x => x.id);
                    table.ForeignKey(
                        name: "FK_dat_phong_nguoi_dung_nguoi_dung_id",
                        column: x => x.nguoi_dung_id,
                        principalTable: "nguoi_dung",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dat_phong_phong_phong_id",
                        column: x => x.phong_id,
                        principalTable: "phong",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nguoi_dung_vai_tro",
                columns: table => new
                {
                    nguoi_dung_id = table.Column<int>(type: "int", nullable: false),
                    vai_tro_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nguoi_dung_vai_tro", x => new { x.nguoi_dung_id, x.vai_tro_id });
                    table.ForeignKey(
                        name: "FK_nguoi_dung_vai_tro_nguoi_dung_nguoi_dung_id",
                        column: x => x.nguoi_dung_id,
                        principalTable: "nguoi_dung",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nguoi_dung_vai_tro_vai_tro_vai_tro_id",
                        column: x => x.vai_tro_id,
                        principalTable: "vai_tro",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "thanh_toan",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dat_phong_id = table.Column<int>(type: "int", nullable: false),
                    loai_thanh_toan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    so_tien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    phuong_thuc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    han_thanh_toan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ngay_thanh_toan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_thanh_toan", x => x.id);
                    table.ForeignKey(
                        name: "FK_thanh_toan_dat_phong_dat_phong_id",
                        column: x => x.dat_phong_id,
                        principalTable: "dat_phong",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_danh_gia_nguoi_dung_id",
                table: "danh_gia",
                column: "nguoi_dung_id");

            migrationBuilder.CreateIndex(
                name: "IX_danh_gia_phong_id",
                table: "danh_gia",
                column: "phong_id");

            migrationBuilder.CreateIndex(
                name: "IX_dat_phong_nguoi_dung_id",
                table: "dat_phong",
                column: "nguoi_dung_id");

            migrationBuilder.CreateIndex(
                name: "IX_dat_phong_phong_id",
                table: "dat_phong",
                column: "phong_id");

            migrationBuilder.CreateIndex(
                name: "IX_nguoi_dung_email",
                table: "nguoi_dung",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nguoi_dung_vai_tro_vai_tro_id",
                table: "nguoi_dung_vai_tro",
                column: "vai_tro_id");

            migrationBuilder.CreateIndex(
                name: "IX_thanh_toan_dat_phong_id",
                table: "thanh_toan",
                column: "dat_phong_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "danh_gia");

            migrationBuilder.DropTable(
                name: "nguoi_dung_vai_tro");

            migrationBuilder.DropTable(
                name: "thanh_toan");

            migrationBuilder.DropTable(
                name: "vai_tro");

            migrationBuilder.DropTable(
                name: "dat_phong");

            migrationBuilder.DropTable(
                name: "nguoi_dung");

            migrationBuilder.DropTable(
                name: "phong");
        }
    }
}
