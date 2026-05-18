using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class Examination : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public string? DoctorUserId { get; set; }
    public ApplicationUser? Doctor { get; set; }

    public DateTimeOffset ExaminedAt { get; set; } = DateTimeOffset.UtcNow;

    public ExaminationStatus Status { get; set; } = ExaminationStatus.Draft;

    [MaxLength(2000)]
    public string? ChiefComplaint { get; set; }

    [MaxLength(4000)]
    public string? ClinicalFindings { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public ICollection<ExaminationDiagnosis> Diagnoses { get; set; } = [];
    public ICollection<MedicalIntervention> Interventions { get; set; } = [];
    public ICollection<PatientClinicalNote> ClinicalNotes { get; set; } = [];
}
