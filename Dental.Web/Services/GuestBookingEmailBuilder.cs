using Dental.Web.Models;

namespace Dental.Web.Services;

public static class GuestBookingEmailBuilder
{
    public static string BuildSubject(bool isTurkish) =>
        isTurkish ? "Randevu talebiniz — onay bekleniyor" : "Your appointment request — confirmation required";

    public static string BuildBody(
        EmailOptions options,
        Appointment appointment,
        AppointmentResource resource,
        Patient patient,
        string confirmationToken,
        string? registrationInviteToken,
        bool isTurkish)
    {
        var baseUrl = options.FrontendBaseUrl.TrimEnd('/');
        var confirmUrl =
            $"{baseUrl}/confirm-appointment?appointmentId={appointment.Id}&token={Uri.EscapeDataString(confirmationToken)}";
        var registerUrl = registrationInviteToken is not null
            ? $"{baseUrl}/complete-account?token={Uri.EscapeDataString(registrationInviteToken)}"
            : null;

        var when = appointment.StartAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        var doctor = resource.Name;
        var patientName = $"{patient.Name} {patient.Surname}".Trim();

        if (isTurkish)
        {
            return $"""
                <p>Merhaba {patientName},</p>
                <p>Randevu talebiniz alındı. Lütfen aşağıdaki bilgileri kontrol edip randevunuzu onaylayın.</p>
                <ul>
                  <li><strong>Doktor:</strong> {doctor}</li>
                  <li><strong>Tarih ve saat:</strong> {when}</li>
                  <li><strong>Durum:</strong> Onay bekliyor</li>
                </ul>
                <p><a href="{confirmUrl}">Randevumu onayla</a></p>
                {(registerUrl is not null
                    ? $"""
                       <p>Hesabınız henüz oluşturulmadı. Randevularınızı yönetmek ve şifrenizi belirlemek için kayıt işlemini tamamlayın:</p>
                       <p><a href="{registerUrl}">Kayıt işlemini tamamla ve şifre oluştur</a></p>
                       """
                    : string.Empty)}
                <p>Bu e-postayı siz talep etmediyseniz lütfen kliniğimizle iletişime geçin.</p>
                """;
        }

        return $"""
            <p>Hello {patientName},</p>
            <p>We received your appointment request. Please review the details below and confirm your visit.</p>
            <ul>
              <li><strong>Doctor:</strong> {doctor}</li>
              <li><strong>Date and time:</strong> {when}</li>
              <li><strong>Status:</strong> Awaiting confirmation</li>
            </ul>
            <p><a href="{confirmUrl}">Confirm my appointment</a></p>
            {(registerUrl is not null
                ? $"""
                   <p>You do not have an online account yet. Complete registration to manage appointments and set your password:</p>
                   <p><a href="{registerUrl}">Complete registration and set password</a></p>
                   """
                : string.Empty)}
            <p>If you did not request this email, please contact our clinic.</p>
            """;
    }
}
