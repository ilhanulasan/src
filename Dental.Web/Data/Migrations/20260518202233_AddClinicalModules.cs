using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Dental.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BloodType",
                table: "patients",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClinicalSummary",
                table: "patients",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "patients",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "patients",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "patients",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "patients",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                table: "patients",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "patients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "patients",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "patients",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "patients",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "appointment_resources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ResourceType = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    DefaultDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointment_resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_appointment_resources_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "examinations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorUserId = table.Column<string>(type: "text", nullable: true),
                    ExaminedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ChiefComplaint = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ClinicalFindings = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_examinations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_examinations_AspNetUsers_DoctorUserId",
                        column: x => x.DoctorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_examinations_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "financial_accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AccountType = table.Column<string>(type: "text", nullable: false),
                    AccountNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financial_accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "icd10_codes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    DescriptionTr = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    DescriptionEn = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Category = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_icd10_codes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "patient_allergies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Substance = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Reaction = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_allergies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_patient_allergies_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "patient_chronic_conditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConditionName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Icd10Code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    DiagnosedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_chronic_conditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_patient_chronic_conditions_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "patient_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IsEncrypted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_patient_documents_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "patient_kvkk_consents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsentType = table.Column<string>(type: "text", nullable: false),
                    IsGranted = table.Column<bool>(type: "boolean", nullable: false),
                    ConsentedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ConsentVersion = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_kvkk_consents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_patient_kvkk_consents_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "patient_medical_histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RecordedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    RecordedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_medical_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_patient_medical_histories_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContactPerson = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Phone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Address = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    TaxNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recurring_appointment_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrimaryResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Frequency = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recurring_appointment_templates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recurring_appointment_templates_appointment_resources_Prima~",
                        column: x => x.PrimaryResourceId,
                        principalTable: "appointment_resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_recurring_appointment_templates_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "waitlist_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreferredResourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    PreferredFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PreferredTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_waitlist_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_waitlist_entries_appointment_resources_PreferredResourceId",
                        column: x => x.PreferredResourceId,
                        principalTable: "appointment_resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_waitlist_entries_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ExaminationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_invoices_examinations_ExaminationId",
                        column: x => x.ExaminationId,
                        principalTable: "examinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_invoices_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "medical_interventions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExaminationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ToothNumbers = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PerformedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PerformedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medical_interventions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_medical_interventions_examinations_ExaminationId",
                        column: x => x.ExaminationId,
                        principalTable: "examinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "patient_clinical_notes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Content = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    IsConfidential = table.Column<bool>(type: "boolean", nullable: false),
                    ExaminationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_clinical_notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_patient_clinical_notes_examinations_ExaminationId",
                        column: x => x.ExaminationId,
                        principalTable: "examinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_patient_clinical_notes_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "treatment_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExaminationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    PlannedStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PlannedEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EstimatedTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_treatment_plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_treatment_plans_examinations_ExaminationId",
                        column: x => x.ExaminationId,
                        principalTable: "examinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_treatment_plans_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "examination_diagnoses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExaminationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Icd10CodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_examination_diagnoses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_examination_diagnoses_examinations_ExaminationId",
                        column: x => x.ExaminationId,
                        principalTable: "examinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_examination_diagnoses_icd10_codes_Icd10CodeId",
                        column: x => x.Icd10CodeId,
                        principalTable: "icd10_codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Sku = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Barcode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Unit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    UnitCost = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    MinimumStockLevel = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_products_suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrimaryResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsOnlineBooking = table.Column<bool>(type: "boolean", nullable: false),
                    RecurringTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_appointments_appointment_resources_PrimaryResourceId",
                        column: x => x.PrimaryResourceId,
                        principalTable: "appointment_resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_appointments_recurring_appointment_templates_RecurringTempl~",
                        column: x => x.RecurringTemplateId,
                        principalTable: "recurring_appointment_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "invoice_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric", nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_invoice_lines_invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_installment_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    InstallmentCount = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_installment_plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payment_installment_plans_invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_payment_installment_plans_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "treatment_plan_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TreatmentPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedureName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ToothNumbers = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_treatment_plan_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_treatment_plan_items_treatment_plans_TreatmentPlanId",
                        column: x => x.TreatmentPlanId,
                        principalTable: "treatment_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_lots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Barcode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    BatchNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    QuantityOnHand = table.Column<decimal>(type: "numeric", nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ReceivedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_lots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stock_lots_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "appointment_resource_links",
                columns: table => new
                {
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointment_resource_links", x => new { x.AppointmentId, x.ResourceId });
                    table.ForeignKey(
                        name: "FK_appointment_resource_links_appointment_resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "appointment_resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointment_resource_links_appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sms_reminder_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Message = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ScheduledFor = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ProviderResponse = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sms_reminder_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sms_reminder_logs_appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    FinancialAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Method = table.Column<string>(type: "text", nullable: false),
                    PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Reference = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    InstallmentPlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payments_financial_accounts_FinancialAccountId",
                        column: x => x.FinancialAccountId,
                        principalTable: "financial_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payments_invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_payments_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_payments_payment_installment_plans_InstallmentPlanId",
                        column: x => x.InstallmentPlanId,
                        principalTable: "payment_installment_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "stock_movements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StockLotId = table.Column<Guid>(type: "uuid", nullable: false),
                    MovementType = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    MovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Reference = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_movements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stock_movements_stock_lots_StockLotId",
                        column: x => x.StockLotId,
                        principalTable: "stock_lots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "patient_ledger_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryType = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    EntryDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_ledger_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_patient_ledger_entries_invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_patient_ledger_entries_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_patient_ledger_entries_payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "payment_installments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstallmentNumber = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_installments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payment_installments_payment_installment_plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "payment_installment_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_payment_installments_payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "icd10_codes",
                columns: new[] { "Id", "Category", "Code", "DescriptionEn", "DescriptionTr", "IsActive" },
                values: new object[,]
                {
                    { new Guid("a1000001-0000-4000-8000-000000000001"), "Diş", "K02", "Dental caries", "Diş çürüğü", true },
                    { new Guid("a1000001-0000-4000-8000-000000000002"), "Diş", "K04.0", "Pulpitis", "Pulpitis", true },
                    { new Guid("a1000001-0000-4000-8000-000000000003"), "Diş", "K04.7", "Periapical abscess", "Periapikal apse", true },
                    { new Guid("a1000001-0000-4000-8000-000000000004"), "Dişeti", "K05.0", "Acute gingivitis", "Akut gingivit", true },
                    { new Guid("a1000001-0000-4000-8000-000000000005"), "Dişeti", "K05.3", "Chronic periodontitis", "Kronik periodontitis", true },
                    { new Guid("a1000001-0000-4000-8000-000000000006"), "Diş", "K08.1", "Loss of teeth", "Diş kaybı", true },
                    { new Guid("a1000001-0000-4000-8000-000000000007"), "Diş", "K08.3", "Retained dental root", "Kalan kök", true },
                    { new Guid("a1000001-0000-4000-8000-000000000008"), "Çene", "K10.2", "Inflammatory conditions of jaws", "Çene inflamasyonu", true },
                    { new Guid("a1000001-0000-4000-8000-000000000009"), "Ağız", "K12.0", "Recurrent oral aphthae", "Rekürren aftöz stomatit", true },
                    { new Guid("a1000001-0000-4000-8000-000000000010"), "Ağız", "K13.0", "Diseases of lips", "Dudak çatlakları", true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_patients_UserId",
                table: "patients",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_resource_links_ResourceId",
                table: "appointment_resource_links",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_resources_UserId",
                table: "appointment_resources",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_PatientId",
                table: "appointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_PrimaryResourceId_StartAt",
                table: "appointments",
                columns: new[] { "PrimaryResourceId", "StartAt" });

            migrationBuilder.CreateIndex(
                name: "IX_appointments_RecurringTemplateId",
                table: "appointments",
                column: "RecurringTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_examination_diagnoses_ExaminationId",
                table: "examination_diagnoses",
                column: "ExaminationId");

            migrationBuilder.CreateIndex(
                name: "IX_examination_diagnoses_Icd10CodeId",
                table: "examination_diagnoses",
                column: "Icd10CodeId");

            migrationBuilder.CreateIndex(
                name: "IX_examinations_DoctorUserId",
                table: "examinations",
                column: "DoctorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_examinations_PatientId",
                table: "examinations",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_icd10_codes_Code",
                table: "icd10_codes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoice_lines_InvoiceId",
                table: "invoice_lines",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_ExaminationId",
                table: "invoices",
                column: "ExaminationId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_InvoiceNumber",
                table: "invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoices_PatientId",
                table: "invoices",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_medical_interventions_ExaminationId",
                table: "medical_interventions",
                column: "ExaminationId");

            migrationBuilder.CreateIndex(
                name: "IX_patient_allergies_PatientId",
                table: "patient_allergies",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_patient_chronic_conditions_PatientId",
                table: "patient_chronic_conditions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_patient_clinical_notes_ExaminationId",
                table: "patient_clinical_notes",
                column: "ExaminationId");

            migrationBuilder.CreateIndex(
                name: "IX_patient_clinical_notes_PatientId",
                table: "patient_clinical_notes",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_patient_documents_PatientId",
                table: "patient_documents",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_patient_kvkk_consents_PatientId_ConsentType",
                table: "patient_kvkk_consents",
                columns: new[] { "PatientId", "ConsentType" });

            migrationBuilder.CreateIndex(
                name: "IX_patient_ledger_entries_InvoiceId",
                table: "patient_ledger_entries",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_patient_ledger_entries_PatientId",
                table: "patient_ledger_entries",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_patient_ledger_entries_PaymentId",
                table: "patient_ledger_entries",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_patient_medical_histories_PatientId",
                table: "patient_medical_histories",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_installment_plans_InvoiceId",
                table: "payment_installment_plans",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_installment_plans_PatientId",
                table: "payment_installment_plans",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_installments_PaymentId",
                table: "payment_installments",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_installments_PlanId",
                table: "payment_installments",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_FinancialAccountId",
                table: "payments",
                column: "FinancialAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_InstallmentPlanId",
                table: "payments",
                column: "InstallmentPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_InvoiceId",
                table: "payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_PatientId",
                table: "payments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_products_Barcode",
                table: "products",
                column: "Barcode");

            migrationBuilder.CreateIndex(
                name: "IX_products_Sku",
                table: "products",
                column: "Sku");

            migrationBuilder.CreateIndex(
                name: "IX_products_SupplierId",
                table: "products",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_recurring_appointment_templates_PatientId",
                table: "recurring_appointment_templates",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_recurring_appointment_templates_PrimaryResourceId",
                table: "recurring_appointment_templates",
                column: "PrimaryResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_sms_reminder_logs_AppointmentId",
                table: "sms_reminder_logs",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_stock_lots_Barcode",
                table: "stock_lots",
                column: "Barcode");

            migrationBuilder.CreateIndex(
                name: "IX_stock_lots_ExpiryDate",
                table: "stock_lots",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_stock_lots_ProductId",
                table: "stock_lots",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_stock_movements_StockLotId",
                table: "stock_movements",
                column: "StockLotId");

            migrationBuilder.CreateIndex(
                name: "IX_treatment_plan_items_TreatmentPlanId",
                table: "treatment_plan_items",
                column: "TreatmentPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_treatment_plans_ExaminationId",
                table: "treatment_plans",
                column: "ExaminationId");

            migrationBuilder.CreateIndex(
                name: "IX_treatment_plans_PatientId",
                table: "treatment_plans",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_waitlist_entries_PatientId",
                table: "waitlist_entries",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_waitlist_entries_PreferredResourceId",
                table: "waitlist_entries",
                column: "PreferredResourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_patients_AspNetUsers_UserId",
                table: "patients",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_patients_AspNetUsers_UserId",
                table: "patients");

            migrationBuilder.DropTable(
                name: "appointment_resource_links");

            migrationBuilder.DropTable(
                name: "examination_diagnoses");

            migrationBuilder.DropTable(
                name: "invoice_lines");

            migrationBuilder.DropTable(
                name: "medical_interventions");

            migrationBuilder.DropTable(
                name: "patient_allergies");

            migrationBuilder.DropTable(
                name: "patient_chronic_conditions");

            migrationBuilder.DropTable(
                name: "patient_clinical_notes");

            migrationBuilder.DropTable(
                name: "patient_documents");

            migrationBuilder.DropTable(
                name: "patient_kvkk_consents");

            migrationBuilder.DropTable(
                name: "patient_ledger_entries");

            migrationBuilder.DropTable(
                name: "patient_medical_histories");

            migrationBuilder.DropTable(
                name: "payment_installments");

            migrationBuilder.DropTable(
                name: "sms_reminder_logs");

            migrationBuilder.DropTable(
                name: "stock_movements");

            migrationBuilder.DropTable(
                name: "treatment_plan_items");

            migrationBuilder.DropTable(
                name: "waitlist_entries");

            migrationBuilder.DropTable(
                name: "icd10_codes");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "appointments");

            migrationBuilder.DropTable(
                name: "stock_lots");

            migrationBuilder.DropTable(
                name: "treatment_plans");

            migrationBuilder.DropTable(
                name: "financial_accounts");

            migrationBuilder.DropTable(
                name: "payment_installment_plans");

            migrationBuilder.DropTable(
                name: "recurring_appointment_templates");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropTable(
                name: "appointment_resources");

            migrationBuilder.DropTable(
                name: "suppliers");

            migrationBuilder.DropTable(
                name: "examinations");

            migrationBuilder.DropIndex(
                name: "IX_patients_UserId",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "BloodType",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "ClinicalSummary",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "patients");
        }
    }
}
