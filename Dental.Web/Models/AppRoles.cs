namespace Dental.Web.Models;

public static class AppRoles
{
    public const string Admin = nameof(Admin);
    public const string Patient = nameof(Patient);
    public const string Doctor = nameof(Doctor);
    public const string Nurse = nameof(Nurse);
    public const string Technician = nameof(Technician);
    public const string Finance = nameof(Finance);

    public static readonly string[] All =
    [
        Admin,
        Patient,
        Doctor,
        Nurse,
        Technician,
        Finance,
    ];
}
