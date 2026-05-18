using System.ComponentModel.DataAnnotations;
using Dental.Web.Models.Common;

namespace Dental.Web.Models;

public class PatientDocument : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(512)]
    public string StoragePath { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? ContentType { get; set; }

    public long FileSizeBytes { get; set; }

    public PatientDocumentCategory Category { get; set; } = PatientDocumentCategory.Other;

    [MaxLength(512)]
    public string? Description { get; set; }

    public bool IsEncrypted { get; set; } = true;
}
