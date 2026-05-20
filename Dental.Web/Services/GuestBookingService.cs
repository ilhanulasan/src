using System.Security.Cryptography;
using Dental.Web.Data;
using Dental.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Dental.Web.Services;

public class GuestBookingService(
    ApplicationDbContext db,
    IEmailSender emailSender,
    IOptions<EmailOptions> emailOptions,
    ILogger<GuestBookingService> log)
{
    private readonly EmailOptions _email = emailOptions.Value;

    public static string NormalizePhone(string? phone) =>
        string.IsNullOrWhiteSpace(phone)
            ? string.Empty
            : new string(phone.Where(char.IsDigit).ToArray());

    public async Task<Patient> ResolvePatientAsync(GuestBookRequest request, CancellationToken ct)
    {
        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        var normPhone = NormalizePhone(request.Phone);

        Patient? patient = null;
        if (!string.IsNullOrWhiteSpace(email))
        {
            var lower = email.ToLowerInvariant();
            patient = await db.Patients.FirstOrDefaultAsync(
                p => p.Email != null && p.Email.ToLower() == lower, ct);
        }

        if (patient is null && normPhone.Length >= 10)
        {
            var candidates = await db.Patients
                .Where(p => p.Phone != null)
                .ToListAsync(ct);
            patient = candidates.FirstOrDefault(p => NormalizePhone(p.Phone) == normPhone);
        }

        if (patient is not null)
        {
            if (!string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(patient.Email))
            {
                patient.Email = email;
            }

            if (!string.IsNullOrWhiteSpace(request.Phone) && string.IsNullOrWhiteSpace(patient.Phone))
            {
                patient.Phone = request.Phone.Trim();
            }

            patient.UpdatedAt = DateTimeOffset.UtcNow;
            return patient;
        }

        patient = new Patient
        {
            Id = Guid.NewGuid(),
            Name = request.FirstName.Trim(),
            Surname = request.LastName.Trim(),
            Email = email,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            SocialSecurityNumber = $"ONL-{Guid.NewGuid():N}"[..20],
            DateOfBirth = request.DateOfBirth ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
            Gender = request.Gender ?? "Other",
            Education = EducationLevel.Graduate,
            IsActive = true,
        };

        if (string.IsNullOrWhiteSpace(patient.Email))
        {
            throw new InvalidOperationException("Email is required for new patients.");
        }

        EnsureRegistrationInvite(patient);
        db.Patients.Add(patient);
        return patient;
    }

    public static void EnsureRegistrationInvite(Patient patient)
    {
        if (patient.UserId is not null)
        {
            return;
        }

        patient.RegistrationInviteToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        patient.RegistrationInviteExpiresAt = DateTimeOffset.UtcNow.AddDays(7);
    }

    public async Task<GuestBookResult> BookAsync(GuestBookRequest request, CancellationToken ct)
    {
        var startAt = AppointmentScheduling.ToUtc(request.StartAt);
        var endAt = AppointmentScheduling.ToUtc(request.EndAt);

        if (endAt <= startAt)
        {
            throw new ArgumentException("Invalid time range.");
        }

        if (await AppointmentScheduling.HasResourceConflictAsync(
                db, request.ResourceId, null, startAt, endAt, null, ct))
        {
            throw new InvalidOperationException("Selected slot is no longer available.");
        }

        var resource = await db.AppointmentResources.AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.Id == request.ResourceId && r.IsActive && r.ResourceType == AppointmentResourceType.Doctor,
                ct);
        if (resource is null)
        {
            throw new InvalidOperationException("Doctor not found.");
        }

        var patient = await ResolvePatientAsync(request, ct);
        var isNewPatient = db.Entry(patient).State == EntityState.Added;
        var needsRegistration = patient.UserId is null;

        if (!isNewPatient && needsRegistration)
        {
            EnsureRegistrationInvite(patient);
        }

        var confirmationToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var appt = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            PrimaryResourceId = request.ResourceId,
            StartAt = startAt,
            EndAt = endAt,
            Notes = request.Notes,
            Status = AppointmentStatus.Scheduled,
            IsOnlineBooking = true,
            GuestConfirmationToken = confirmationToken,
        };

        db.Appointments.Add(appt);

        if (!string.IsNullOrWhiteSpace(patient.Phone))
        {
            db.SmsReminderLogs.Add(new SmsReminderLog
            {
                Id = Guid.NewGuid(),
                AppointmentId = appt.Id,
                PhoneNumber = patient.Phone,
                Message = $"Online randevu: {startAt.LocalDateTime:dd.MM.yyyy HH:mm}",
                ScheduledFor = startAt.AddHours(-24),
                Status = SmsReminderStatus.Pending,
            });
        }

        await db.SaveChangesAsync(ct);

        if (!string.IsNullOrWhiteSpace(patient.Email))
        {
            try
            {
                var body = GuestBookingEmailBuilder.BuildBody(
                    _email,
                    appt,
                    resource,
                    patient,
                    confirmationToken,
                    needsRegistration ? patient.RegistrationInviteToken : null,
                    request.PreferTurkish);
                await emailSender.SendAsync(
                    patient.Email,
                    GuestBookingEmailBuilder.BuildSubject(request.PreferTurkish),
                    body,
                    ct);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to send guest booking email for appointment {AppointmentId}", appt.Id);
            }
        }

        log.LogInformation(
            "Guest booking {AppointmentId} for patient {PatientId} (newPatient={IsNew})",
            appt.Id,
            patient.Id,
            isNewPatient);

        return new GuestBookResult(appt.Id, patient.Id, isNewPatient, needsRegistration);
    }

    public async Task<bool> ConfirmAsync(Guid appointmentId, string token, CancellationToken ct)
    {
        var appt = await db.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId, ct);
        if (appt is null || appt.GuestConfirmationToken is null)
        {
            return false;
        }

        if (!string.Equals(appt.GuestConfirmationToken, token.Trim(), StringComparison.Ordinal))
        {
            return false;
        }

        if (appt.Status is AppointmentStatus.Cancelled)
        {
            return false;
        }

        appt.Status = AppointmentStatus.Confirmed;
        appt.ConfirmedAt = DateTimeOffset.UtcNow;
        appt.GuestConfirmationToken = null;
        appt.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        log.LogInformation("Guest confirmed appointment {AppointmentId}", appointmentId);
        return true;
    }
}

public record GuestBookRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    Guid ResourceId,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    string? Notes,
    DateOnly? DateOfBirth,
    string? Gender,
    bool PreferTurkish);

public record GuestBookResult(
    Guid AppointmentId,
    Guid PatientId,
    bool IsNewPatient,
    bool NeedsRegistration);
