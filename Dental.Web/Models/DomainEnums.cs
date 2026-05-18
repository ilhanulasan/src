namespace Dental.Web.Models;

public enum AppointmentStatus
{
    Scheduled,
    Confirmed,
    Completed,
    Cancelled,
    NoShow,
    Rescheduled,
}

public enum AppointmentResourceType
{
    Doctor,
    Room,
    Device,
}

public enum ExaminationStatus
{
    Draft,
    InProgress,
    Completed,
    Cancelled,
}

public enum TreatmentPlanStatus
{
    Draft,
    Active,
    Completed,
    Cancelled,
}

public enum TreatmentPlanItemStatus
{
    Planned,
    InProgress,
    Completed,
    Cancelled,
}

public enum InvoiceStatus
{
    Draft,
    Issued,
    PartiallyPaid,
    Paid,
    Cancelled,
    Overdue,
}

public enum PaymentMethod
{
    Cash,
    Card,
    BankTransfer,
    Other,
}

public enum FinancialAccountType
{
    Cash,
    Bank,
}

public enum LedgerEntryType
{
    Charge,
    Payment,
    Adjustment,
    Refund,
}

public enum StockMovementType
{
    Inbound,
    Outbound,
    Adjustment,
    Expired,
    Return,
}

public enum PatientDocumentCategory
{
    Identity,
    Medical,
    Consent,
    Imaging,
    Other,
}

public enum KvkkConsentType
{
    DataProcessing,
    Marketing,
    ThirdPartySharing,
    OnlineServices,
}

public enum RecurrenceFrequency
{
    Daily,
    Weekly,
    BiWeekly,
    Monthly,
}

public enum SmsReminderStatus
{
    Pending,
    Sent,
    Failed,
    Cancelled,
}

public enum WaitlistStatus
{
    Active,
    Scheduled,
    Cancelled,
    Expired,
}
