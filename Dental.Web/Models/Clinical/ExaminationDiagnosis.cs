namespace Dental.Web.Models;

public class ExaminationDiagnosis
{
    public Guid Id { get; set; }
    public Guid ExaminationId { get; set; }
    public Examination Examination { get; set; } = null!;

    public Guid Icd10CodeId { get; set; }
    public Icd10Code Icd10Code { get; set; } = null!;

    public bool IsPrimary { get; set; }

    public string? Notes { get; set; }
}
