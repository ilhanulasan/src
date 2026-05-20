using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dental.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicPersonnel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clinic_personnel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LastName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    PersonnelType = table.Column<string>(type: "text", nullable: false),
                    Specialties = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    AppointmentResourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clinic_personnel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_clinic_personnel_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_clinic_personnel_appointment_resources_AppointmentResourceId",
                        column: x => x.AppointmentResourceId,
                        principalTable: "appointment_resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clinic_personnel_AppointmentResourceId",
                table: "clinic_personnel",
                column: "AppointmentResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_clinic_personnel_PersonnelType_IsActive",
                table: "clinic_personnel",
                columns: new[] { "PersonnelType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_clinic_personnel_UserId",
                table: "clinic_personnel",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clinic_personnel");
        }
    }
}
