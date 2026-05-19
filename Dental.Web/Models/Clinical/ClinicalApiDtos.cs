namespace Dental.Web.Models;

public record CreateExaminationRequest(
    Guid PatientId,
    string? DoctorUserId,
    DateTimeOffset? ExaminedAt,
    ExaminationStatus Status,
    string? ChiefComplaint,
    string? ClinicalFindings,
    string? Notes);

public record AddExaminationDiagnosisRequest(
    Guid Icd10CodeId,
    bool IsPrimary,
    string? Notes);

public record AppointmentSummaryDto(
    Guid Id,
    Guid PatientId,
    Guid PrimaryResourceId,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    AppointmentStatus Status,
    string? Notes,
    bool IsOnlineBooking,
    string? PrimaryResourceName);
