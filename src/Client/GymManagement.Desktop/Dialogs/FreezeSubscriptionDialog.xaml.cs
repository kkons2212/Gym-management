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
            return TxtReason.Text;
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

    private void UpdateCalculations()
    {
        if (DpStart == null || DpEnd == null || TxtDaysFrozen == null || TxtProjectedEnd == null)
        {
            return;
        }

        var start = DpStart.Date.DateTime;
        var end = DpEnd.Date.DateTime;

        if (end <= start)
        {
            TxtDaysFrozen.Text = "Ngày tập lại phải sau ngày bắt đầu nghỉ!";
            TxtProjectedEnd.Text = "Hạn mới dự kiến: Không hợp lệ";
            IsPrimaryButtonEnabled = false;
            return;
        }

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

        IsPrimaryButtonEnabled = true;
    }
}
