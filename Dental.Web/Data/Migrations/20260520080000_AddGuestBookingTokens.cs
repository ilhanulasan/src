using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dental.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestBookingTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GuestConfirmationToken",
                table: "appointments",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationInviteToken",
                table: "patients",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RegistrationInviteExpiresAt",
                table: "patients",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_appointments_GuestConfirmationToken",
                table: "appointments",
                column: "GuestConfirmationToken");

            migrationBuilder.CreateIndex(
                name: "IX_patients_RegistrationInviteToken",
                table: "patients",
                column: "RegistrationInviteToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_patients_RegistrationInviteToken",
                table: "patients");

            migrationBuilder.DropIndex(
                name: "IX_appointments_GuestConfirmationToken",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "RegistrationInviteExpiresAt",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "RegistrationInviteToken",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "GuestConfirmationToken",
                table: "appointments");
        }
    }
}
