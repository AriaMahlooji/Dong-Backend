using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebAPI.Migrations
{
    public partial class RoozeAkhar111 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinutesRemained",
                table: "Request");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Remained",
                table: "Request",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.CreateIndex(
                name: "IX_Request_BuildingId",
                table: "Request",
                column: "BuildingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Request_Buildings_BuildingId",
                table: "Request",
                column: "BuildingId",
                principalTable: "Buildings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Request_Buildings_BuildingId",
                table: "Request");

            migrationBuilder.DropIndex(
                name: "IX_Request_BuildingId",
                table: "Request");

            migrationBuilder.DropColumn(
                name: "Remained",
                table: "Request");

            migrationBuilder.AddColumn<int>(
                name: "MinutesRemained",
                table: "Request",
                nullable: false,
                defaultValue: 0);
        }
    }
}
