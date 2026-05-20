using System.ComponentModel.DataAnnotations;

namespace Dental.Web.Models;

/// <summary>
/// Canvas-based odontograph (bardurt/odontograma) persisted per patient as JSON.
/// Separate from <see cref="PatientOdontogram"/> (face-based OdontoManage chart).
/// </summary>
public class PatientOdontograph
{
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }

    public Patient Patient { get; set; } = null!;

    /// <summary>adult | child — last active view in the canvas editor.</summary>
    [Required]
    [MaxLength(16)]
    public string Type { get; set; } = "adult";

    /// <summary>Serialized <see cref="OdontographSnapshot"/>.</summary>
    [Required]
    public string PayloadJson { get; set; } = "{}";
}
