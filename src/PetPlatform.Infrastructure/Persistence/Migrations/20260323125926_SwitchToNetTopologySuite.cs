using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace PetPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SwitchToNetTopologySuite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastKnownLatitude",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastKnownLongitude",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "PostSightings");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "PostSightings");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Posts");

            migrationBuilder.AddColumn<Point>(
                name: "Location",
                table: "Users",
                type: "geography",
                nullable: true);

            migrationBuilder.AddColumn<Point>(
                name: "Location",
                table: "PostSightings",
                type: "geography",
                nullable: false);

            migrationBuilder.AddColumn<Point>(
                name: "Location",
                table: "Posts",
                type: "geography",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "PostSightings");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Posts");

            migrationBuilder.AddColumn<double>(
                name: "LastKnownLatitude",
                table: "Users",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LastKnownLongitude",
                table: "Users",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "PostSightings",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "PostSightings",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Posts",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Posts",
                type: "float",
                nullable: true);
        }
    }
}
