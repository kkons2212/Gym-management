using GymManagement.Core.Enums;

namespace GymManagement.Core.DTOs;

public record MemberDto(
    string MemberCode,
    string FullName,
    string Phone,
    string Email,
    string CitizenId,
    DateOnly DateOfBirth,
    string Address,
    string? AvatarUrl,
    MemberStatus Status,
    string? ActivePackageName,
    DateOnly? ExpirationDate,
    int RemainingPtSessions
);

public record MembershipCardDto(
    string CardCode,
    string MemberCode,
    string MemberName,
    string PackageName,
    DateOnly StartDate,
    DateOnly? EndDate,
    int TotalSessions,
    int RemainingSessions,
    CardStatus Status,
    string QrCodeString
);

public record PackageDto(
    string PackageId,
    string Name,
    decimal Price,
    string DurationDisplay,
    int SessionCount,
    int MaxFreezeDays,
    string Benefits,
    int ActiveMembersCount,
    bool IsActive
);

public record CheckInRecordDto(
    string RecordId,
    string CardCode,
    string MemberCode,
    string MemberName,
    string PackageName,
    DateTime CheckInTime,
    bool IsSuccess,
    string? Reason,
    string? Shift
);

public record PtProfileDto(
    string PtId,
    string FullName,
    string Phone,
    string Email,
    string Specialty,
    int ExperienceYears,
    decimal RatingAverage,
    int ReviewCount,
    string? AvatarUrl,
    bool IsActive
);

public record PtSessionDto(
    string BookingId,
    string PtId,
    string PtName,
    string MemberCode,
    string MemberName,
    DateTime StartTime,
    DateTime EndTime,
    BookingStatus Status,
    string? Note,
    int? Rating,
    string? FeedbackComment
);

/// <summary>
/// Một khung giờ trong lịch rảnh của PT (UC 8.1.1).
/// </summary>
public record PtSlotDto(
    string SlotId,
    DateOnly Date,
    string StartTime,
    string EndTime,
    PtSlotStatus Status
);

public record InBodyRecordDto(
    string RecordId,
    string MemberCode,
    DateTime MeasuredDate,
    decimal WeightKg,
    decimal BodyFatPercent,
    decimal MuscleMassKg,
    string? Note,
    string MeasuredByPtName
);

public record InvoiceDto(
    string InvoiceCode,
    string MemberCode,
    string MemberName,
    string CashierName,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal FinalAmount,
    PaymentMethod Method,
    InvoiceStatus Status,
    DateTime CreatedAt,
    string ItemsSummary
);

public record UserAccountDto(
    string UserId,
    string Username,
    string FullName,
    string Email,
    string Phone,
    UserRole Role,
    bool IsActive
);

public record SystemConfigDto(
    string OpeningTime,
    string ClosingTime,
    int MinCancellationHours,
    int MaxFreezeDaysPerYear,
    int DuplicateScanCooldownSeconds
);
