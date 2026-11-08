using GymManagement.Server.Core.DTOs.Members;

namespace GymManagement.Server.Core.Contracts;

/// <summary>
/// Hợp đồng dịch vụ MemberService Component — Kết nối Authenticated + Database API + SMS Gateway.
/// Bao gồm UC02 và UC09:
///   UC02: Admin quản lý hội viên, vòng đời gói tập (đăng ký, gia hạn, bảo lưu, hủy).
///   UC09: Hội viên xem thông tin cá nhân và lịch sử tập luyện.
/// </summary>
public interface IMemberService
{
    // ─────────────────────────────────────────────────────────────────────────
    // NHÓM 1 — HỒ SƠ HỘI VIÊN (UC02.1, UC02.2, UC09.1)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Tìm kiếm hội viên theo từ khóa (SĐT, Họ tên, Mã hội viên).
    /// UC02.2 — Admin tra cứu trước khi thực hiện thao tác.
    /// </summary>
    Task<IReadOnlyList<MemberSearchResponse>> SearchMembersAsync(
        string? keyword,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy toàn bộ thông tin hội viên kèm gói tập hiện tại.
    /// Ném <see cref="Core.Exceptions.NotFoundException"/> nếu không tìm thấy.
    /// UC09.1.
    /// </summary>
    Task<MemberProfileResponse> GetMemberProfileAsync(
        string userName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Đăng ký hội viên mới cùng gói tập, sinh QrCode và gửi Email chào mừng.
    /// Ném <see cref="Core.Exceptions.ValidationException"/> nếu Phone đã tồn tại.
    /// UC02.1.
    /// </summary>
    Task<MemberProfileResponse> RegisterMemberAsync(
        RegisterMemberRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật thông tin cá nhân hội viên (FullName, Phone, Email).
    /// Ném <see cref="Core.Exceptions.ValidationException"/> nếu dữ liệu không hợp lệ.
    /// UC02.2.
    /// </summary>
    Task<MemberProfileResponse> UpdateMemberAsync(
        UpdateMemberRequest request,
        CancellationToken cancellationToken = default);

    // ─────────────────────────────────────────────────────────────────────────
    // NHÓM 2 — GÓI TẬP (UC02.3, UC02.4, UC02.5)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Lấy gói tập đang hoạt động của hội viên.
    /// Trả về null nếu chưa có gói tập.
    /// UC09.1.
    /// </summary>
    Task<SubscriptionDetailResponse?> GetActiveSubscriptionAsync(
        string userName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gia hạn hoặc đổi gói tập mới cho hội viên.
    /// Tính EndDate mới = NewStartDate + DurationDays của gói mới.
    /// Ném <see cref="Core.Exceptions.BusinessRuleException"/> nếu gói không hợp lệ.
    /// UC02.3.
    /// </summary>
    Task<SubscriptionDetailResponse> RenewSubscriptionAsync(
        RenewSubscriptionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Nâng cấp gói tập lên hạng cao hơn.
    /// Giữ nguyên EndDate, cập nhật sessions theo gói mới.
    /// Ném <see cref="Core.Exceptions.BusinessRuleException"/> nếu gói đích không tồn tại hoặc không hoạt động.
    /// UC02.3.
    /// </summary>
    Task<SubscriptionDetailResponse> UpgradeSubscriptionAsync(
        UpgradeSubscriptionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bảo lưu gói tập, chuyển Status = FROZEN và kéo dài EndDate.
    /// EndDate mới = EndDate cũ + (FreezeEndDate - FreezeStartDate).
    /// Ném <see cref="Core.Exceptions.BusinessRuleException"/> nếu gói không ở trạng thái ACTIVE.
    /// UC02.4.
    /// </summary>
    Task<SubscriptionDetailResponse> FreezeSubscriptionAsync(
        FreezeSubscriptionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Hủy gói tập, chuyển Status = CANCELLED.
    /// Giữ toàn bộ lịch sử; không xóa, không hoàn tiền tự động, không khóa tài khoản.
    /// Lý do hủy bắt buộc phải được cung cấp.
    /// UC02.5.
    /// </summary>
    Task CancelSubscriptionAsync(
        CancelSubscriptionRequest request,
        CancellationToken cancellationToken = default);

    // ─────────────────────────────────────────────────────────────────────────
    // NHÓM 3 — LỊCH SỬ TẬP LUYỆN (UC09.2)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Lấy lịch sử check-in của hội viên, sắp xếp mới nhất trước.
    /// Hỗ trợ lọc theo khoảng ngày.
    /// UC09.2.
    /// </summary>
    Task<IReadOnlyList<WorkoutHistoryResponse>> GetWorkoutHistoryAsync(
        string userName,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        CancellationToken cancellationToken = default);
}
