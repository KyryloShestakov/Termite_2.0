using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLib.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedPeerInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Neighbors",
                table: "PeersInfo");

            migrationBuilder.DropColumn(
                name: "PublicKey",
                table: "PeersInfo");

            migrationBuilder.DropColumn(
                name: "SessionKey",
                table: "PeersInfo");

            migrationBuilder.DropColumn(
                name: "TransactionCount",
                table: "PeersInfo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Neighbors",
                table: "PeersInfo",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicKey",
                table: "PeersInfo",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionKey",
                table: "PeersInfo",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransactionCount",
                table: "PeersInfo",
                type: "INTEGER",
                nullable: true);
        }
    }
}
