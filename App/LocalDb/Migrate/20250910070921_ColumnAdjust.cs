using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.LocalDb.Migrate
{
    /// <inheritdoc />
    public partial class ColumnAdjust : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Frame",
                table: "Frame");

            migrationBuilder.DropIndex(
                name: "IX_Frame_NameNumber",
                table: "Frame");

            migrationBuilder.DropColumn(
                name: "NameNumber",
                table: "Frame");

            migrationBuilder.RenameTable(
                name: "Frame",
                newName: "FrameModel");

            migrationBuilder.RenameColumn(
                name: "Brightness",
                table: "FrameModel",
                newName: "Frame");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FrameModel",
                table: "FrameModel",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FrameModel",
                table: "FrameModel");

            migrationBuilder.RenameTable(
                name: "FrameModel",
                newName: "Frame");

            migrationBuilder.RenameColumn(
                name: "Frame",
                table: "Frame",
                newName: "Brightness");

            migrationBuilder.AddColumn<int>(
                name: "NameNumber",
                table: "Frame",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Frame",
                table: "Frame",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Frame_NameNumber",
                table: "Frame",
                column: "NameNumber",
                unique: true);
        }
    }
}
