namespace Dental.Web.Models;

/// <summary>Payload stored in <see cref="PatientOdontograph.PayloadJson"/> (bardurt engine format).</summary>
public sealed class OdontographSnapshot
{
    public string Type { get; set; } = "adult";

    public List<OdontographDamageEntry> Damages { get; set; } = [];

    public string? Observations { get; set; }

    public string? Specifications { get; set; }
}

public sealed class OdontographDamageEntry
{
    public int Tooth { get; set; }

    /// <summary>Damage id or surface checkbox state (string in engine export).</summary>
    public string Damage { get; set; } = "";

    public string Surface { get; set; } = "0";

    public string Note { get; set; } = "";
}
