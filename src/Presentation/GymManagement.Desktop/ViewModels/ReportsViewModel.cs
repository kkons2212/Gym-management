using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Core.Contracts;

namespace GymManagement.Desktop.ViewModels;

/// <summary>
/// Loại báo cáo Admin có thể lựa chọn trên màn hình Báo cáo &amp; Thống kê (UC06 – bước 3).
/// Thứ tự giá trị enum phải khớp với <see cref="ReportsViewModel.ReportTypeOptions"/>.
/// </summary>
public enum ReportType
{
    Revenue,        // Doanh thu
    MemberGrowth,   // Tăng trưởng hội viên
    CheckInTraffic  // Lưu lượng điểm danh
}

/// <summary>
/// Một dòng số liệu báo cáo. Dùng chung cho cả bảng số liệu và biểu đồ cột (UC06 – bước 6).
/// </summary>
public sealed class ReportRow
{
    public string Label { get; init; } = string.Empty;

    public double Value { get; init; }

    /// <summary>Giá trị đã định dạng sẵn để hiển thị (kèm đơn vị rút gọn).</summary>
    public string DisplayValue { get; init; } = string.Empty;

    /// <summary>Tỷ trọng của dòng so với tổng, đã định dạng phần trăm.</summary>
    public string ShareText { get; init; } = string.Empty;

    /// <summary>
    /// Chiều cao cột (px) đã quy đổi tuyến tính theo giá trị lớn nhất trong kỳ báo cáo.
    /// WinUI không có control biểu đồ sẵn nên phải tự tính tỷ lệ chiều cao thay vì scale trong XAML.
    /// </summary>
    public double BarHeight { get; init; }
}

public partial class ReportsViewModel : ObservableObject
{
    /// <summary>Chiều cao tối đa của cột biểu đồ (px); cột có giá trị lớn nhất sẽ chiếm đúng giá trị này.</summary>
    private const double MaxBarHeight = 160;

    private readonly IReportService _reportService;

    /// <summary>Nhãn hiển thị của từng loại báo cáo, ánh xạ theo chỉ số với enum <see cref="ReportType"/>.</summary>
    public IReadOnlyList<string> ReportTypeOptions { get; } = ["Doanh thu", "Tăng trưởng hội viên", "Lưu lượng điểm danh"];

    [ObservableProperty]
    private int _selectedReportIndex;

    [ObservableProperty]
    private DateTimeOffset _fromDate = DateTimeOffset.Now.Date.AddDays(-6);

    [ObservableProperty]
    private DateTimeOffset _toDate = DateTimeOffset.Now.Date;

    [ObservableProperty]
    private string _reportTitle = "Báo cáo Doanh thu";

    [ObservableProperty]
    private string _reportSubtitle = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>Dùng để mở/đóng thanh thông báo (InfoBar) mà không cần converter trong XAML.</summary>
    [ObservableProperty]
    private bool _hasStatusMessage;

    /// <summary>True khi báo cáo có dữ liệu → hiện biểu đồ + bảng số liệu.</summary>
    [ObservableProperty]
    private bool _hasData;

    /// <summary>True khi không tìm thấy dữ liệu phù hợp (UC06 – 5.1) → hiện thông báo rỗng.</summary>
    [ObservableProperty]
    private bool _showEmptyState;

    public ObservableCollection<ReportRow> Rows { get; } = [];

    public ReportsViewModel(IReportService reportService)
    {
        _reportService = reportService;
        _ = LoadReportAsync();
    }

    /// <summary>Tự động tải lại khi Admin đổi loại báo cáo (UC06 – bước 3).</summary>
    partial void OnSelectedReportIndexChanged(int value) => _ = LoadReportAsync();

    /// <summary>Đồng bộ trạng thái hiển thị thanh thông báo với nội dung thông báo.</summary>
    partial void OnStatusMessageChanged(string value) => HasStatusMessage = !string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Truy xuất và tổng hợp dữ liệu theo loại báo cáo + khoảng thời gian đã chọn (UC06 – bước 5, 6).
    /// </summary>
    [RelayCommand]
    public async Task LoadReportAsync()
    {
        // 5.1: khoảng thời gian không hợp lệ cũng coi như không có dữ liệu phù hợp.
        if (FromDate.Date > ToDate.Date)
        {
            ApplyResult([], "Báo cáo", "Khoảng thời gian không hợp lệ.", string.Empty);
            StatusMessage = "Không có dữ liệu phù hợp.";
            return;
        }

        var reportType = (ReportType)SelectedReportIndex;

        // Mỗi loại báo cáo có nguồn dữ liệu, tiêu đề và đơn vị hiển thị riêng.
        List<(string Label, double Value)> data;
        string unit;
        string title;
        string subtitle;

        switch (reportType)
        {
            case ReportType.MemberGrowth:
                // Số tháng được suy ra từ khoảng thời gian đã lọc để truyền vào IReportService.
                var months = Math.Max(1, ((ToDate.Year - FromDate.Year) * 12) + ToDate.Month - FromDate.Month + 1);
                var growth = await _reportService.GetMemberGrowthAsync(months);
                data = growth.Select(kv => (kv.Key, (double)kv.Value)).ToList();
                unit = "hội viên";
                title = "Báo cáo Tăng trưởng Hội viên";
                subtitle = $"Số hội viên mới theo tháng ({months} tháng)";
                break;

            case ReportType.CheckInTraffic:
                // Báo cáo lưu lượng chỉ áp dụng cho một ngày → dùng ngày bắt đầu của bộ lọc.
                var traffic = await _reportService.GetCheckInTrafficByHourAsync(FromDate.Date);
                data = traffic.Select(kv => (kv.Key, (double)kv.Value)).ToList();
                unit = "lượt";
                title = "Báo cáo Lưu lượng Điểm danh";
                subtitle = string.Format(CultureInfo.InvariantCulture, "Số lượt check-in theo khung giờ ngày {0:dd/MM/yyyy}", FromDate);
                break;

            default:
                var revenue = await _reportService.GetRevenueByTimeRangeAsync(FromDate.Date, ToDate.Date);
                data = revenue.Select(kv => (kv.Key, (double)kv.Value)).ToList();
                unit = "VNĐ";
                title = "Báo cáo Doanh thu";
                subtitle = string.Format(CultureInfo.InvariantCulture, "Doanh thu từ {0:dd/MM/yyyy} đến {1:dd/MM/yyyy}", FromDate, ToDate);
                break;
        }

        ApplyResult(data, title, subtitle, unit);
        StatusMessage = HasData
            ? $"Đã tổng hợp {Rows.Count} dòng số liệu."
            : "Không có dữ liệu phù hợp.";
    }

    /// <summary>
    /// Đưa kết quả truy xuất lên giao diện: tính tỷ lệ chiều cao cột và phần trăm tỷ trọng cho từng dòng.
    /// </summary>
    private void ApplyResult(IEnumerable<(string Label, double Value)> data, string title, string subtitle, string unit)
    {
        ReportTitle = title;
        ReportSubtitle = subtitle;

        var items = data.ToList();
        Rows.Clear();

        // 5.1.1: không có bản ghi nào thoả tiêu chí → bật trạng thái rỗng và bỏ qua vẽ biểu đồ.
        if (items.Count == 0)
        {
            HasData = false;
            ShowEmptyState = true;
            return;
        }

        // Tính trước giá trị lớn nhất và tổng để chuẩn hoá chiều cao cột / tỷ trọng phần trăm.
        double max = items.Max(i => i.Value);
        double total = items.Sum(i => i.Value);

        foreach (var (label, value) in items)
        {
            // Đảm bảo cột nhỏ nhất vẫn nhìn thấy được (tối thiểu 4px) nhưng không vượt quá khung.
            var barHeight = max <= 0 ? 0 : Math.Max(4, MaxBarHeight * value / max);
            var share = total <= 0 ? 0 : value / total * 100;

            Rows.Add(new ReportRow
            {
                Label = label,
                Value = value,
                DisplayValue = FormatValue(value, unit),
                ShareText = string.Format(CultureInfo.InvariantCulture, "{0:0.#}%", share),
                BarHeight = barHeight
            });
        }

        HasData = true;
        ShowEmptyState = false;
    }

    /// <summary>
    /// Định dạng giá trị hiển thị: doanh thu quy đổi sang "triệu đồng" cho gọn, các chỉ số đếm giữ nguyên số.
    /// </summary>
    private static string FormatValue(double value, string unit) => unit == "VNĐ"
        ? string.Format(CultureInfo.InvariantCulture, "{0:0.#} tr", value / 1_000_000)
        : string.Format(CultureInfo.InvariantCulture, "{0:0} {1}", value, unit);
}
