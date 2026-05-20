using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class Patient : AuditableEntity
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string Surname { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string SocialSecurityNumber { get; set; } = string.Empty;

    [MaxLength(512)]
    public string? Address { get; set; }

    [MaxLength(64)]
    public string? Phone { get; set; }

    [MaxLength(256)]
    public string? Email { get; set; }

    public DateOnly DateOfBirth { get; set; }

    [Required]
    [MaxLength(32)]
    public string Gender { get; set; } = string.Empty;

    public EducationLevel Education { get; set; }

    [MaxLength(16)]
    public string? BloodType { get; set; }

    [MaxLength(128)]
    public string? EmergencyContactName { get; set; }

    [MaxLength(64)]
    public string? EmergencyContactPhone { get; set; }

    [MaxLength(2000)]
    public string? ClinicalSummary { get; set; }

    public string? UserId { get; set; }

    public ApplicationUser? User { get; set; }

    [MaxLength(64)]
    public string? RegistrationInviteToken { get; set; }

    public DateTimeOffset? RegistrationInviteExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<PatientMedicalHistory> MedicalHistories { get; set; } = [];
    public ICollection<PatientClinicalNote> ClinicalNotes { get; set; } = [];
    public ICollection<PatientAllergy> Allergies { get; set; } = [];
    public ICollection<PatientChronicCondition> ChronicConditions { get; set; } = [];
    public ICollection<PatientDocument> Documents { get; set; } = [];
    public ICollection<PatientKvkkConsent> KvkkConsents { get; set; } = [];
}
