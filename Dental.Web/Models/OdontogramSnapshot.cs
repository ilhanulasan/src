namespace Dental.Web.Models;

/// <summary>Dental chart payload stored inside <see cref="PatientOdontogram.PayloadJson"/>.</summary>
public sealed class OdontogramSnapshot
{
    public string Type { get; set; } = "adult";

    public List<SnapshotToothPathology> ToothPathologies { get; set; } = [];

    public List<SnapshotToothTreatment> ToothTreatments { get; set; } = [];

    public List<SnapshotBridgeTreatment> BridgeTreatments { get; set; } = [];
}

public sealed class SnapshotToothRef
{
    public int ToothNumber { get; set; }
}

public sealed class SnapshotPathologyRef
{
    public int Id { get; set; }
}

public sealed class SnapshotToothPathology
{
    public SnapshotToothRef Tooth { get; set; } = null!;

    public SnapshotPathologyRef Pathology { get; set; } = null!;

    public int ToothFace { get; set; }
}

public sealed class SnapshotTreatmentRef
{
    public int Id { get; set; }
}

public sealed class SnapshotToothTreatment
{
    public SnapshotTreatmentRef Treatment { get; set; } = null!;

    public int ToothNumber { get; set; }

    public int ToothFace { get; set; }

    public string Status { get; set; } = "pending";
}

public sealed class SnapshotBridgeTreatment
{
    public SnapshotTreatmentRef Treatment { get; set; } = null!;

    public int StartTooth { get; set; }

    public int EndTooth { get; set; }

    public string Status { get; set; } = "pending";
}
