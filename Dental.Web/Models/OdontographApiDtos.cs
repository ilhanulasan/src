using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Dental.Web.Models;

public sealed class OdontographDocumentDto
{
    public Guid? Id { get; set; }

    [Required]
    [MaxLength(16)]
    public string Type { get; set; } = "adult";

    public List<OdontographDamageDto> Damages { get; set; } = [];

    public string? Observations { get; set; }

    public string? Specifications { get; set; }

    internal OdontographSnapshot ToSnapshot()
    {
        return new OdontographSnapshot
        {
            Type = OdontographDocumentDto.NormalizedChartType(Type),
            Damages = Damages.Select(d => new OdontographDamageEntry
            {
                Tooth = d.Tooth,
                Damage = d.Damage ?? "",
                Surface = d.Surface ?? "0",
                Note = d.Note ?? "",
            }).ToList(),
            Observations = Observations,
            Specifications = Specifications,
        };
    }

    internal static string NormalizedChartType(string? raw)
    {
        if (string.Equals(raw, "child", StringComparison.OrdinalIgnoreCase))
            return "child";
        return "adult";
    }

    internal static OdontographDocumentDto FromEntity(PatientOdontograph entity)
    {
        var opts = SerializationOptions.Json;
        var snap = JsonSerializer.Deserialize<OdontographSnapshot>(entity.PayloadJson, opts)
                   ?? new OdontographSnapshot { Type = entity.Type };

        return new OdontographDocumentDto
        {
            Id = entity.Id,
            Type = NormalizedChartType(snap.Type),
            Damages = snap.Damages.Select(d => new OdontographDamageDto
            {
                Tooth = d.Tooth,
                Damage = d.Damage,
                Surface = d.Surface,
                Note = d.Note,
            }).ToList(),
            Observations = snap.Observations,
            Specifications = snap.Specifications,
        };
    }
}

public sealed class OdontographDamageDto
{
    public int Tooth { get; set; }

    public string Damage { get; set; } = "";

    public string Surface { get; set; } = "0";

    public string Note { get; set; } = "";
}
