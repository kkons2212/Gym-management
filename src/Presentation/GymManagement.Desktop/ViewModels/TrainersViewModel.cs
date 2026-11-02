using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Core.Contracts;
using GymManagement.Core.DTOs;

namespace GymManagement.Desktop.ViewModels;

public partial class TrainersViewModel : ObservableObject
{
    private readonly IPtService _ptService;

    [ObservableProperty]
    private string _searchKeyword = string.Empty;

    [ObservableProperty]
    private string _selectedSpecialtyFilter = "Tất cả";

    [ObservableProperty]
    private string _selectedStatusFilter = "Tất cả";

    [ObservableProperty]
    private PtProfileDto? _selectedTrainer;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // --- Statistics ---
    [ObservableProperty]
    private int _totalTrainersCount;

    [ObservableProperty]
    private int _activeTrainersCount;

    [ObservableProperty]
    private int _inactiveTrainersCount;

    [ObservableProperty]
    private decimal _totalSalaryFund;

    // --- Form Modal State (Thêm mới / Chỉnh sửa hồ sơ PT - UC04.1) ---
    [ObservableProperty]
    private bool _isFormOpen;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private string _formTitle = "Thêm Huấn luyện viên mới";

    [ObservableProperty]
    private string _formPtId = string.Empty;

    [ObservableProperty]
    private string _formFullName = string.Empty;

    [ObservableProperty]
    private DateTimeOffset _formDateOfBirth = new(1995, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [ObservableProperty]
    private string _formGender = "Nam";

    [ObservableProperty]
    private string _formPhone = string.Empty;

    [ObservableProperty]
    private string _formEmail = string.Empty;

    [ObservableProperty]
    private string _formSpecialty = "Gym";

    [ObservableProperty]
    private int _formExperienceYears = 3;

    [ObservableProperty]
    private decimal _formBaseSalary = 15000000m;

    [ObservableProperty]
    private bool? _formIsActive = true;

    // Phân quyền chỉnh sửa (Acceptance Criteria #2)
    // Admin: Toàn quyền cập nhật Chuyên môn & Mức lương
    // PT tự đăng nhập: Chỉ sửa SĐT, Email, Avatar; Mức lương & Chuyên môn bị KHÓA
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEditRestrictedFields))]
    [NotifyPropertyChangedFor(nameof(RolePermissionDescription))]
    private bool _isAdminRole = true;

    public bool CanEditRestrictedFields => IsAdminRole;

    public string RolePermissionDescription => IsAdminRole
        ? "Đang đóng vai: Quản trị viên (Admin) – Toàn quyền thêm, sửa Mức lương, Chuyên môn và Trạng thái."
        : "Đang đóng vai: Huấn luyện viên (PT tự sửa) – Chỉ được sửa SĐT, Email, Ảnh đại diện; Mức lương & Chuyên môn bị KHÓA.";

    // --- Warning Dialog State (Ràng buộc khi xóa / vô hiệu hoá PT - Acceptance Criteria #3) ---
    [ObservableProperty]
    private bool _isDeactivateWarningOpen;

    [ObservableProperty]
    private PtProfileDto? _targetPtToDeactivate;

    [ObservableProperty]
    private string _deactivateWarningTitle = string.Empty;

    [ObservableProperty]
    private string _deactivateWarningMessage = string.Empty;

    [ObservableProperty]
    private int _deactivateActiveStudentsCount;

    [ObservableProperty]
    private int _deactivatePendingSessionsCount;

    [ObservableProperty]
    private bool _canDeactivateTargetPt;

    public ObservableCollection<PtProfileDto> TrainersList { get; } = [];
    public ObservableCollection<string> SpecialtyOptions { get; } = ["Gym", "Yoga", "Boxing", "Cardio & Thể lực", "Pilates"];
    public ObservableCollection<string> GenderOptions { get; } = ["Nam", "Nữ", "Khác"];
    public ObservableCollection<string> SpecialtyFilterOptions { get; } = ["Tất cả", "Gym", "Yoga", "Boxing"];
    public ObservableCollection<string> StatusFilterOptions { get; } = ["Tất cả", "Đang hoạt động (ACTIVE)", "Đã vô hiệu hóa (INACTIVE)"];

    public TrainersViewModel(IPtService ptService)
    {
        _ptService = ptService;
        _ = LoadTrainersAsync();
    }

    [RelayCommand]
    public async Task LoadTrainersAsync()
    {
        IsLoading = true;
        try
        {
            var rawList = await _ptService.GetTrainersAsync();

            // Filter by search keyword
            var filtered = rawList.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                var q = SearchKeyword.Trim();
                filtered = filtered.Where(t =>
                    t.FullName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    t.PtId.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    t.Phone.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    t.Email.Contains(q, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by Specialty
            if (!string.IsNullOrWhiteSpace(SelectedSpecialtyFilter) && SelectedSpecialtyFilter != "Tất cả")
            {
                filtered = filtered.Where(t => t.Specialty.Contains(SelectedSpecialtyFilter, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by Status
            if (SelectedStatusFilter == "Đang hoạt động (ACTIVE)")
            {
                filtered = filtered.Where(t => t.IsActive);
            }
            else if (SelectedStatusFilter == "Đã vô hiệu hóa (INACTIVE)")
            {
                filtered = filtered.Where(t => !t.IsActive);
            }

            TrainersList.Clear();
            foreach (var trainer in filtered)
            {
                TrainersList.Add(trainer);
            }

            // Compute statistics
            TotalTrainersCount = rawList.Count;
            ActiveTrainersCount = rawList.Count(t => t.IsActive);
            InactiveTrainersCount = rawList.Count(t => !t.IsActive);
            TotalSalaryFund = rawList.Where(t => t.IsActive).Sum(t => t.BaseSalary);

            StatusMessage = $"Đang hiển thị {TrainersList.Count} / {rawList.Count} huấn luyện viên.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task SearchAsync()
    {
        await LoadTrainersAsync();
    }

    [RelayCommand]
    public void OpenAddTrainerDialog()
    {
        IsEditMode = false;
        FormTitle = "Thêm mới Huấn luyện viên (PT)";
        FormPtId = $"PT-{TrainersList.Count + 1:D2}";
        FormFullName = string.Empty;
        FormDateOfBirth = new DateTimeOffset(1996, 6, 15, 0, 0, 0, TimeSpan.Zero);
        FormGender = "Nam";
        FormPhone = string.Empty;
        FormEmail = string.Empty;
        FormSpecialty = "Gym";
        FormExperienceYears = 3;
        FormBaseSalary = 16000000m;
        FormIsActive = true;

        IsFormOpen = true;
    }

    [RelayCommand]
    public void OpenEditTrainerDialog(PtProfileDto trainer)
    {
        SelectedTrainer = trainer;
        IsEditMode = true;
        FormTitle = $"Chỉnh sửa Hồ sơ PT: {trainer.FullName} ({trainer.PtId})";
        FormPtId = trainer.PtId;
        FormFullName = trainer.FullName;
        FormDateOfBirth = trainer.DateOfBirth.HasValue
            ? new DateTimeOffset(trainer.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue))
            : new DateTimeOffset(1995, 1, 1, 0, 0, 0, TimeSpan.Zero);
        FormGender = trainer.Gender ?? "Nam";
        FormPhone = trainer.Phone;
        FormEmail = trainer.Email;
        FormSpecialty = trainer.Specialty;
        FormExperienceYears = trainer.ExperienceYears;
        FormBaseSalary = trainer.BaseSalary;
        FormIsActive = trainer.IsActive;

        IsFormOpen = true;
    }

    [RelayCommand]
    public void CloseFormDialog()
    {
        IsFormOpen = false;
    }

    [RelayCommand]
    public void ToggleRoleSimulation()
    {
        IsAdminRole = !IsAdminRole;
        StatusMessage = IsAdminRole
            ? "Đã chuyển sang góc nhìn Admin (Toàn quyền chỉnh sửa Mức lương & Chuyên môn)."
            : "Đã chuyển sang góc nhìn PT tự sửa (Mức lương & Chuyên môn bị khóa).";
    }

    [RelayCommand]
    public async Task SaveTrainerAsync()
    {
        if (string.IsNullOrWhiteSpace(FormFullName))
        {
            StatusMessage = "Vui lòng nhập Họ và tên Huấn luyện viên.";
            return;
        }

        if (string.IsNullOrWhiteSpace(FormPhone))
        {
            StatusMessage = "Vui lòng nhập Số điện thoại liên hệ.";
            return;
        }

        var dob = DateOnly.FromDateTime(FormDateOfBirth.DateTime);
        var updatedTrainer = new PtProfileDto(
            FormPtId,
            FormFullName.Trim(),
            FormPhone.Trim(),
            FormEmail.Trim(),
            FormSpecialty,
            FormExperienceYears,
            SelectedTrainer?.RatingAverage ?? 5.0m,
            SelectedTrainer?.ReviewCount ?? 0,
            SelectedTrainer?.AvatarUrl,
            FormIsActive ?? true,
            dob,
            FormGender,
            FormBaseSalary,
            SelectedTrainer?.ActiveStudentsCount ?? 0,
            SelectedTrainer?.PendingSessionsCount ?? 0
        );

        await _ptService.SaveTrainerAsync(updatedTrainer);
        IsFormOpen = false;
        await LoadTrainersAsync();

        StatusMessage = IsEditMode
            ? $"Đã cập nhật hồ sơ HLV {FormFullName} thành công."
            : $"Đã thêm mới HLV {FormFullName} vào đội ngũ.";
    }

    [RelayCommand]
    public async Task RequestDeactivateTrainerAsync(PtProfileDto trainer)
    {
        TargetPtToDeactivate = trainer;

        if (!trainer.IsActive)
        {
            // Nếu đã Inactive -> Kích hoạt lại trực tiếp
            await _ptService.SetTrainerStatusAsync(trainer.PtId, true);
            await LoadTrainersAsync();
            StatusMessage = $"Đã kích hoạt lại tài khoản HLV {trainer.FullName} (ACTIVE).";
            return;
        }

        // Kiểm tra ràng buộc theo Acceptance Criteria #3:
        // "Nếu PT đang có ca dạy chưa hoàn thành hoặc đang phụ trách học viên, hệ thống từ chối xóa và hiển thị cảnh báo:
        // 'PT đang có lịch dạy chưa hoàn thành hoặc đang phụ trách hội viên', yêu cầu chuyển giao học viên trước khi chuyển trạng thái sang INACTIVE."
        var checkResult = await _ptService.CheckDeactivationEligibilityAsync(trainer.PtId);

        DeactivateActiveStudentsCount = checkResult.ActiveStudents;
        DeactivatePendingSessionsCount = checkResult.PendingSessions;
        CanDeactivateTargetPt = checkResult.CanDeactivate;

        if (!checkResult.CanDeactivate)
        {
            DeactivateWarningTitle = "Từ chối Vô hiệu hóa Tài khoản PT";
            DeactivateWarningMessage = checkResult.Message ?? "PT đang có lịch dạy chưa hoàn thành hoặc đang phụ trách hội viên";
        }
        else
        {
            DeactivateWarningTitle = "Xác nhận Vô hiệu hóa Tài khoản PT";
            DeactivateWarningMessage = $"Huấn luyện viên {trainer.FullName} hiện không có học viên hay lịch dạy dang dở. Bạn có chắc chắn muốn chuyển tài khoản sang INACTIVE?";
        }

        IsDeactivateWarningOpen = true;
    }

    [RelayCommand]
    public async Task ConfirmDeactivationAsync()
    {
        if (TargetPtToDeactivate == null || !CanDeactivateTargetPt)
            return;

        await _ptService.SetTrainerStatusAsync(TargetPtToDeactivate.PtId, false);
        IsDeactivateWarningOpen = false;
        await LoadTrainersAsync();

        StatusMessage = $"Đã chuyển trạng thái HLV {TargetPtToDeactivate.FullName} sang INACTIVE.";
    }

    [RelayCommand]
    public void CloseDeactivateWarning()
    {
        IsDeactivateWarningOpen = false;
    }

    [RelayCommand]
    public void OpenAssignTransfer()
    {
        IsDeactivateWarningOpen = false;
        StatusMessage = "Điều hướng đến màn hình Phân công lại PT (UC04.2) để chuyển giao học viên...";
    }
}
