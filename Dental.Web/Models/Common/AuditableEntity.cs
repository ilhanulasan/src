namespace Dental.Web.Models.Common;

public abstract class AuditableEntity
{
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedByUserId { get; set; }
    public string? UpdatedByUserId { get; set; }
}
