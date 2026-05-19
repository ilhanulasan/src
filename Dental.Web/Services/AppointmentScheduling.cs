using Dental.Web.Data;
using Dental.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Dental.Web.Services;

public static class AppointmentScheduling
{
    public static readonly AppointmentStatus[] BlockingStatuses =
    [
        AppointmentStatus.Scheduled,
        AppointmentStatus.Confirmed,
        AppointmentStatus.Rescheduled,
    ];

    public static bool TimesOverlap(DateTimeOffset start, DateTimeOffset end, DateTimeOffset otherStart, DateTimeOffset otherEnd) =>
        otherStart < end && otherEnd > start;

    public static DateTimeOffset ToUtc(DateTimeOffset value) => value.ToUniversalTime();

    public static (DateTimeOffset DayStart, DateTimeOffset DayEnd) GetWorkingDay(DateOnly date)
    {
        var dayStartLocal = DateTime.SpecifyKind(date.ToDateTime(new TimeOnly(9, 0)), DateTimeKind.Unspecified);
        var dayEndLocal = DateTime.SpecifyKind(date.ToDateTime(new TimeOnly(18, 0)), DateTimeKind.Unspecified);
        var dayStart = ToUtc(TimeZoneInfo.ConvertTimeToUtc(dayStartLocal, TimeZoneInfo.Local));
        var dayEnd = ToUtc(TimeZoneInfo.ConvertTimeToUtc(dayEndLocal, TimeZoneInfo.Local));
        return (dayStart, dayEnd);
    }

    public static async Task<bool> HasResourceConflictAsync(
        ApplicationDbContext db,
        Guid primaryResourceId,
        IEnumerable<Guid>? additionalResourceIds,
        DateTimeOffset start,
        DateTimeOffset end,
        Guid? excludeAppointmentId,
        CancellationToken ct)
    {
        start = ToUtc(start);
        end = ToUtc(end);

        if (end <= start)
        {
            return true;
        }

        var resourceIds = new List<Guid> { primaryResourceId };
        if (additionalResourceIds != null)
        {
            resourceIds.AddRange(additionalResourceIds);
        }

        var q = db.Appointments.Where(a =>
            BlockingStatuses.Contains(a.Status) &&
            a.StartAt < end &&
            a.EndAt > start &&
            (resourceIds.Contains(a.PrimaryResourceId) ||
             a.AdditionalResources.Any(l => resourceIds.Contains(l.ResourceId))));

        if (excludeAppointmentId.HasValue)
        {
            q = q.Where(a => a.Id != excludeAppointmentId);
        }

        return await q.AnyAsync(ct);
    }

    public static async Task<List<TimeSlotResult>> GetAvailableSlotsAsync(
        ApplicationDbContext db,
        Guid resourceId,
        DateOnly date,
        CancellationToken ct)
    {
        var resource = await db.AppointmentResources.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == resourceId && r.IsActive, ct);
        if (resource is null)
        {
            return [];
        }

        var (dayStart, dayEnd) = GetWorkingDay(date);
        var duration = TimeSpan.FromMinutes(Math.Max(15, resource.DefaultDurationMinutes));

        var booked = await db.Appointments.AsNoTracking()
            .Where(a =>
                a.PrimaryResourceId == resourceId &&
                BlockingStatuses.Contains(a.Status) &&
                a.StartAt < dayEnd &&
                a.EndAt > dayStart)
            .Select(a => new { a.StartAt, a.EndAt })
            .ToListAsync(ct);

        var slots = new List<TimeSlotResult>();
        for (var t = dayStart; t.Add(duration) <= dayEnd; t = t.Add(duration))
        {
            var slotEnd = t.Add(duration);
            if (!booked.Any(b => TimesOverlap(t, slotEnd, b.StartAt, b.EndAt)))
            {
                slots.Add(new TimeSlotResult(t, slotEnd));
            }
        }

        return slots;
    }
}

public record TimeSlotResult(DateTimeOffset StartAt, DateTimeOffset EndAt);
