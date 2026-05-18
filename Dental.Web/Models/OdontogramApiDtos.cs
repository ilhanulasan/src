using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Dental.Web.Models;

public sealed class OdontogramPatientRefDto
{
    public Guid Id { get; set; }
}

/// <summary>GET/PUT odontogram body compatible with OdontoManage-Frontend payloads.</summary>
public sealed class OdontogramDocumentDto
{
    public Guid? Id { get; set; }

    [Required]
    [MaxLength(16)]
    public string Type { get; set; } = "adult";

    /// <summary>Ignored on write — patient comes from route.</summary>
    public OdontogramPatientRefDto? Patient { get; set; }

    public List<OdontogramToothPathologyDto> ToothPathologies { get; set; } = [];

    public List<OdontogramToothTreatmentDto> ToothTreatments { get; set; } = [];

    public List<OdontogramBridgeTreatmentDto> BridgeTreatments { get; set; } = [];

    internal OdontogramSnapshot ToSnapshot()
    {
        var type = NormalizedChartType(Type);
        return new OdontogramSnapshot
        {
            Type = type,
            ToothPathologies = ToothPathologies.Select(p => new SnapshotToothPathology
            {
                Tooth = new SnapshotToothRef { ToothNumber = p.Tooth.ToothNumber },
                Pathology = new SnapshotPathologyRef { Id = p.Pathology.Id },
                ToothFace = p.ToothFace
            }).ToList(),
            ToothTreatments = ToothTreatments.Select(t => new SnapshotToothTreatment
            {
                Treatment = new SnapshotTreatmentRef { Id = t.Treatment.Id },
                ToothNumber = t.ToothNumber,
                ToothFace = t.ToothFace,
                Status = NormalizedTreatmentStatus(t.Status)
            }).ToList(),
            BridgeTreatments = BridgeTreatments.Select(b => new SnapshotBridgeTreatment
            {
                Treatment = new SnapshotTreatmentRef { Id = b.Treatment.Id },
                StartTooth = b.StartTooth,
                EndTooth = b.EndTooth,
                Status = NormalizedTreatmentStatus(b.Status)
            }).ToList()
        };
    }

    internal static string NormalizedChartType(string? raw)
    {
        if (string.Equals(raw, "child", StringComparison.OrdinalIgnoreCase))
            return "child";
        return "adult";
    }

    internal static string NormalizedTreatmentStatus(string? raw)
    {
        return string.Equals(raw, "done", StringComparison.OrdinalIgnoreCase) ? "done" : "pending";
    }

    internal static OdontogramDocumentDto FromEntity(PatientOdontogram entity)
    {
        var opts = SerializationOptions.Json;
        var snap = JsonSerializer.Deserialize<OdontogramSnapshot>(entity.PayloadJson, opts)
                   ?? new OdontogramSnapshot { Type = entity.Type };

        return new OdontogramDocumentDto
        {
            Id = entity.Id,
            Type = OdontogramDocumentDto.NormalizedChartType(snap.Type),
            Patient = new OdontogramPatientRefDto { Id = entity.PatientId },
            ToothPathologies = snap.ToothPathologies.Select(p => new OdontogramToothPathologyDto
            {
                Tooth = new OdontogramToothRefDto { Id = 0, ToothNumber = p.Tooth.ToothNumber },
                Pathology = new OdontogramPathologyRefDto { Id = p.Pathology.Id },
                ToothFace = p.ToothFace
            }).ToList(),
            ToothTreatments = snap.ToothTreatments.Select(t => new OdontogramToothTreatmentDto
            {
                Treatment = new OdontogramTreatmentRefDto { Id = t.Treatment.Id },
                ToothNumber = t.ToothNumber,
                ToothFace = t.ToothFace,
                Status = NormalizedTreatmentStatus(t.Status)
            }).ToList(),
            BridgeTreatments = snap.BridgeTreatments.Select(b => new OdontogramBridgeTreatmentDto
            {
                Treatment = new OdontogramTreatmentRefDto { Id = b.Treatment.Id },
                StartTooth = b.StartTooth,
                EndTooth = b.EndTooth,
                Status = NormalizedTreatmentStatus(b.Status)
            }).ToList()
        };
    }
}

internal static class SerializationOptions
{
    internal static JsonSerializerOptions Json { get; } = new(JsonSerializerDefaults.Web);
}

public sealed class OdontogramToothRefDto
{
    public int Id { get; set; }

    public int ToothNumber { get; set; }
}

public sealed class OdontogramPathologyRefDto
{
    public int Id { get; set; }
}

public sealed class OdontogramToothPathologyDto
{
    [Required]
    public OdontogramToothRefDto Tooth { get; set; } = null!;

    [Required]
    public OdontogramPathologyRefDto Pathology { get; set; } = null!;

    public int ToothFace { get; set; }
}

public sealed class OdontogramTreatmentRefDto
{
    public int Id { get; set; }
}

public sealed class OdontogramToothTreatmentDto
{
    [Required]
    public OdontogramTreatmentRefDto Treatment { get; set; } = null!;
    public int ToothNumber { get; set; }
    public int ToothFace { get; set; }
    public string Status { get; set; } = "pending";
}

public sealed class OdontogramBridgeTreatmentDto
{
    [Required]
    public OdontogramTreatmentRefDto Treatment { get; set; } = null!;
    public int StartTooth { get; set; }
    public int EndTooth { get; set; }
    public string Status { get; set; } = "pending";
}
