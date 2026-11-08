using GymManagement.Server.Core.DTOs.PT;

namespace GymManagement.Server.Core.Contracts;

/// <summary>
/// Hợp đồng dịch vụ PtService Component — Kết nối Authenticated + Database API + SMS Gateway.
/// Bao gồm UC04, UC08, UC10:
///   UC04: Admin quản lý hồ sơ PT và điều phối lịch dạy.
///   UC08: PT đăng ký lịch rảnh, xem học viên, ghi InBody, xác nhận ca tập.
///   UC10: Hội viên đặt/hủy/đổi lịch tập và đánh giá PT.
/// </summary>
public interface IPtService
{
    // ─────────────────────────────────────────────────────────────────────────
    // NHÓM 1 — QUẢN LÝ HỒ SƠ PT (UC 4.1, UC 4.3)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Lấy danh sách tất cả PT, có thể lọc theo trạng thái hoạt động.
    /// UC 4.2 — Admin tra cứu lịch dạy.
    /// </summary>
    Task<IReadOnlyList<PtProfileDto>> GetTrainersAsync(
        bool? activeOnly = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy chi tiết hồ sơ một PT theo ID.
    /// Ném <see cref="Core.Exceptions.NotFoundException"/> nếu PT không tồn tại.
    /// </summary>
    Task<PtProfileDto> GetTrainerByIdAsync(
        string ptId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tạo mới hoặc cập nhật hồ sơ PT.
    /// Ném <see cref="Core.Exceptions.ValidationException"/> nếu trùng Phone/Email.
    /// UC 4.1 (tạo mới), UC 4.3 (cập nhật).
    /// </summary>
    Task<PtProfileDto> SaveTrainerAsync(
        SavePtProfileRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra PT có thể vô hiệu hóa không (không còn học viên, không còn ca chờ).
    /// UC 4.3 — Cập nhật trạng thái PT.
    /// </summary>
    Task<(bool CanDeactivate, string? BlockReason, int ActiveStudents, int PendingSessions)>
        CheckDeactivationEligibilityAsync(
            string ptId,
            CancellationToken cancellationToken = default);

    /// <summary>
    /// Bật/tắt trạng thái hoạt động của PT.
    /// Ném <see cref="Core.Exceptions.BusinessRuleException"/> nếu vô hiệu hóa khi còn ràng buộc.
    /// UC 4.3.
    /// </summary>
    Task SetTrainerStatusAsync(
        string ptId,
        bool isActive,
        CancellationToken cancellationToken = default);

    // ─────────────────────────────────────────────────────────────────────────
    // NHÓM 2 — PHÂN CÔNG PT (UC 4.4)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Admin phân công PT phụ trách cố định cho hội viên.
    /// Ghi vào <c>MemberSubscription.AssignedPtId</c>.
    /// Ném <see cref="Core.Exceptions.BusinessRuleException"/> nếu PT đã đạt giới hạn học viên.
    /// UC 4.4.
    /// </summary>
    Task AssignPtToMemberAsync(
        AssignPtRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách học viên đang được một PT phụ trách.
    /// Trả MemberSummaryDto để phản ánh đúng domain — học viên không phải PT.
    /// UC 8.2.1.
    /// </summary>
    Task<IReadOnlyList<MemberSummaryDto>> GetPtStudentsAsync(
        string ptId,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    // ─────────────────────────────────────────────────────────────────────────
    // NHÓM 3 — LỊCH RẢNH PT (UC 4.6, UC 8.1.1, UC 8.1.2)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Lấy danh sách lịch rảnh của PT trong một khoảng thời gian.
    /// UC 8.1.2 — PT xem lịch dạy theo tuần; UC 4.2 — Admin tra cứu lịch PT.
    /// </summary>
    Task<IReadOnlyList<PtSlotDto>> GetPtSlotsAsync(
        string ptId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// PT đăng ký hoặc hủy đăng ký lịch rảnh theo tuần.
    /// Slot có <c>is_Booked = true</c> không được hủy — ném <see cref="Core.Exceptions.BusinessRuleException"/>.
    /// UC 8.1.1, UC 4.6.
    /// </summary>
    Task SavePtSlotsAsync(
        SavePtSlotsRequest request,
        CancellationToken cancellationToken = default);

    // ─────────────────────────────────────────────────────────────────────────
    // NHÓM 4 — CA TẬP PT (UC 10.1, UC 10.2, UC 10.4, UC 4.5, UC 8.2.3)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Lấy lịch dạy của PT (hoặc hội viên) trong khoảng thời gian.
    /// UC 4.2, UC 8.1.2.
    /// </summary>
    Task<IReadOnlyList<PtSessionDto>> GetSessionsAsync(
        string? ptId = null,
        string? memberUserName = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Hội viên đặt lịch tập với PT.
    /// Kiểm tra: còn buổi PT, slot trống, không trùng lịch hội viên.
    /// Ném <see cref="Core.Exceptions.BusinessRuleException"/> khi vi phạm bất kỳ điều kiện nào.
    /// UC 10.1.
    /// </summary>
    Task<PtSessionDto> BookSessionAsync(
        BookSessionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Hội viên hủy lịch tập.
    /// Quy tắc phạt: hủy &lt; 2 giờ trước giờ tập = không hoàn buổi; hủy ≥ 2h = hoàn 1 buổi.
    /// UC 10.2.
    /// </summary>
    Task CancelSessionAsync(
        CancelSessionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Hội viên đổi lịch tập sang slot khác của cùng PT phụ trách.
    /// Giải phóng slot cũ, giữ chỗ slot mới trong 1 Transaction.
    /// UC 10.4.
    /// </summary>
    Task<PtSessionDto> RescheduleSessionAsync(
        RescheduleSessionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// PT xác nhận hoàn thành ca tập — trừ 1 buổi <c>RemainingPtSessions</c>.
    /// Chỉ xác nhận được ca đang ở trạng thái Booked và đã đến giờ diễn ra.
    /// UC 8.2.3.
    /// </summary>
    Task<PtSessionDto> CompleteSessionByPtAsync(
        PtCompleteSessionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin xác nhận hoặc điều chỉnh kết quả ca tập (Completed/AbsentMember/AbsentPt/Postponed).
    /// Tự động trừ hoặc hoàn buổi tùy theo kết quả cũ và mới.
    /// UC 4.5.
    /// </summary>
    Task<PtSessionDto> ConfirmSessionResultAsync(
        ConfirmSessionResultRequest request,
        CancellationToken cancellationToken = default);

    // ─────────────────────────────────────────────────────────────────────────
    // NHÓM 5 — INBODY (UC 8.2.2)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Lấy lịch sử chỉ số InBody của hội viên, sắp xếp mới nhất trước.
    /// UC 8.2.1 (xem trong danh sách học viên).
    /// </summary>
    Task<IReadOnlyList<InBodyRecordDto>> GetInBodyHistoryAsync(
        string memberUserName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// PT ghi nhận chỉ số InBody mới cho học viên.
    /// Ném <see cref="Core.Exceptions.ValidationException"/> nếu giá trị âm hoặc thiếu trường bắt buộc.
    /// UC 8.2.2.
    /// </summary>
    Task<InBodyRecordDto> RecordInBodyAsync(
        RecordInBodyRequest request,
        CancellationToken cancellationToken = default);

    // ─────────────────────────────────────────────────────────────────────────
    // NHÓM 6 — ĐÁNH GIÁ PT (UC 10.3)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Hội viên đánh giá sao (1–5) sau ca tập đã hoàn thành.
    /// Mỗi ca chỉ được đánh giá 1 lần.
    /// Ném <see cref="Core.Exceptions.BusinessRuleException"/> nếu ca chưa hoàn thành hoặc đã đánh giá.
    /// UC 10.3.
    /// </summary>
    Task SubmitRatingAsync(
        SubmitRatingRequest request,
        CancellationToken cancellationToken = default);
}
