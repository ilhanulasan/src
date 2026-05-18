using System.ComponentModel.DataAnnotations;

namespace Dental.Web.Models;

/// <summary>
/// One persisted odontogram per patient (diagnosis snapshot) as JSON payloads.
/// </summary>
public class PatientOdontogram
{
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }

    public Patient Patient { get; set; } = null!;

    /// <summary>adult | child</summary>
    [Required]
    [MaxLength(16)]
    public string Type { get; set; } = "adult";

    /// <summary>Serialized <see cref="OdontogramSnapshot"/>.</summary>
    [Required]
    public string PayloadJson { get; set; } = "{}";
}
