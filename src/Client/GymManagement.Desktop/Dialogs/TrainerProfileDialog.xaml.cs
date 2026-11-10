using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.Services.Api;
using GymManagement.Server.Core.DTOs.PT;

namespace GymManagement.Desktop.Dialogs;

public sealed partial class TrainerProfileDialog : ContentDialog
{
    private readonly IApiClient _apiClient;
    private readonly PtProfileDto? _existingTrainer;

    internal PtProfileDto? SavedTrainer { get; private set; }

    public TrainerProfileDialog(IApiClient apiClient, PtProfileDto? trainer = null)
    {
        ArgumentNullException.ThrowIfNull(apiClient);

        _apiClient = apiClient;
        _existingTrainer = trainer;

        InitializeComponent();

        if (trainer != null)
        {
            Title = "CẬP NHẬT HỒ SƠ HUẤN LUYỆN VIÊN";
            TxtSubtitle.Text = "Chỉnh sửa thông tin chuyên môn hoặc trạng thái hoạt động của huấn luyện viên.";
            PnlPtId.Visibility = Visibility.Visible;
            TxtPtId.Text = trainer.PtId;
            TxtFullName.Text = trainer.FullName;
            TxtPhone.Text = trainer.Phone;
            TxtEmail.Text = trainer.Email;
            NbExperienceYears.Value = trainer.ExperienceYears;
            TsIsActive.IsOn = trainer.IsActive;

            SelectSpecialty(trainer.Specialty);
        }

        PrimaryButtonClick += TrainerProfileDialog_PrimaryButtonClick;
    }

    private void SelectSpecialty(string specialty)
    {
        if (string.IsNullOrWhiteSpace(specialty))
        {
            return;
        }

        foreach (var item in CboSpecialty.Items)
        {
            if (item is ComboBoxItem cbi)
            {
                string text = cbi.Content?.ToString() ?? string.Empty;
                if (text.Contains(specialty, StringComparison.OrdinalIgnoreCase) ||
                    specialty.Contains(text, StringComparison.OrdinalIgnoreCase))
                {
                    CboSpecialty.SelectedItem = cbi;
                    return;
                }
            }
        }
    }

    private async void TrainerProfileDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;

        PnlValidationError.Visibility = Visibility.Collapsed;
        TxtValidationError.Text = string.Empty;

        string fullName = TxtFullName.Text.Trim();
        string phone = TxtPhone.Text.Trim();
        string email = TxtEmail.Text.Trim();
        int experienceYears = (int)NbExperienceYears.Value;
        bool isActive = TsIsActive.IsOn;

        string specialty = "Thể hình & Tăng cơ";
        if (CboSpecialty.SelectedItem is ComboBoxItem selectedItem && selectedItem.Content != null)
        {
            specialty = selectedItem.Content.ToString()!;
        }

        // 1. Validation
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Length < 3)
        {
            ShowError("Họ và tên HLV phải có ít nhất 3 ký tự.");
            return;
        }

        if (!Regex.IsMatch(phone, @"^0\d{9}$"))
        {
            ShowError("Số điện thoại không hợp lệ. Vui lòng nhập đúng 10 chữ số (bắt đầu bằng 0).");
            return;
        }

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            ShowError("Địa chỉ email không đúng định dạng.");
            return;
        }

        // 2. Idempotency Lock
        IsPrimaryButtonEnabled = false;
        PrSaving.Visibility = Visibility.Visible;
        PrSaving.IsActive = true;

        try
        {
            string? ptId = null;
            if (_existingTrainer != null)
            {
                ptId = _existingTrainer.PtId;
            }

            var request = new SavePtProfileRequest(
                ptId,
                fullName,
                phone,
                email,
                specialty,
                experienceYears,
                isActive
            );

            SavedTrainer = await _apiClient.SaveTrainerAsync(request);
            Hide();
        }
        catch (Exception ex)
        {
            ShowError($"Không thể lưu hồ sơ HLV: {ex.Message}");
        }
        finally
        {
            IsPrimaryButtonEnabled = true;
            PrSaving.IsActive = false;
            PrSaving.Visibility = Visibility.Collapsed;
        }
    }

    private void ShowError(string message)
    {
        TxtValidationError.Text = message;
        PnlValidationError.Visibility = Visibility.Visible;
    }
}

