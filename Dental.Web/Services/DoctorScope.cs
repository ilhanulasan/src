using Dental.Web.Data;
using Dental.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Dental.Web.Services;

public static class DoctorScope
{
    public static async Task<IReadOnlyList<Guid>> GetResourceIdsAsync(
        ApplicationDbContext db,
        string userId,
        CancellationToken ct) =>
        await db.AppointmentResources.AsNoTracking()
            .Where(r => r.UserId == userId)
            .Select(r => r.Id)
            .ToListAsync(ct);

    public static async Task<IQueryable<Patient>> ApplyPatientFilterAsync(
        IQueryable<Patient> query,
        ApplicationDbContext db,
        string userId,
        CancellationToken ct)
    {
        var resourceIds = await GetResourceIdsAsync(db, userId, ct);
        if (resourceIds.Count == 0)
        {
            return query.Where(_ => false);
        }

        var patientIds = db.Appointments.AsNoTracking()
            .Where(a =>
                resourceIds.Contains(a.PrimaryResourceId) ||
                a.AdditionalResources.Any(l => resourceIds.Contains(l.ResourceId)))
            .Select(a => a.PatientId)
            .Distinct();

        return query.Where(p => patientIds.Contains(p.Id));
    }

    public static async Task<IQueryable<Appointment>> ApplyAppointmentFilterAsync(
        IQueryable<Appointment> query,
        ApplicationDbContext db,
        string userId,
        CancellationToken ct)
    {
        var resourceIds = await GetResourceIdsAsync(db, userId, ct);
        if (resourceIds.Count == 0)
        {
            return query.Where(_ => false);
        }

        return query.Where(a =>
            resourceIds.Contains(a.PrimaryResourceId) ||
            a.AdditionalResources.Any(l => resourceIds.Contains(l.ResourceId)));
    }

    public static async Task<IQueryable<Examination>> ApplyExaminationFilterAsync(
        IQueryable<Examination> query,
        ApplicationDbContext db,
        string userId,
        CancellationToken ct)
    {
        var resourceIds = await GetResourceIdsAsync(db, userId, ct);
        if (resourceIds.Count == 0)
        {
            return query.Where(e => e.DoctorUserId == userId);
        }

        var patientIds = db.Appointments.AsNoTracking()
            .Where(a =>
                resourceIds.Contains(a.PrimaryResourceId) ||
                a.AdditionalResources.Any(l => resourceIds.Contains(l.ResourceId)))
            .Select(a => a.PatientId)
            .Distinct();

        return query.Where(e => e.DoctorUserId == userId || patientIds.Contains(e.PatientId));
    }
}
