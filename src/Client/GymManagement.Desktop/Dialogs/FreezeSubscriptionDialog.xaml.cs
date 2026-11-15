using Microsoft.UI.Xaml.Controls;

namespace GymManagement.Desktop.Dialogs;

public sealed partial class FreezeSubscriptionDialog : ContentDialog
{
    private readonly DateTime? _currentEndDate;

    public DateTime FreezeStartDate
    {
        get
        {
            return DpStart.Date.DateTime;
        }
    }

    public DateTime FreezeEndDate
    {
        get
        {
            return DpEnd.Date.DateTime;
        }
    }

    public string Reason
    {
        get
        {
            if (TxtReason != null && !string.IsNullOrWhiteSpace(TxtReason.Text))
            {
                return TxtReason.Text.Trim();
            }
            return string.Empty;
        }
    }

    public FreezeSubscriptionDialog(string memberName, DateTime? currentEndDate)
    {
        InitializeComponent();
        _currentEndDate = currentEndDate;

        TxtMemberName.Text = $"Hội viên: {memberName}";
        if (currentEndDate.HasValue)
        {
            TxtCurrentEnd.Text = $"Hạn hiện tại: {currentEndDate.Value:dd/MM/yyyy}";
        }
        else
        {
            TxtCurrentEnd.Text = "Chưa kích hoạt gói";
        }

        DpStart.Date = DateTimeOffset.Now;
        DpEnd.Date = DateTimeOffset.Now.AddDays(14);
        UpdateCalculations();
    }

    private void OnDateChanged(object sender, DatePickerValueChangedEventArgs e)
    {
        UpdateCalculations();
    }

    private void OnReasonTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateCalculations();
    }

    private void UpdateCalculations()
    {
        if (DpStart == null || DpEnd == null || TxtDaysFrozen == null || TxtProjectedEnd == null)
        {
            return;
        }

        var start = DpStart.Date.DateTime;
        var end = DpEnd.Date.DateTime;
        bool isDateValid = true;

        if (end <= start)
        {
            TxtDaysFrozen.Text = "Ngày tập lại phải sau ngày bắt đầu nghỉ!";
            TxtProjectedEnd.Text = "Hạn mới dự kiến: Không hợp lệ";
            isDateValid = false;
        }
        else
        {
            var days = (end - start).Days;
            TxtDaysFrozen.Text = $"Số ngày bảo lưu: {days} ngày";

            if (_currentEndDate.HasValue)
            {
                var projected = _currentEndDate.Value.AddDays(days);
                TxtProjectedEnd.Text = $"Hạn mới dự kiến: {projected:dd/MM/yyyy} (+{days} ngày cộng dồn)";
            }
            else
            {
                TxtProjectedEnd.Text = "Hạn mới dự kiến: Tính từ ngày kích hoạt lại";
            }
        }

        string reason = string.Empty;
        if (TxtReason != null && !string.IsNullOrWhiteSpace(TxtReason.Text))
        {
            reason = TxtReason.Text.Trim();
        }

        bool isReasonValid = true;
        if (string.IsNullOrWhiteSpace(reason))
        {
            isReasonValid = false;
            if (TxtReasonError != null)
            {
                TxtReasonError.Text = "Vui lòng nhập lý do bảo lưu gói tập.";
                TxtReasonError.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            }
        }
        else if (reason.Length < 3)
        {
            isReasonValid = false;
            if (TxtReasonError != null)
            {
                TxtReasonError.Text = "Lý do bảo lưu phải có tối thiểu 3 ký tự.";
                TxtReasonError.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            }
        }
        else
        {
            if (TxtReasonError != null)
            {
                TxtReasonError.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            }
        }

        if (isDateValid && isReasonValid)
        {
            IsPrimaryButtonEnabled = true;
        }
        else
        {
            IsPrimaryButtonEnabled = false;
        }
    }
}
