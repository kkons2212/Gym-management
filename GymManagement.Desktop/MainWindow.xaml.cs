using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;

namespace GymManagement.Desktop;

public partial class MainWindow : Window
{
    private const string AdminRole = "Admin";
    private const string TrainerRole = "Huấn luyện viên";
    private const string MemberRole = "Hội viên";

    private static readonly Color Navy = FromHex(0x172033);
    private static readonly Color NavyLight = FromHex(0x222E45);
    private static readonly Color Green = FromHex(0x24B98A);
    private static readonly Color GreenPale = FromHex(0xE8F7F1);
    private static readonly Color Ink = FromHex(0x1C2638);
    private static readonly Color Muted = FromHex(0x8490A3);
    private static readonly Color Canvas = FromHex(0xF5F7FA);
    private static readonly Color Line = FromHex(0xE8ECF1);
    private static readonly Color White = Colors.White;

    private readonly Grid _shell = new();
    private readonly StackPanel _navigation = new();
    private readonly StackPanel _pageHost = new();
    private readonly ComboBox _rolePicker = new();
    private readonly List<UserAccount> _userAccounts =
    [
        new("Lê Thanh Tùng", "tung.le@gymhouse.vn", "0908 123 456", "tung.le", "Admin", true, true),
        new("Nguyễn Quốc Bảo", "bao.nguyen@gymhouse.vn", "0903 456 789", "bao.nguyen", "PT", true, false),
        new("Trần Thu Trang", "trang.tran@gymhouse.vn", "0907 111 222", "trang.tran", "PT", true, false),
        new("Hoàng Thu Hà", "ha.hoang@gymhouse.vn", "0912 345 678", "ha.hoang", "Lễ tân", true, false),
        new("Lê Ngọc Mai", "mai.le@gymhouse.vn", "0933 222 111", "mai.le", "Lễ tân", true, false),
        new("Đỗ Văn Nam", "nam.do@gymhouse.vn", "0988 765 432", "nam.do", "Lễ tân", true, false),
        new("Nguyễn Minh Anh", "minhanh@email.com", "0901 234 567", "minhanh", MemberRole, true, false, "Hội viên", "MB-0248"),
        new("Trần Quốc Bảo", "baotran@email.com", "0903 456 789", "baotran", MemberRole, false, false, "Hội viên", "MB-0247")
    ];
    private readonly List<MembershipPackage> _membershipPackages =
    [
        new("Standard", 3600000, "6 tháng", 0, 7, "Sử dụng khu vực tập gym", 84, true),
        new("Premium", 6900000, "12 tháng", 0, 14, "Gym, lớp nhóm, phòng xông hơi", 126, true),
        new("Gym + PT", 4800000, "12 buổi", 12, 7, "12 buổi tập 1:1 cùng huấn luyện viên", 23, true),
        new("Yoga linh hoạt", 2400000, "3 tháng", 12, 0, "12 buổi Yoga nhóm", 0, false)
    ];
    private readonly List<MemberEnrollment> _memberEnrollments =
    [
        new("Nguyễn Minh Anh", "MB-0248", "0901 234 567", "079012340001", "Premium", new DateOnly(2026, 4, 19), new DateOnly(2027, 4, 18), "Đang hoạt động", "minhanh@email.com", "Quận 1, TP. Hồ Chí Minh"),
        new("Trần Quốc Bảo", "MB-0247", "0903 456 789", "079012340002", "Gym + PT", new DateOnly(2026, 3, 3), new DateOnly(2027, 3, 2), "Đang hoạt động", "baotran@email.com", "Quận 3, TP. Hồ Chí Minh"),
        new("Lê Hoàng Nam", "MB-0246", "0905 111 246", "079012340003", "Standard", new DateOnly(2025, 4, 17), null, "Hết hạn", "nam.le@email.com", "Thủ Đức, TP. Hồ Chí Minh"),
        new("Phạm Thu Hà", "MB-0245", "0908 222 345", "079012340004", "Premium", new DateOnly(2025, 10, 29), new DateOnly(2026, 10, 28), "Đang bảo lưu", "ha.pham@email.com", "Quận Bình Thạnh, TP. Hồ Chí Minh"),
        new("Đỗ Gia Huy", "MB-0244", "0906 333 444", "079012340005", "Gym + PT", new DateOnly(2026, 1, 31), new DateOnly(2027, 1, 30), "Đang hoạt động", "huy.do@email.com", "Quận 7, TP. Hồ Chí Minh")
    ];
    private StackPanel? _accountsTableBody;
    private StackPanel? _memberTableBody;
    private string _role = AdminRole;
    private string _page = "Tổng quan";
    private string _settingsSection = "Quản lý tài khoản";
    private string _accountStatusFilter = "Tất cả trạng thái";
    private string _accountRoleFilter = "Tất cả vai trò";
    private string _accountTypeFilter = "Tất cả loại tài khoản";
    private string _openingTime = "05:00";
    private string _closingTime = "22:00";
    private string _cancellationHours = "2";
    private string _maxFreezeDays = "31";
    private string _duplicateScanSeconds = "30";
    private string _memberPhone = "0901 234 567";
    private string _memberEmail = "minhanh@email.com";
    private string _memberAddress = "Quận 1, TP. Hồ Chí Minh";

    public MainWindow()
    {
        InitializeComponent();
        Title = "GYM HOUSE | Quản lý phòng tập";
        RootGrid.Children.Add(_shell);
        BuildShell();
    }

    private void BuildShell()
    {
        _shell.Background = Brush(Canvas);
        _shell.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(248) });
        _shell.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var sidebar = new StackPanel { Background = Brush(Navy), Spacing = 0 };
        Grid.SetColumn(sidebar, 0);
        _shell.Children.Add(sidebar);

        var brand = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Margin = new Thickness(22, 25, 18, 30),
            VerticalAlignment = VerticalAlignment.Center
        };
        brand.Children.Add(new Border
        {
            Width = 40,
            Height = 40,
            CornerRadius = new CornerRadius(13),
            Background = Brush(Green),
            Child = new TextBlock
            {
                Text = "G",
                FontSize = 23,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = Brush(White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        });
        var brandText = new StackPanel { Spacing = 1, VerticalAlignment = VerticalAlignment.Center };
        brandText.Children.Add(new TextBlock
        {
            Text = "GYM HOUSE",
            FontSize = 15,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = Brush(White)
        });
        brandText.Children.Add(new TextBlock { Text = "FITNESS MANAGEMENT", FontSize = 9, Foreground = Brush(FromHex(0x9AA7BB)) });
        brand.Children.Add(brandText);
        sidebar.Children.Add(brand);

        sidebar.Children.Add(new TextBlock
        {
            Text = "KHÔNG GIAN LÀM VIỆC",
            FontSize = 9,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = Brush(FromHex(0x8490A3)),
            Margin = new Thickness(22, 0, 12, 12)
        });
        _navigation.Margin = new Thickness(12, 0, 12, 0);
        sidebar.Children.Add(_navigation);

        var sidebarBottom = new StackPanel { VerticalAlignment = VerticalAlignment.Bottom, Spacing = 12, Margin = new Thickness(18, 0, 18, 20) };
        sidebarBottom.Children.Add(new Border
        {
            Background = Brush(NavyLight),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(14),
            Child = new StackPanel
            {
                Spacing = 5,
                Children =
                {
                    new TextBlock { Text = "CẦN HỖ TRỢ?", FontSize = 9, FontWeight = Microsoft.UI.Text.FontWeights.Bold, Foreground = Brush(FromHex(0xA9B5C8)) },
                    new TextBlock { Text = "Liên hệ đội ngũ kỹ thuật", FontSize = 12, Foreground = Brush(White) }
                }
            }
        });
        sidebarBottom.Children.Add(new TextBlock { Text = "Bản mẫu giao diện · Dữ liệu minh họa", FontSize = 9, Foreground = Brush(FromHex(0x8490A3)) });
        sidebar.Children.Add(new Border { VerticalAlignment = VerticalAlignment.Stretch });
        sidebar.Children.Add(sidebarBottom);

        var main = new Grid();
        main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(72) });
        main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        Grid.SetColumn(main, 1);
        _shell.Children.Add(main);

        var topBar = new Grid { Background = Brush(White), Padding = new Thickness(30, 0, 32, 0) };
        topBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        topBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        topBar.Children.Add(new TextBlock
        {
            Text = "Không gian quản lý phòng tập",
            FontSize = 13,
            Foreground = Brush(Muted),
            VerticalAlignment = VerticalAlignment.Center
        });

        var topActions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 14, VerticalAlignment = VerticalAlignment.Center };
        topActions.Children.Add(new TextBlock { Text = "ĐANG XEM VỚI VAI TRÒ", FontSize = 9, Foreground = Brush(Muted), VerticalAlignment = VerticalAlignment.Center });
        _rolePicker.Items.Add(AdminRole);
        _rolePicker.Items.Add(TrainerRole);
        _rolePicker.Items.Add(MemberRole);
        _rolePicker.SelectedIndex = 0;
        _rolePicker.Width = 164;
        _rolePicker.SelectionChanged += (_, _) =>
        {
            _role = _rolePicker.SelectedItem?.ToString() ?? AdminRole;
            _page = "Tổng quan";
            Render();
        };
        topActions.Children.Add(_rolePicker);
        topActions.Children.Add(new Border
        {
            Width = 36,
            Height = 36,
            CornerRadius = new CornerRadius(18),
            Background = Brush(GreenPale),
            Child = new TextBlock
            {
                Text = _role == AdminRole ? "A" : "U",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = Brush(Green),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        });
        Grid.SetColumn(topActions, 1);
        topBar.Children.Add(topActions);
        main.Children.Add(topBar);

        var scroller = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        _pageHost.Margin = new Thickness(34, 27, 34, 34);
        scroller.Content = _pageHost;
        Grid.SetRow(scroller, 1);
        main.Children.Add(scroller);
        Render();
    }

    private void Render()
    {
        BuildNavigation();
        _pageHost.Children.Clear();
        _pageHost.Spacing = 22;

        switch (_role)
        {
            case TrainerRole:
                if (_page == "Học viên") RenderTrainerMembers();
                else if (_page == "Tiến độ") RenderTrainerProgress();
                else RenderTrainerSchedule();
                break;
            case MemberRole:
                if (_page == "Gói tập") RenderMembership();
                else if (_page == "Đặt lịch với PT") RenderBookings();
                else if (_page == "Hồ sơ cá nhân") RenderProfile();
                else RenderMemberHome();
                break;
            default:
                if (_page == "Quản lý hội viên & gói tập") RenderAdminMembers();
                else if (_page == "Check-in") RenderCheckIn();
                else if (_page == "PT & lịch dạy") RenderAdminSchedule();
                else if (_page == "Thanh toán") RenderPayments();
                else if (_page == "Báo cáo") RenderReports();
                else if (_page == "Cài đặt") RenderSettings();
                else RenderAdminHome();
                break;
        }
    }

    private void BuildNavigation()
    {
        _navigation.Children.Clear();
        var items = _role switch
        {
            TrainerRole => new[] { "Lịch dạy", "Học viên", "Tiến độ" },
            MemberRole => new[] { "Tổng quan", "Gói tập", "Đặt lịch với PT", "Hồ sơ cá nhân" },
            _ => new[] { "Tổng quan", "Quản lý hội viên & gói tập", "Check-in", "PT & lịch dạy", "Thanh toán", "Báo cáo", "Cài đặt" }
        };

        foreach (var item in items)
        {
            var selected = item == _page || (_page == "Tổng quan" && item == "Lịch dạy" && _role == TrainerRole);
            var button = new Button
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Background = selected ? Brush(NavyLight) : Brush(Navy),
                Foreground = selected ? Brush(White) : Brush(FromHex(0xB4BFD0)),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(13, 11, 12, 11),
                Margin = new Thickness(0, 2, 0, 4),
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 12,
                    Children =
                    {
                        new TextBlock { Text = NavGlyph(item), FontSize = 15, Foreground = selected ? Brush(Green) : Brush(FromHex(0xAAB5C6)), VerticalAlignment = VerticalAlignment.Center, Width = 20 },
                        new TextBlock { Text = item, FontSize = 12, VerticalAlignment = VerticalAlignment.Center }
                    }
                }
            };
            button.Click += (_, _) =>
            {
                _page = item;
                Render();
            };
            _navigation.Children.Add(button);
        }
    }

    private void RenderAdminHome()
    {
        AddHeading("Tổng quan", "Thứ Bảy, 17 tháng 10, 2026", "Chào buổi sáng, Admin 👋");
        var stats = new[]
        {
            ("Hội viên đang hoạt động", "1.284", "+8,2%", "♙", Green),
            ("Doanh thu tháng này", "248,5 tr", "+12,5%", "₫", FromHex(0x5785F5)),
            ("Lượt check-in hôm nay", "186", "+5,4%", "↗", FromHex(0xF2A64A)),
            ("Ca tập với PT hôm nay", "24", "6 ca sắp tới", "◷", FromHex(0x9B73E8))
        };
        var statGrid = CreateGrid(4, 14);
        foreach (var stat in stats)
            statGrid.Children.Add(MetricCard(stat.Item1, stat.Item2, stat.Item3, stat.Item4, stat.Item5));
        _pageHost.Children.Add(statGrid);

        var split = CreateGrid(2, 18);
        split.ColumnDefinitions[0].Width = new GridLength(1.35, GridUnitType.Star);
        split.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
        var revenue = Panel("Doanh thu", "Tổng quan doanh thu theo tháng");
        revenue.Children.Add(RevenueChart());
        Grid.SetColumn(revenue, 0);
        split.Children.Add(revenue);

        var activity = Panel("Hoạt động hôm nay", "Cập nhật lượt ra vào mới nhất");
        activity.Children.Add(RowHeader("HỘI VIÊN", "THỜI GIAN", "TRẠNG THÁI"));
        activity.Children.Add(DataRow("Nguyễn Minh Anh", "09:42", "Đã vào", Green));
        activity.Children.Add(DataRow("Trần Quốc Bảo", "09:38", "Đã vào", Green));
        activity.Children.Add(DataRow("Lê Hoàng Nam", "09:31", "Đã vào", Green));
        activity.Children.Add(DataRow("Phạm Thu Hà", "09:16", "Từ chối", FromHex(0xE47A72)));
        split.Children.Add(activity);
        Grid.SetColumn(activity, 1);
        _pageHost.Children.Add(split);

        var schedule = Panel("Lịch PT sắp tới", "Các ca tập tiếp theo trong ngày");
        schedule.Children.Add(DataRow("10:00  ·  Nguyễn Minh Anh", "PT Quốc Bảo", "Đang chờ", FromHex(0xE7A943)));
        schedule.Children.Add(DataRow("10:30  ·  Lê Hoàng Nam", "PT Thu Trang", "Đã xác nhận", Green));
        schedule.Children.Add(DataRow("11:00  ·  Trần Gia Huy", "PT Minh Khang", "Đã xác nhận", Green));
        _pageHost.Children.Add(schedule);
    }

    private void RenderAdminMembers()
    {
        AddHeading("Hội viên & gói tập", "Tìm kiếm, cập nhật hồ sơ và quản lý trạng thái đăng ký gói tập", "Danh sách hội viên");
        var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        var search = new TextBox { PlaceholderText = "Tìm theo tên, số điện thoại hoặc mã hội viên...", Width = 380 };
        search.TextChanged += (_, _) => PopulateMemberTable(search.Text);
        toolbar.Children.Add(search);
        var registerButton = AccentButton("+  Đăng ký hội viên");
        registerButton.Click += async (_, _) => await ShowMemberRegistrationAsync();
        toolbar.Children.Add(registerButton);
        toolbar.Children.Add(OutlineButton("Gia hạn / nâng cấp"));
        _pageHost.Children.Add(toolbar);
        var panel = Panel("1.284 hội viên", "Tìm hội viên rồi chọn Cập nhật để sửa SĐT, email, địa chỉ · Bảo lưu hoặc hủy đăng ký gói tại từng hội viên · UC02");
        _memberTableBody = new StackPanel { Spacing = 0 };
        panel.Children.Add(MemberTableHeader());
        panel.Children.Add(_memberTableBody);
        PopulateMemberTable(search.Text);
        _pageHost.Children.Add(panel);
    }

    private void PopulateMemberTable(string searchText)
    {
        if (_memberTableBody is null)
            return;

        _memberTableBody.Children.Clear();
        var query = searchText.Trim();
        var members = _memberEnrollments.Where(member =>
            string.IsNullOrEmpty(query)
            || member.FullName.Contains(query, StringComparison.OrdinalIgnoreCase)
            || member.Phone.Contains(query, StringComparison.OrdinalIgnoreCase)
            || member.MemberCode.Contains(query, StringComparison.OrdinalIgnoreCase)
            || member.Email.Contains(query, StringComparison.OrdinalIgnoreCase));

        foreach (var member in members)
        {
            var package = _membershipPackages.FirstOrDefault(item => item.Name == member.PackageName);
            var expiry = member.Status == "Đã hủy"
                ? $"Hủy ngày {member.CanceledDate:dd/MM/yyyy}"
                : member.Status == "Hết hạn"
                    ? "Đã hết hạn"
                    : member.EndDate.HasValue
                        ? member.EndDate.Value.ToString("dd/MM/yyyy")
                        : package is null ? "Theo số buổi" : $"Theo số buổi · {package.SessionCount} buổi";
            var statusColor = member.Status switch
            {
                "Đang hoạt động" => Green,
                "Đang bảo lưu" => FromHex(0x5785F5),
                "Đã hủy" => FromHex(0xD75E55),
                _ => FromHex(0xE7A943)
            };
            _memberTableBody.Children.Add(MemberManagementRow(member, package, expiry, statusColor));
        }

        if (!_memberTableBody.Children.Any())
            _memberTableBody.Children.Add(new TextBlock { Text = "Không tìm thấy hội viên phù hợp.", FontSize = 11, Foreground = Brush(Muted), Margin = new Thickness(16) });
    }

    private UIElement MemberManagementRow(MemberEnrollment member, MembershipPackage? package, string expiry, Color statusColor)
    {
        var row = new Grid { Padding = new Thickness(15, 10, 15, 10), BorderBrush = Brush(Line), BorderThickness = new Thickness(0, 0, 0, 1) };
        foreach (var width in new[] { 1.35, 1.0, 0.9, 1.0, 2.6 })
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(width, GridUnitType.Star) });
        var values = new[] { $"{member.FullName}  ·  {member.MemberCode}", package?.Name ?? member.PackageName, expiry, member.Status };
        for (var index = 0; index < values.Length; index++)
        {
            var text = new TextBlock
            {
                Text = values[index],
                FontSize = 10,
                Foreground = index == 3 ? Brush(statusColor) : Brush(index == 0 ? Ink : Muted),
                FontWeight = index == 0 || index == 3 ? Microsoft.UI.Text.FontWeights.SemiBold : Microsoft.UI.Text.FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            Grid.SetColumn(text, index);
            row.Children.Add(text);
        }

        var canManage = member.Status is "Đang hoạt động" or "Đang bảo lưu";
        var canFreeze = member.Status == "Đang hoạt động" && package is { MaxFreezeDays: > 0 };
        var action = SmallActionButton(member.Status == "Đang bảo lưu" ? "Tiếp tục gói" : "Bảo lưu");
        action.IsEnabled = member.Status == "Đang bảo lưu" || canFreeze;
        action.Opacity = action.IsEnabled ? 1 : 0.45;
        action.Click += async (_, _) =>
        {
            if (member.Status == "Đang bảo lưu")
                await ResumeMemberMembershipAsync(member);
            else if (package is not null)
                await ShowFreezeMembershipAsync(member, package);
        };
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, VerticalAlignment = VerticalAlignment.Center };
        var edit = SmallActionButton("Cập nhật");
        edit.Click += async (_, _) => await ShowMemberEditorAsync(member);
        actions.Children.Add(edit);
        actions.Children.Add(action);
        var cancel = SmallActionButton("Hủy gói");
        cancel.IsEnabled = canManage;
        cancel.Opacity = cancel.IsEnabled ? 1 : 0.45;
        cancel.Click += async (_, _) => await CancelMemberMembershipAsync(member);
        actions.Children.Add(cancel);
        Grid.SetColumn(actions, 4);
        row.Children.Add(actions);
        return row;
    }

    private static Grid MemberTableHeader()
    {
        var header = new Grid { Background = Brush(FromHex(0xF8FAFC)), Padding = new Thickness(15, 10, 15, 10) };
        foreach (var width in new[] { 1.35, 1.0, 0.9, 1.0, 2.6 })
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(width, GridUnitType.Star) });
        var labels = new[] { "HỘI VIÊN", "GÓI TẬP", "HẠN GÓI", "TRẠNG THÁI GÓI", "THAO TÁC" };
        for (var index = 0; index < labels.Length; index++)
        {
            var text = new TextBlock
            {
                Text = labels[index],
                FontSize = 8,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = Brush(Muted),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(text, index);
            header.Children.Add(text);
        }
        return header;
    }

    private async Task ShowMemberEditorAsync(MemberEnrollment member)
    {
        var phone = new TextBox { Header = "Số điện thoại", Text = member.Phone, PlaceholderText = "0900 000 000" };
        var email = new TextBox { Header = "Email", Text = member.Email, PlaceholderText = "member@example.com" };
        var address = new TextBox
        {
            Header = "Địa chỉ",
            Text = member.Address,
            PlaceholderText = "Nhập địa chỉ hội viên",
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 64
        };
        var error = new TextBlock { FontSize = 10, Foreground = Brush(FromHex(0xD75E55)), TextWrapping = TextWrapping.Wrap };
        var form = new StackPanel
        {
            Spacing = 9,
            Children =
            {
                new TextBlock { Text = $"{member.FullName} · {member.MemberCode}", FontSize = 12, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(Ink) },
                phone,
                email,
                address,
                error
            }
        };
        var dialog = new ContentDialog
        {
            Title = "Cập nhật hồ sơ hội viên",
            Content = new ScrollViewer { Content = form, MaxHeight = 540 },
            PrimaryButtonText = "Lưu thay đổi",
            CloseButtonText = "Hủy",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = RootGrid.XamlRoot
        };
        dialog.PrimaryButtonClick += (_, args) =>
        {
            var normalizedPhone = phone.Text.Trim().Replace(" ", "");
            var duplicate = _memberEnrollments.Any(other =>
                !ReferenceEquals(other, member)
                && (other.Phone.Replace(" ", "") == normalizedPhone
                    || other.Email.Equals(email.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                || _userAccounts.Any(account =>
                    account.MemberCode != member.MemberCode
                    && account.Email.Equals(email.Text.Trim(), StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrWhiteSpace(normalizedPhone)
                || normalizedPhone.Length < 9
                || string.IsNullOrWhiteSpace(email.Text)
                || !email.Text.Contains('@')
                || string.IsNullOrWhiteSpace(address.Text)
                || duplicate)
            {
                error.Text = duplicate
                    ? "Số điện thoại hoặc email đã được sử dụng bởi hội viên khác."
                    : "Vui lòng nhập số điện thoại, email hợp lệ và địa chỉ.";
                args.Cancel = true;
            }
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            return;

        member.Phone = phone.Text.Trim();
        member.Email = email.Text.Trim();
        member.Address = address.Text.Trim();
        var linkedAccount = _userAccounts.FirstOrDefault(account => account.MemberCode == member.MemberCode);
        if (linkedAccount is not null)
        {
            linkedAccount.Phone = member.Phone;
            linkedAccount.Email = member.Email;
        }
        Render();
        await ShowMessageAsync("Đã cập nhật hồ sơ", $"Thông tin liên hệ của {member.FullName} đã được cập nhật trong bản mẫu hiện tại.");
    }

    private async Task CancelMemberMembershipAsync(MemberEnrollment member)
    {
        if (member.Status is not ("Đang hoạt động" or "Đang bảo lưu"))
            return;

        var reason = new TextBox
        {
            Header = "Lý do hủy gói",
            PlaceholderText = "Nhập lý do hội viên yêu cầu hủy",
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 72
        };
        var error = new TextBlock { FontSize = 10, Foreground = Brush(FromHex(0xD75E55)), TextWrapping = TextWrapping.Wrap };
        var notice = new TextBlock
        {
            Text = "Gói chỉ bị hủy cho hội viên này; danh mục gói vẫn được giữ nguyên. Thao tác không khóa tài khoản đăng nhập và không tự động hoàn tiền.",
            FontSize = 10,
            Foreground = Brush(Muted),
            TextWrapping = TextWrapping.Wrap
        };
        var content = new StackPanel { Spacing = 10, Children = { notice, reason, error } };
        var dialog = new ContentDialog
        {
            Title = $"Xác nhận hủy gói · {member.FullName}",
            Content = content,
            PrimaryButtonText = "Xác nhận hủy gói",
            CloseButtonText = "Quay lại",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = RootGrid.XamlRoot
        };
        dialog.PrimaryButtonClick += (_, args) =>
        {
            if (string.IsNullOrWhiteSpace(reason.Text))
            {
                error.Text = "Vui lòng nhập lý do hủy gói.";
                args.Cancel = true;
            }
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            return;

        member.Status = "Đã hủy";
        member.CanceledDate = DateOnly.FromDateTime(DateTime.Today);
        member.CancellationReason = reason.Text.Trim();
        member.FreezeStart = null;
        member.FreezeEnd = null;
        member.FreezeReason = null;
        Render();
        await ShowMessageAsync("Đã hủy gói tập", $"Đã ghi nhận yêu cầu hủy của {member.FullName}. Lý do: {member.CancellationReason}");
    }

    private async Task ShowFreezeMembershipAsync(MemberEnrollment member, MembershipPackage package)
    {
        var startDate = new DatePicker { Header = "Ngày bắt đầu bảo lưu", Date = DateTimeOffset.Now.Date };
        var endDate = new DatePicker { Header = "Ngày kết thúc bảo lưu", Date = DateTimeOffset.Now.Date.AddDays(6) };
        var reason = new TextBox { Header = "Lý do bảo lưu", PlaceholderText = "Nhập lý do" };
        var maxDays = Math.Min(package.MaxFreezeDays, int.TryParse(_maxFreezeDays, out var configuredMax) ? configuredMax : package.MaxFreezeDays);
        var error = new TextBlock { FontSize = 10, Foreground = Brush(FromHex(0xD75E55)), TextWrapping = TextWrapping.Wrap };
        var note = new TextBlock
        {
            Text = $"Gói {package.Name} được bảo lưu tối đa {maxDays} ngày. Thời gian hợp lệ tính cả ngày bắt đầu và ngày kết thúc.",
            FontSize = 10,
            Foreground = Brush(Muted),
            TextWrapping = TextWrapping.Wrap
        };
        var form = new StackPanel { Spacing = 9, Children = { startDate, endDate, reason, note, error } };
        var dialog = new ContentDialog
        {
            Title = $"Bảo lưu gói · {member.FullName}",
            Content = new ScrollViewer { Content = form, MaxHeight = 540 },
            PrimaryButtonText = "Xác nhận bảo lưu",
            CloseButtonText = "Hủy",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = RootGrid.XamlRoot
        };
        dialog.PrimaryButtonClick += (_, args) =>
        {
            var start = DateOnly.FromDateTime(startDate.Date.DateTime);
            var end = DateOnly.FromDateTime(endDate.Date.DateTime);
            var durationDays = end.DayNumber - start.DayNumber + 1;
            if (start < DateOnly.FromDateTime(DateTime.Today)
                || end < start
                || durationDays > maxDays
                || string.IsNullOrWhiteSpace(reason.Text))
            {
                error.Text = durationDays > maxDays
                    ? $"Thời gian bảo lưu vượt quá giới hạn {maxDays} ngày của gói."
                    : "Chọn khoảng thời gian hợp lệ và nhập lý do bảo lưu.";
                args.Cancel = true;
            }
        };

        if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            return;

        member.Status = "Đang bảo lưu";
        member.FreezeStart = DateOnly.FromDateTime(startDate.Date.DateTime);
        member.FreezeEnd = DateOnly.FromDateTime(endDate.Date.DateTime);
        member.FreezeReason = reason.Text.Trim();
        Render();
        await ShowMessageAsync("Đã bảo lưu gói tập", $"{member.FullName} · {member.FreezeStart:dd/MM/yyyy} – {member.FreezeEnd:dd/MM/yyyy}\nLý do: {member.FreezeReason}");
    }

    private async Task ResumeMemberMembershipAsync(MemberEnrollment member)
    {
        var freezeDays = member.FreezeStart.HasValue && member.FreezeEnd.HasValue
            ? member.FreezeEnd.Value.DayNumber - member.FreezeStart.Value.DayNumber + 1
            : 0;
        var dialog = new ContentDialog
        {
            Title = "Tiếp tục gói tập",
            Content = $"Xác nhận kết thúc bảo lưu cho {member.FullName}? Thời hạn gói sẽ được cộng bù {freezeDays} ngày.",
            PrimaryButtonText = "Tiếp tục gói",
            CloseButtonText = "Hủy",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = RootGrid.XamlRoot
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            return;

        if (member.EndDate.HasValue && freezeDays > 0)
            member.EndDate = member.EndDate.Value.AddDays(freezeDays);
        member.Status = "Đang hoạt động";
        member.FreezeStart = null;
        member.FreezeEnd = null;
        member.FreezeReason = null;
        Render();
        await ShowMessageAsync("Gói tập đã tiếp tục", $"Đã cập nhật thời hạn gói cho {member.FullName}.");
    }

    private async Task ShowMemberRegistrationAsync()
    {
        var name = new TextBox { Header = "Họ và tên", PlaceholderText = "Nhập họ tên hội viên" };
        var phone = new TextBox { Header = "Số điện thoại", PlaceholderText = "0900 000 000" };
        var cccd = new TextBox { Header = "CCCD", PlaceholderText = "Số căn cước công dân" };
        var email = new TextBox { Header = "Email", PlaceholderText = "member@example.com" };
        var address = new TextBox { Header = "Địa chỉ", PlaceholderText = "Nhập địa chỉ hội viên" };
        var birthDate = new DatePicker { Header = "Ngày sinh", Date = DateTimeOffset.Now.Date };
        var startDate = new DatePicker { Header = "Ngày bắt đầu hiệu lực", Date = DateTimeOffset.Now.Date };
        var packagePicker = new ComboBox { Header = "Gói tập", HorizontalAlignment = HorizontalAlignment.Stretch };
        foreach (var package in _membershipPackages.Where(item => item.IsActive))
            packagePicker.Items.Add(package);
        packagePicker.DisplayMemberPath = nameof(MembershipPackage.Name);
        packagePicker.SelectedIndex = 0;

        var validityPreview = new Border
        {
            Background = Brush(GreenPale),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(12),
            Child = new TextBlock { FontSize = 10, Foreground = Brush(Ink), TextWrapping = TextWrapping.Wrap }
        };
        void UpdateValidityPreview()
        {
            if (packagePicker.SelectedItem is not MembershipPackage package)
                return;

            var start = DateOnly.FromDateTime(startDate.Date.DateTime);
            var end = CalculatePackageEndDate(package, start);
            ((TextBlock)validityPreview.Child).Text = end.HasValue
                ? $"Ngày bắt đầu: {start:dd/MM/yyyy}   ·   Ngày hết hạn: {end.Value:dd/MM/yyyy}\nThời hạn {package.Duration} được tính tự động theo gói."
                : $"Ngày bắt đầu: {start:dd/MM/yyyy}   ·   Thời hạn: {package.SessionCount} buổi\nGói tính theo số buổi, không hiển thị ngày hết hạn.";
        }
        startDate.DateChanged += (_, _) => UpdateValidityPreview();
        packagePicker.SelectionChanged += (_, _) => UpdateValidityPreview();
        UpdateValidityPreview();

        var error = new TextBlock { FontSize = 10, Foreground = Brush(FromHex(0xD75E55)), TextWrapping = TextWrapping.Wrap };
        var form = new StackPanel
        {
            Spacing = 8,
            Children = { name, phone, cccd, email, address, birthDate, packagePicker, startDate, validityPreview, error }
        };
        var dialog = new ContentDialog
        {
            Title = "Đăng ký hội viên mới",
            Content = new ScrollViewer { Content = form, MaxHeight = 620 },
            PrimaryButtonText = "Tạo hội viên",
            CloseButtonText = "Hủy",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = RootGrid.XamlRoot
        };
        dialog.PrimaryButtonClick += (_, args) =>
        {
            if (string.IsNullOrWhiteSpace(name.Text)
                || string.IsNullOrWhiteSpace(phone.Text)
                || string.IsNullOrWhiteSpace(cccd.Text)
                || string.IsNullOrWhiteSpace(email.Text)
                || !email.Text.Contains('@')
                || string.IsNullOrWhiteSpace(address.Text)
                || packagePicker.SelectedItem is not MembershipPackage)
            {
                error.Text = "Vui lòng nhập đủ thông tin, kiểm tra email và chọn gói tập.";
                args.Cancel = true;
                return;
            }

            if (_memberEnrollments.Any(existing =>
                existing.Phone.Replace(" ", "") == phone.Text.Trim().Replace(" ", "")
                || existing.CitizenId == cccd.Text.Trim()
                || existing.Email.Equals(email.Text.Trim(), StringComparison.OrdinalIgnoreCase))
                || _userAccounts.Any(account => account.Email.Equals(email.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                error.Text = "Số điện thoại, CCCD hoặc email đã tồn tại trong hệ thống.";
                args.Cancel = true;
                return;
            }
        };

        if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            return;

        var selectedPackage = (MembershipPackage)packagePicker.SelectedItem;
        var effectiveDate = DateOnly.FromDateTime(startDate.Date.DateTime);
        var expiryDate = CalculatePackageEndDate(selectedPackage, effectiveDate);
        var memberCode = $"MB-{_memberEnrollments.Count + 1:0000}";
        var status = effectiveDate > DateOnly.FromDateTime(DateTime.Today) ? "Chưa hiệu lực" : "Đang hoạt động";
        _memberEnrollments.Insert(0, new MemberEnrollment(
            name.Text.Trim(),
            memberCode,
            phone.Text.Trim(),
            cccd.Text.Trim(),
            selectedPackage.Name,
            DateOnly.FromDateTime(birthDate.Date.DateTime),
            expiryDate,
            status,
            email.Text.Trim(),
            address.Text.Trim()));
        Render();

        var validityMessage = expiryDate.HasValue
            ? $"Hiệu lực từ {effectiveDate:dd/MM/yyyy} đến {expiryDate.Value:dd/MM/yyyy}."
            : $"Hiệu lực từ {effectiveDate:dd/MM/yyyy}, áp dụng theo {selectedPackage.SessionCount} buổi của gói.";
        await ShowMessageAsync("Đăng ký thành công", $"Đã tạo mã hội viên {memberCode} cho {name.Text.Trim()}.\n{validityMessage}");
    }

    private static DateOnly? CalculatePackageEndDate(MembershipPackage package, DateOnly startDate)
    {
        var durationParts = package.Duration.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (durationParts.Length < 2 || !int.TryParse(durationParts[0], out var amount) || amount <= 0)
            return null;

        return durationParts[1].StartsWith("tháng", StringComparison.OrdinalIgnoreCase)
            ? startDate.AddMonths(amount).AddDays(-1)
            : durationParts[1].StartsWith("ngày", StringComparison.OrdinalIgnoreCase)
                ? startDate.AddDays(amount - 1)
                : null;
    }

    private void RenderCheckIn()
    {
        AddHeading("Quản lý Check-in", "Tra cứu hội viên hoặc xem minh họa quét mã QR", "Điểm danh hội viên");
        var columns = CreateGrid(2, 18);
        columns.ColumnDefinitions[0].Width = new GridLength(1.15, GridUnitType.Star);
        columns.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
        var checkIn = Panel("Tra cứu hội viên", "Nhập số điện thoại / mã hội viên hoặc mở màn hình QR minh họa");
        var lookup = new StackPanel { Spacing = 8, Margin = new Thickness(16, 8, 16, 16) };
        lookup.Children.Add(new TextBlock { Text = "Số điện thoại / Mã hội viên", FontSize = 10, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(Ink) });
        var lookupInput = new TextBox { PlaceholderText = "Ví dụ: 0901 234 567 hoặc MB-0248" };
        lookup.Children.Add(lookupInput);
        var lookupActions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var searchButton = AccentButton("Tìm hội viên");
        searchButton.HorizontalAlignment = HorizontalAlignment.Left;
        lookupActions.Children.Add(searchButton);
        var qrSampleButton = OutlineButton("▦  Quét mã QR");
        qrSampleButton.HorizontalAlignment = HorizontalAlignment.Left;
        lookupActions.Children.Add(qrSampleButton);
        lookup.Children.Add(lookupActions);
        var result = new Border
        {
            Background = Brush(FromHex(0xF7F9FB)),
            BorderBrush = Brush(Line),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(14),
            Visibility = Visibility.Collapsed
        };
        lookup.Children.Add(result);
        var confirmButton = AccentButton("Xác nhận vào phòng");
        confirmButton.Visibility = Visibility.Collapsed;
        confirmButton.HorizontalAlignment = HorizontalAlignment.Stretch;
        lookup.Children.Add(confirmButton);
        checkIn.Children.Add(lookup);

        var sampleMembers = new[]
        {
            (Id: "MB-0248", Name: "Nguyễn Minh Anh", Phone: "0901 234 567", Package: "Premium 12 tháng", Expiry: "18/04/2027", Status: "Đang hoạt động"),
            (Id: "MB-0247", Name: "Trần Quốc Bảo", Phone: "0903 456 789", Package: "Gym + PT 24 buổi", Expiry: "02/03/2027", Status: "Đang hoạt động"),
            (Id: "MB-0246", Name: "Lê Hoàng Nam", Phone: "0905 111 246", Package: "Standard 6 tháng", Expiry: "Đã hết hạn", Status: "Hết hạn"),
            (Id: "MB-0245", Name: "Phạm Thu Hà", Phone: "0908 222 345", Package: "Premium 12 tháng", Expiry: "Bảo lưu đến 28/10", Status: "Đang bảo lưu")
        };

        void SearchMember()
        {
            var query = lookupInput.Text.Trim();
            confirmButton.Visibility = Visibility.Collapsed;
            if (string.IsNullOrWhiteSpace(query))
            {
                result.Child = new TextBlock { Text = "Nhập số điện thoại hoặc mã hội viên để tra cứu.", FontSize = 10, Foreground = Brush(Muted), TextWrapping = TextWrapping.Wrap };
                result.Visibility = Visibility.Visible;
                return;
            }

            var member = sampleMembers.FirstOrDefault(item =>
                item.Id.Equals(query, StringComparison.OrdinalIgnoreCase)
                || item.Phone.Replace(" ", "").Equals(query.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));
            if (member == default)
            {
                result.Child = new TextBlock { Text = "Không tìm thấy hội viên phù hợp. Kiểm tra lại số điện thoại hoặc mã hội viên.", FontSize = 10, Foreground = Brush(FromHex(0xD75E55)), TextWrapping = TextWrapping.Wrap };
                result.Visibility = Visibility.Visible;
                return;
            }

            var enrollment = _memberEnrollments.FirstOrDefault(item => item.MemberCode == member.Id);
            var memberStatus = enrollment?.Status ?? member.Status;
            var eligible = memberStatus == "Đang hoạt động";
            var memberDetails = new StackPanel { Spacing = 7 };
            memberDetails.Children.Add(new TextBlock { Text = member.Name, FontSize = 15, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(Ink) });
            memberDetails.Children.Add(DataRow("Mã hội viên", member.Id, "Số điện thoại", member.Phone));
            memberDetails.Children.Add(DataRow("Gói tập", member.Package, "Hạn gói", member.Expiry));
            memberDetails.Children.Add(new Border
            {
                Background = Brush(eligible ? GreenPale : FromHex(0xFDEDEC)),
                CornerRadius = new CornerRadius(9),
                Padding = new Thickness(10, 7, 10, 7),
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = new TextBlock
                {
                    Text = eligible ? "ĐỦ ĐIỀU KIỆN VÀO PHÒNG" : $"KHÔNG THỂ CHECK-IN · {memberStatus.ToUpperInvariant()}",
                    FontSize = 9,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = Brush(eligible ? Green : FromHex(0xD75E55))
                }
            });
            if (!eligible)
                memberDetails.Children.Add(new TextBlock { Text = "Hội viên cần liên hệ lễ tân để được hỗ trợ về gói tập.", FontSize = 10, Foreground = Brush(Muted), TextWrapping = TextWrapping.Wrap });
            result.Child = memberDetails;
            result.Visibility = Visibility.Visible;
            confirmButton.Visibility = eligible ? Visibility.Visible : Visibility.Collapsed;
            confirmButton.Tag = (member.Name, member.Id);
        }

        searchButton.Click += (_, _) => SearchMember();
        qrSampleButton.Click += async (_, _) =>
        {
            var qrPreview = new Border
            {
                Height = 190,
                Background = Brush(FromHex(0x172033)),
                CornerRadius = new CornerRadius(14),
                Child = new StackPanel
                {
                    Spacing = 8,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        new TextBlock { Text = "▦", FontSize = 62, Foreground = Brush(White), HorizontalAlignment = HorizontalAlignment.Center },
                        new TextBlock { Text = "VÙNG QUÉT MÃ QR", FontSize = 10, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(FromHex(0xC6D4E6)), HorizontalAlignment = HorizontalAlignment.Center }
                    }
                }
            };
            var dialog = new ContentDialog
            {
                Title = "Quét mã QR hội viên",
                Content = new StackPanel
                {
                    Spacing = 12,
                    Children =
                    {
                        qrPreview,
                        new TextBlock
                        {
                            Text = "Giao diện minh họa cho chức năng quét. Bản hard-code không mở camera và không đọc mã QR thật.",
                            FontSize = 10,
                            Foreground = Brush(Muted),
                            TextWrapping = TextWrapping.Wrap
                        }
                    }
                },
                PrimaryButtonText = "Mô phỏng quét thành công",
                CloseButtonText = "Đóng",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = RootGrid.XamlRoot
            };
            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
                return;

            lookupInput.Text = "MB-0248";
            SearchMember();
        };
        lookupInput.KeyDown += (_, args) =>
        {
            if (args.Key == Windows.System.VirtualKey.Enter)
                SearchMember();
        };
        confirmButton.Click += async (_, _) =>
        {
            if (confirmButton.Tag is ValueTuple<string, string> member)
                await ShowMessageAsync("Check-in thành công", $"Đã ghi nhận lượt vào phòng cho {member.Item1} ({member.Item2}). Đây là thao tác minh họa bằng dữ liệu hard-code.");
        };
        columns.Children.Add(checkIn);

        var recent = Panel("Lượt check-in gần đây", "Hiện có 42 hội viên trong phòng");
        recent.Children.Add(DataRow("Nguyễn Minh Anh", "09:42", "Thành công", Green));
        recent.Children.Add(DataRow("Trần Quốc Bảo", "09:38", "Thành công", Green));
        recent.Children.Add(DataRow("Lê Hoàng Nam", "09:31", "Thành công", Green));
        recent.Children.Add(DataRow("Phạm Thu Hà", "09:16", "Gói đang bảo lưu", FromHex(0xE47A72)));
        columns.Children.Add(recent);
        Grid.SetColumn(recent, 1);
        _pageHost.Children.Add(columns);
    }

    private void RenderAdminSchedule()
    {
        AddHeading("PT & lịch dạy", "Quản lý huấn luyện viên, phân công và lịch tập", "Điều phối ca tập");
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        actions.Children.Add(AccentButton("+  Đặt lịch tập"));
        actions.Children.Add(OutlineButton("Phân công PT"));
        actions.Children.Add(OutlineButton("Danh sách huấn luyện viên"));
        _pageHost.Children.Add(actions);
        var panel = Panel("Lịch trong ngày", "Thứ Bảy, 17/10/2026  ·  24 ca tập");
        panel.Children.Add(RowHeader("THỜI GIAN", "HUẤN LUYỆN VIÊN", "HỘI VIÊN", "TRẠNG THÁI"));
        panel.Children.Add(DataRow("09:00 – 10:00", "Nguyễn Quốc Bảo", "Minh Anh", "Hoàn thành", Green));
        panel.Children.Add(DataRow("10:00 – 11:00", "Trần Thu Trang", "Hoàng Nam", "Đang diễn ra", FromHex(0x5785F5)));
        panel.Children.Add(DataRow("10:30 – 11:30", "Lê Minh Khang", "Gia Huy", "Đã xác nhận", Green));
        panel.Children.Add(DataRow("11:00 – 12:00", "Nguyễn Quốc Bảo", "Thu Hà", "Chờ xác nhận", FromHex(0xE7A943)));
        _pageHost.Children.Add(panel);
        var trainers = Panel("Đội ngũ huấn luyện viên", "Thông tin tổng quan");
        trainers.Children.Add(DataRow("Nguyễn Quốc Bảo  ·  Gym / Strength", "4,9 ★", "12 học viên", Green));
        trainers.Children.Add(DataRow("Trần Thu Trang  ·  Yoga / Fitness", "4,8 ★", "9 học viên", Green));
        trainers.Children.Add(DataRow("Lê Minh Khang  ·  Boxing / Gym", "4,7 ★", "11 học viên", Green));
        _pageHost.Children.Add(trainers);
    }

    private void RenderPayments()
    {
        AddHeading("Thanh toán & hóa đơn", "Lập hóa đơn, tiếp nhận thanh toán và tra cứu giao dịch", "Quầy thanh toán");
        var stats = CreateGrid(3, 14);
        stats.Children.Add(MetricCard("Doanh thu hôm nay", "18.450.000 ₫", "+8,4%", "₫", Green));
        stats.Children.Add(MetricCard("Hóa đơn đã thanh toán", "32", "Hôm nay", "▤", FromHex(0x5785F5)));
        stats.Children.Add(MetricCard("Chờ thanh toán", "3", "Cần đối soát", "◷", FromHex(0xE7A943)));
        _pageHost.Children.Add(stats);
        var bar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        bar.Children.Add(new TextBox { PlaceholderText = "Tìm mã hóa đơn / SĐT hội viên", Width = 340 });
        bar.Children.Add(AccentButton("+  Tạo hóa đơn"));
        bar.Children.Add(OutlineButton("Xuất báo cáo"));
        _pageHost.Children.Add(bar);
        var invoices = Panel("Giao dịch gần đây", "UC05 · Thu tiền, in lại và hoàn tiền");
        invoices.Children.Add(RowHeader("MÃ HÓA ĐƠN", "HỘI VIÊN", "THỜI GIAN", "TỔNG TIỀN", "TRẠNG THÁI"));
        invoices.Children.Add(DataRow("INV-10248", "Nguyễn Minh Anh", "09:42", "2.500.000 ₫", "Đã thanh toán", Green));
        invoices.Children.Add(DataRow("INV-10247", "Trần Quốc Bảo", "09:15", "1.200.000 ₫", "Đã thanh toán", Green));
        invoices.Children.Add(DataRow("INV-10246", "Lê Hoàng Nam", "08:58", "3.600.000 ₫", "Chờ thanh toán", FromHex(0xE7A943)));
        _pageHost.Children.Add(invoices);
    }

    private void RenderReports()
    {
        AddHeading("Báo cáo & thống kê", "Theo dõi hiệu quả hoạt động và doanh thu phòng tập", "Bức tranh kinh doanh");
        var filters = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        filters.Children.Add(OutlineButton("Tháng này  ▾"));
        filters.Children.Add(OutlineButton("01/10/2026 – 31/10/2026"));
        filters.Children.Add(OutlineButton("Xuất Excel"));
        filters.Children.Add(OutlineButton("Xuất PDF"));
        _pageHost.Children.Add(filters);
        var stats = CreateGrid(4, 14);
        stats.Children.Add(MetricCard("Tổng doanh thu", "248,5 tr", "+12,5%", "₫", Green));
        stats.Children.Add(MetricCard("Hội viên mới", "86", "+6,1%", "♙", FromHex(0x5785F5)));
        stats.Children.Add(MetricCard("Lượt check-in", "4.826", "+10,2%", "↗", FromHex(0xF2A64A)));
        stats.Children.Add(MetricCard("Buổi PT hoàn thành", "412", "+4,8%", "◷", FromHex(0x9B73E8)));
        _pageHost.Children.Add(stats);
        var report = Panel("Doanh thu theo ngày", "Tháng 10 · Dữ liệu minh họa");
        report.Children.Add(RevenueChart());
        _pageHost.Children.Add(report);
        var breakdown = Panel("Tổng hợp theo dịch vụ", "Doanh thu trong khoảng thời gian đã chọn");
        breakdown.Children.Add(DataRow("Gói tập hội viên", "154.200.000 ₫", "62%", Green));
        breakdown.Children.Add(DataRow("Huấn luyện cá nhân", "72.500.000 ₫", "29%", FromHex(0x5785F5)));
        breakdown.Children.Add(DataRow("Phụ kiện & dịch vụ", "21.800.000 ₫", "9%", FromHex(0xF2A64A)));
        _pageHost.Children.Add(breakdown);
    }

    private void RenderSettings()
    {
        AddHeading("Cài đặt hệ thống", "Quản lý tài khoản, danh mục gói tập và tham số vận hành · UC07", "Quản trị hệ thống");
        var sectionTabs = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        foreach (var section in new[] { "Quản lý tài khoản", "Danh mục gói tập", "Tham số vận hành" })
        {
            var selected = _settingsSection == section;
            var button = new Button
            {
                Content = section,
                Background = Brush(selected ? Navy : White),
                Foreground = Brush(selected ? White : Ink),
                BorderBrush = Brush(selected ? Navy : Line),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(16, 10, 16, 10)
            };
            button.Click += (_, _) =>
            {
                _settingsSection = section;
                Render();
            };
            sectionTabs.Children.Add(button);
        }
        _pageHost.Children.Add(sectionTabs);

        switch (_settingsSection)
        {
            case "Danh mục gói tập":
                RenderPackageCatalog();
                break;
            case "Tham số vận hành":
                RenderOperationalSettings();
                break;
            default:
                RenderUserAccounts();
                break;
        }
    }

    private void RenderUserAccounts()
    {
        var activeCount = _userAccounts.Count(account => account.IsActive);
        var accountStats = CreateGrid(4, 14);
        accountStats.Children.Add(MetricCard("Tổng tài khoản", _userAccounts.Count.ToString(), "Nhân viên và hội viên", "♙", FromHex(0x5785F5)));
        accountStats.Children.Add(MetricCard("Tài khoản nhân viên", _userAccounts.Count(account => account.AccountType == "Nhân viên").ToString(), "Admin, Lễ tân, PT", "▣", FromHex(0x5785F5)));
        accountStats.Children.Add(MetricCard("Tài khoản hội viên", _userAccounts.Count(account => account.AccountType == "Hội viên").ToString(), "Đăng nhập ứng dụng hội viên", "♙", Green));
        accountStats.Children.Add(MetricCard("Đang khóa", (_userAccounts.Count - activeCount).ToString(), "Không thể đăng nhập", "⊘", FromHex(0xE47A72)));
        _pageHost.Children.Add(accountStats);

        var header = new Grid { ColumnSpacing = 12 };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var title = new StackPanel { Spacing = 4, VerticalAlignment = VerticalAlignment.Center };
        title.Children.Add(new TextBlock { Text = "Tài khoản hệ thống", FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(Ink) });
        title.Children.Add(new TextBlock { Text = "Quản lý đăng nhập của nhân viên và hội viên; khóa tài khoản không làm khóa gói tập", FontSize = 11, Foreground = Brush(Muted) });
        header.Children.Add(title);
        var addButton = AccentButton("+  Tạo tài khoản");
        addButton.Click += async (_, _) => await ShowUserAccountEditorAsync();
        Grid.SetColumn(addButton, 1);
        header.Children.Add(addButton);
        _pageHost.Children.Add(header);

        var filters = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        var search = new TextBox { PlaceholderText = "Tìm theo họ tên, email hoặc tên đăng nhập...", Width = 300 };
        search.TextChanged += (_, _) => UpdateAccountsTable(search.Text);
        filters.Children.Add(search);
        var typeFilter = new ComboBox { Width = 170, PlaceholderText = "Loại tài khoản" };
        foreach (var type in new[] { "Tất cả loại tài khoản", "Nhân viên", "Hội viên" })
            typeFilter.Items.Add(type);
        typeFilter.SelectedItem = _accountTypeFilter;
        typeFilter.SelectionChanged += (_, _) =>
        {
            _accountTypeFilter = typeFilter.SelectedItem?.ToString() ?? "Tất cả loại tài khoản";
            UpdateAccountsTable(search.Text);
        };
        filters.Children.Add(typeFilter);
        var roleFilter = new ComboBox { Width = 150, PlaceholderText = "Lọc theo vai trò" };
        foreach (var role in new[] { "Tất cả vai trò", "Admin", "Lễ tân", "PT", MemberRole })
            roleFilter.Items.Add(role);
        roleFilter.SelectedItem = _accountRoleFilter;
        roleFilter.SelectionChanged += (_, _) =>
        {
            _accountRoleFilter = roleFilter.SelectedItem?.ToString() ?? "Tất cả vai trò";
            UpdateAccountsTable(search.Text);
        };
        filters.Children.Add(roleFilter);
        var statusFilter = new ComboBox { Width = 170, PlaceholderText = "Lọc theo trạng thái" };
        foreach (var status in new[] { "Tất cả trạng thái", "Đang hoạt động", "Đang khóa" })
            statusFilter.Items.Add(status);
        statusFilter.SelectedItem = _accountStatusFilter;
        statusFilter.SelectionChanged += (_, _) =>
        {
            _accountStatusFilter = statusFilter.SelectedItem?.ToString() ?? "Tất cả trạng thái";
            UpdateAccountsTable(search.Text);
        };
        filters.Children.Add(statusFilter);
        _pageHost.Children.Add(filters);

        var table = Panel("Danh sách tài khoản", "Email và tên đăng nhập phải là duy nhất · Khóa tài khoản sẽ chặn đăng nhập nhưng không thay đổi trạng thái gói tập");
        var tableBody = new StackPanel { Spacing = 0 };
        _accountsTableBody = new StackPanel { Spacing = 0 };
        tableBody.Children.Add(UserAccountTableHeader());
        tableBody.Children.Add(_accountsTableBody);
        table.Children.Add(tableBody);
        UpdateAccountsTable(search.Text);
        _pageHost.Children.Add(table);

        var safety = new Border
        {
            Background = Brush(FromHex(0xFFF7E8)),
            BorderBrush = Brush(FromHex(0xF4D99A)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(11),
            Padding = new Thickness(14),
            Child = new TextBlock
            {
                Text = "Lưu ý: khóa tài khoản hội viên chỉ ngăn đăng nhập; việc khóa hoặc hủy gói tập được quản lý riêng trong Hội viên & gói tập. Không thể tự khóa hoặc tự đổi vai trò tài khoản Admin đang đăng nhập.",
                FontSize = 11,
                Foreground = Brush(FromHex(0x8C6414)),
                TextWrapping = TextWrapping.Wrap
            }
        };
        _pageHost.Children.Add(safety);
    }

    private void UpdateAccountsTable(string searchText)
    {
        var body = _accountsTableBody;
        if (body is null)
            return;

        body.Children.Clear();
        var search = searchText.Trim();
        var filtered = _userAccounts.Where(account =>
            (string.IsNullOrEmpty(search)
                || account.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || account.Email.Contains(search, StringComparison.OrdinalIgnoreCase)
                || account.Username.Contains(search, StringComparison.OrdinalIgnoreCase))
            && (_accountTypeFilter == "Tất cả loại tài khoản" || account.AccountType == _accountTypeFilter)
            && (_accountRoleFilter == "Tất cả vai trò" || account.Role == _accountRoleFilter)
            && (_accountStatusFilter == "Tất cả trạng thái"
                || (_accountStatusFilter == "Đang hoạt động" && account.IsActive)
                || (_accountStatusFilter == "Đang khóa" && !account.IsActive)));

        foreach (var account in filtered)
            body.Children.Add(UserAccountRow(account));
        if (!filtered.Any())
            body.Children.Add(new TextBlock { Text = "Không tìm thấy tài khoản phù hợp.", FontSize = 11, Foreground = Brush(Muted), Margin = new Thickness(16) });
    }

    private static Grid UserAccountTableHeader()
    {
        var header = new Grid { Background = Brush(FromHex(0xF8FAFC)), Padding = new Thickness(16, 11, 16, 11) };
        var widths = new[] { 1.45, 1.6, 1.0, 0.9, 0.9, 0.9, 1.5 };
        foreach (var width in widths)
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(width, GridUnitType.Star) });
        var labels = new[] { "TÀI KHOẢN", "EMAIL", "TÊN ĐĂNG NHẬP", "LOẠI", "VAI TRÒ", "TRẠNG THÁI", "THAO TÁC" };
        for (var index = 0; index < labels.Length; index++)
        {
            var text = new TextBlock { Text = labels[index], FontSize = 8, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(Muted), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(text, index);
            header.Children.Add(text);
        }
        return header;
    }

    private UIElement UserAccountRow(UserAccount account)
    {
        var row = new Grid { Padding = new Thickness(16, 11, 16, 11), BorderBrush = Brush(Line), BorderThickness = new Thickness(0, 0, 0, 1) };
        var widths = new[] { 1.45, 1.6, 1.0, 0.9, 0.9, 0.9, 1.5 };
        foreach (var width in widths)
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(width, GridUnitType.Star) });

        var name = new StackPanel { Spacing = 3 };
        name.Children.Add(new TextBlock { Text = account.FullName, FontSize = 10, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(Ink), TextTrimming = TextTrimming.CharacterEllipsis });
        name.Children.Add(new TextBlock { Text = account.IsCurrentUser ? "Tài khoản đang đăng nhập" : account.Phone, FontSize = 8, Foreground = Brush(Muted), TextTrimming = TextTrimming.CharacterEllipsis });
        Grid.SetColumn(name, 0);
        row.Children.Add(name);
        AddStaffCell(row, account.Email, 1);
        AddStaffCell(row, account.Username, 2);
        AddStaffCell(row, account.AccountType, 3);
        AddStaffCell(row, account.Role, 4);

        var status = new Border
        {
            Background = Brush(account.IsActive ? GreenPale : FromHex(0xFDEDEC)),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(8, 5, 8, 5),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                Text = account.IsActive ? "Hoạt động" : "Đang khóa",
                FontSize = 8,
                Foreground = Brush(account.IsActive ? Green : FromHex(0xD75E55))
            }
        };
        Grid.SetColumn(status, 5);
        row.Children.Add(status);

        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5, VerticalAlignment = VerticalAlignment.Center };
        var edit = SmallActionButton("Sửa");
        edit.Click += async (_, _) => await ShowUserAccountEditorAsync(account);
        actions.Children.Add(edit);
        var toggle = SmallActionButton(account.IsActive ? "Khóa" : "Mở khóa");
        toggle.IsEnabled = !account.IsCurrentUser;
        toggle.Opacity = account.IsCurrentUser ? 0.45 : 1;
        toggle.Click += async (_, _) => await ToggleUserAccountStatusAsync(account);
        actions.Children.Add(toggle);
        Grid.SetColumn(actions, 6);
        row.Children.Add(actions);
        return row;
    }

    private static void AddStaffCell(Grid row, string value, int column)
    {
        var text = new TextBlock
        {
            Text = value,
            FontSize = 9,
            Foreground = Brush(Muted),
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        Grid.SetColumn(text, column);
        row.Children.Add(text);
    }

    private async Task ShowUserAccountEditorAsync(UserAccount? account = null)
    {
        var accountType = new ComboBox { Header = "Loại tài khoản", HorizontalAlignment = HorizontalAlignment.Stretch };
        foreach (var type in new[] { "Nhân viên", "Hội viên" })
            accountType.Items.Add(type);
        accountType.SelectedItem = account?.AccountType ?? "Nhân viên";
        accountType.IsEnabled = account is null;
        var name = new TextBox { Header = "Họ và tên", PlaceholderText = "Nhập họ tên", Text = account?.FullName ?? "" };
        var email = new TextBox { Header = "Email", PlaceholderText = "name@example.com", Text = account?.Email ?? "" };
        var phone = new TextBox { Header = "Số điện thoại", PlaceholderText = "0900 000 000", Text = account?.Phone ?? "" };
        var username = new TextBox { Header = "Tên đăng nhập", PlaceholderText = "username", Text = account?.Username ?? "" };
        var memberPicker = new ComboBox { Header = "Hồ sơ hội viên", HorizontalAlignment = HorizontalAlignment.Stretch, IsEnabled = account is null };
        foreach (var member in _memberEnrollments.Where(member =>
            account?.MemberCode == member.MemberCode
            || !_userAccounts.Any(existing => existing.MemberCode == member.MemberCode)))
        {
            memberPicker.Items.Add(new ComboBoxItem { Content = $"{member.FullName} · {member.MemberCode}", Tag = member });
        }
        if (account?.MemberCode is not null)
            memberPicker.SelectedItem = memberPicker.Items.Cast<ComboBoxItem>().FirstOrDefault(item => (item.Tag as MemberEnrollment)?.MemberCode == account.MemberCode);
        var memberPickerLabel = new StackPanel { Children = { memberPicker } };
        memberPickerLabel.Visibility = accountType.SelectedItem?.ToString() == "Hội viên" ? Visibility.Visible : Visibility.Collapsed;
        if (memberPicker.SelectedItem is ComboBoxItem selectedMemberItem && selectedMemberItem.Tag is MemberEnrollment selectedMember)
        {
            if (account is null)
            {
                name.Text = selectedMember.FullName;
                phone.Text = selectedMember.Phone;
            }
        }
        name.IsReadOnly = accountType.SelectedItem?.ToString() == "Hội viên";
        phone.IsReadOnly = accountType.SelectedItem?.ToString() == "Hội viên";
        memberPicker.SelectionChanged += (_, _) =>
        {
            if (memberPicker.SelectedItem is ComboBoxItem item && item.Tag is MemberEnrollment member)
            {
                name.Text = member.FullName;
                phone.Text = member.Phone;
            }
        };
        var role = new ComboBox { Header = "Vai trò", HorizontalAlignment = HorizontalAlignment.Stretch };
        void UpdateRoleOptions()
        {
            role.Items.Clear();
            if (accountType.SelectedItem?.ToString() == "Hội viên")
            {
                memberPickerLabel.Visibility = Visibility.Visible;
                name.IsReadOnly = true;
                phone.IsReadOnly = true;
                role.Items.Add(MemberRole);
                role.SelectedItem = MemberRole;
                role.IsEnabled = false;
                return;
            }

            memberPickerLabel.Visibility = Visibility.Collapsed;
            name.IsReadOnly = false;
            phone.IsReadOnly = false;
            foreach (var item in new[] { "Admin", "Lễ tân", "PT" })
                role.Items.Add(item);
            role.SelectedItem = account?.Role is "Admin" or "Lễ tân" or "PT" ? account.Role : "Lễ tân";
            role.IsEnabled = account?.IsCurrentUser != true;
        };
        accountType.SelectionChanged += (_, _) => UpdateRoleOptions();
        UpdateRoleOptions();
        var error = new TextBlock { Foreground = Brush(FromHex(0xD75E55)), FontSize = 10, TextWrapping = TextWrapping.Wrap };
        var form = new StackPanel { Spacing = 8, Children = { accountType, memberPickerLabel, name, email, phone, username, role, error } };
        var dialog = new ContentDialog
        {
            Title = account is null ? "Tạo tài khoản hệ thống" : "Cập nhật tài khoản",
            Content = new ScrollViewer { Content = form, MaxHeight = 520 },
            PrimaryButtonText = account is null ? "Tạo tài khoản" : "Lưu thông tin",
            CloseButtonText = "Hủy",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = RootGrid.XamlRoot
        };
        dialog.PrimaryButtonClick += (_, args) =>
        {
            var duplicate = _userAccounts.FirstOrDefault(existing => !ReferenceEquals(existing, account)
                && (existing.Email.Equals(email.Text.Trim(), StringComparison.OrdinalIgnoreCase)
                    || existing.Username.Equals(username.Text.Trim(), StringComparison.OrdinalIgnoreCase)));
            if (string.IsNullOrWhiteSpace(name.Text)
                || string.IsNullOrWhiteSpace(phone.Text)
                || string.IsNullOrWhiteSpace(email.Text)
                || !email.Text.Contains('@')
                || string.IsNullOrWhiteSpace(username.Text)
                || (accountType.SelectedItem?.ToString() == "Hội viên" && memberPicker.SelectedItem is not ComboBoxItem { Tag: MemberEnrollment })
                || duplicate is not null)
            {
                error.Text = duplicate is not null
                    ? "Email hoặc tên đăng nhập đã được sử dụng."
                    : accountType.SelectedItem?.ToString() == "Hội viên" && memberPicker.SelectedItem is not ComboBoxItem { Tag: MemberEnrollment }
                        ? "Chọn hồ sơ hội viên cần liên kết với tài khoản."
                        : "Vui lòng nhập đủ thông tin và kiểm tra định dạng email.";
                args.Cancel = true;
                return;
            }

            if (account is null)
            {
                var type = accountType.SelectedItem?.ToString() ?? "Nhân viên";
                var selectedRole = type == "Hội viên" ? MemberRole : role.SelectedItem?.ToString() ?? "Lễ tân";
                var memberCode = (memberPicker.SelectedItem as ComboBoxItem)?.Tag is MemberEnrollment selected ? selected.MemberCode : null;
                _userAccounts.Add(new UserAccount(name.Text.Trim(), email.Text.Trim(), phone.Text.Trim(), username.Text.Trim(), selectedRole, true, false, type, memberCode));
            }
            else
            {
                account.FullName = name.Text.Trim();
                account.Email = email.Text.Trim();
                account.Phone = phone.Text.Trim();
                account.Username = username.Text.Trim();
                if (!account.IsCurrentUser)
                    account.Role = role.SelectedItem?.ToString() ?? account.Role;
            }
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            Render();
    }

    private async Task ToggleUserAccountStatusAsync(UserAccount account)
    {
        if (account.IsCurrentUser)
        {
            await ShowMessageAsync("Không thể thay đổi tài khoản hiện tại", "Không thể tự khóa hoặc tự thay đổi vai trò tài khoản đang đăng nhập.");
            return;
        }

        var action = account.IsActive ? "khóa" : "mở khóa";
        var dialog = new ContentDialog
        {
            Title = $"Xác nhận {action} tài khoản",
            Content = $"Bạn có chắc muốn {action} tài khoản {account.FullName}?",
            PrimaryButtonText = "Xác nhận",
            CloseButtonText = "Hủy",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = RootGrid.XamlRoot
        };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            account.IsActive = !account.IsActive;
            Render();
        }
    }

    private void RenderPackageCatalog()
    {
        var summary = CreateGrid(3, 14);
        summary.Children.Add(MetricCard("Tổng danh mục", _membershipPackages.Count.ToString(), "Các gói tập được cấu hình", "▣", FromHex(0x5785F5)));
        summary.Children.Add(MetricCard("Đang kinh doanh", _membershipPackages.Count(package => package.IsActive).ToString(), "Có thể đăng ký mới", "✓", Green));
        summary.Children.Add(MetricCard("Đã ngừng", _membershipPackages.Count(package => !package.IsActive).ToString(), "Không bán mới", "⊘", FromHex(0xE7A943)));
        _pageHost.Children.Add(summary);

        var heading = new Grid();
        heading.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        heading.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var label = new StackPanel { Spacing = 4 };
        label.Children.Add(new TextBlock { Text = "Danh mục gói tập", FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(Ink) });
        label.Children.Add(new TextBlock { Text = "Cấu hình tên, đơn giá, thời hạn, số buổi, bảo lưu và quyền lợi", FontSize = 11, Foreground = Brush(Muted) });
        heading.Children.Add(label);
        var addPackage = AccentButton("+  Thêm gói tập");
        addPackage.Click += async (_, _) => await ShowPackageEditorAsync();
        Grid.SetColumn(addPackage, 1);
        heading.Children.Add(addPackage);
        _pageHost.Children.Add(heading);

        var cards = CreateGrid(2, 14);
        var rowNumber = 0;
        foreach (var package in _membershipPackages)
        {
            var card = new CardPanel();
            var content = new StackPanel { Spacing = 12, Margin = new Thickness(17) };
            var title = new Grid();
            title.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            title.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            title.Children.Add(new TextBlock { Text = package.Name, FontSize = 15, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(Ink), VerticalAlignment = VerticalAlignment.Center });
            var status = new Border
            {
                Background = Brush(package.IsActive ? GreenPale : FromHex(0xF1F2F4)),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8, 5, 8, 5),
                Child = new TextBlock { Text = package.IsActive ? "Đang kinh doanh" : "Ngừng kinh doanh", FontSize = 8, Foreground = Brush(package.IsActive ? Green : Muted) }
            };
            Grid.SetColumn(status, 1);
            title.Children.Add(status);
            content.Children.Add(title);
            content.Children.Add(new TextBlock { Text = package.Price.ToString("N0") + " ₫", FontSize = 22, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(Green) });
            content.Children.Add(DataRow("Thời hạn", package.Duration, "Số buổi tập", package.SessionCount == 0 ? "Không giới hạn" : package.SessionCount.ToString()));
            content.Children.Add(DataRow("Bảo lưu tối đa", package.MaxFreezeDays + " ngày", "Hội viên đang dùng", package.ActiveMemberCount.ToString()));
            content.Children.Add(new TextBlock { Text = "QUYỀN LỢI", FontSize = 8, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(Muted) });
            content.Children.Add(new TextBlock { Text = package.Benefits, FontSize = 10, Foreground = Brush(Ink), TextWrapping = TextWrapping.Wrap });
            var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            var edit = SmallActionButton("Chỉnh sửa");
            edit.Click += async (_, _) => await ShowPackageEditorAsync(package);
            actions.Children.Add(edit);
            var statusButton = SmallActionButton(package.IsActive ? "Ngừng kinh doanh" : "Mở kinh doanh");
            statusButton.Click += (_, _) =>
            {
                package.IsActive = !package.IsActive;
                Render();
            };
            actions.Children.Add(statusButton);
            var delete = SmallActionButton("Xóa");
            delete.Click += async (_, _) => await DeletePackageAsync(package);
            actions.Children.Add(delete);
            content.Children.Add(actions);
            card.Children.Add(content);
            Grid.SetRow(card, rowNumber / 2);
            Grid.SetColumn(card, rowNumber % 2);
            cards.Children.Add(card);
            rowNumber++;
        }
        for (var row = 0; row < (rowNumber + 1) / 2; row++)
            cards.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _pageHost.Children.Add(cards);
        _pageHost.Children.Add(new TextBlock
        {
            Text = "Nếu gói đang được hội viên sử dụng, không thể xóa. Hãy chuyển gói sang “Ngừng kinh doanh” để chặn đăng ký mới mà vẫn giữ quyền lợi cho hội viên hiện tại.",
            FontSize = 10,
            Foreground = Brush(FromHex(0x8C6414)),
            TextWrapping = TextWrapping.Wrap
        });
    }

    private async Task ShowPackageEditorAsync(MembershipPackage? package = null)
    {
        var name = new TextBox { Header = "Tên gói tập", Text = package?.Name ?? "", PlaceholderText = "Ví dụ: Premium Fitness" };
        var price = new TextBox { Header = "Đơn giá (VNĐ)", Text = package?.Price.ToString() ?? "", PlaceholderText = "6900000" };
        var duration = new TextBox { Header = "Thời hạn", Text = package?.Duration ?? "", PlaceholderText = "12 tháng hoặc 12 buổi" };
        var sessions = new TextBox { Header = "Số buổi tập", Text = package?.SessionCount.ToString() ?? "0" };
        var freeze = new TextBox { Header = "Số ngày bảo lưu tối đa", Text = package?.MaxFreezeDays.ToString() ?? "0" };
        var benefits = new TextBox { Header = "Mô tả quyền lợi", Text = package?.Benefits ?? "", AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 72 };
        var error = new TextBlock { FontSize = 10, Foreground = Brush(FromHex(0xD75E55)), TextWrapping = TextWrapping.Wrap };
        var form = new StackPanel { Spacing = 7, Children = { name, price, duration, sessions, freeze, benefits, error } };
        var dialog = new ContentDialog
        {
            Title = package is null ? "Thêm gói tập" : "Chỉnh sửa gói tập",
            Content = new ScrollViewer { Content = form, MaxHeight = 540 },
            PrimaryButtonText = "Lưu thay đổi",
            CloseButtonText = "Hủy",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = RootGrid.XamlRoot
        };
        dialog.PrimaryButtonClick += (_, args) =>
        {
            if (string.IsNullOrWhiteSpace(name.Text)
                || string.IsNullOrWhiteSpace(duration.Text)
                || string.IsNullOrWhiteSpace(benefits.Text)
                || !decimal.TryParse(price.Text, out var parsedPrice)
                || parsedPrice <= 0
                || !int.TryParse(sessions.Text, out var parsedSessions)
                || parsedSessions < 0
                || !int.TryParse(freeze.Text, out var parsedFreeze)
                || parsedFreeze < 0)
            {
                error.Text = "Vui lòng nhập đủ thông tin; đơn giá phải lớn hơn 0 và số buổi / ngày bảo lưu không được âm.";
                args.Cancel = true;
                return;
            }

            if (_membershipPackages.Any(existing => !ReferenceEquals(existing, package) && existing.Name.Equals(name.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                error.Text = "Tên gói tập đã tồn tại trong danh mục.";
                args.Cancel = true;
                return;
            }

            if (package is null)
                _membershipPackages.Add(new MembershipPackage(name.Text.Trim(), parsedPrice, duration.Text.Trim(), parsedSessions, parsedFreeze, benefits.Text.Trim(), 0, true));
            else
            {
                package.Name = name.Text.Trim();
                package.Price = parsedPrice;
                package.Duration = duration.Text.Trim();
                package.SessionCount = parsedSessions;
                package.MaxFreezeDays = parsedFreeze;
                package.Benefits = benefits.Text.Trim();
            }
        };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            Render();
    }

    private async Task DeletePackageAsync(MembershipPackage package)
    {
        if (package.ActiveMemberCount > 0)
        {
            await ShowMessageAsync("Không thể xóa gói tập", $"Gói “{package.Name}” đang có {package.ActiveMemberCount} hội viên sử dụng. Hãy chuyển trạng thái sang “Ngừng kinh doanh” để không nhận đăng ký mới.");
            return;
        }

        var confirm = new ContentDialog
        {
            Title = "Xóa gói tập",
            Content = $"Bạn có chắc muốn xóa gói “{package.Name}” khỏi danh mục?",
            PrimaryButtonText = "Xóa gói",
            CloseButtonText = "Hủy",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = RootGrid.XamlRoot
        };
        if (await confirm.ShowAsync() == ContentDialogResult.Primary)
        {
            _membershipPackages.Remove(package);
            Render();
        }
    }

    private void RenderOperationalSettings()
    {
        var intro = Panel("Tham số vận hành chung", "Thay đổi áp dụng ngay cho quy trình hội viên, check-in và đặt lịch PT · UC02 / UC03 / UC10");
        var form = new Grid { Margin = new Thickness(18, 0, 18, 8), ColumnSpacing = 18, RowSpacing = 14 };
        form.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        form.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        for (var index = 0; index < 4; index++)
            form.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var openTime = SettingInput(form, "Giờ mở cửa", _openingTime, 0, 0, "Định dạng 24 giờ (HH:mm)");
        var closeTime = SettingInput(form, "Giờ đóng cửa", _closingTime, 0, 1, "Phải sau giờ mở cửa");
        var cancellation = SettingInput(form, "Thời gian tối thiểu được hủy lịch PT", _cancellationHours, 1, 0, "Giờ trước khi ca tập bắt đầu");
        var freezeLimit = SettingInput(form, "Giới hạn bảo lưu thẻ tối đa", _maxFreezeDays, 1, 1, "Ngày bảo lưu tối đa trong năm");
        var duplicateScan = SettingInput(form, "Thời gian chống quét thẻ trùng", _duplicateScanSeconds, 2, 0, "Giây để chặn quẹt liên tiếp");
        var weekStart = new ComboBox { Header = "Ngày bắt đầu tuần", PlaceholderText = "Chọn ngày", SelectedIndex = 0 };
        foreach (var day in new[] { "Thứ Hai", "Chủ Nhật" })
            weekStart.Items.Add(day);
        Grid.SetRow(weekStart, 2);
        Grid.SetColumn(weekStart, 1);
        form.Children.Add(weekStart);

        intro.Children.Add(form);
        var error = new TextBlock { FontSize = 10, Foreground = Brush(FromHex(0xD75E55)), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(18, 0, 18, 0) };
        intro.Children.Add(error);
        var footer = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, Margin = new Thickness(18, 12, 18, 18) };
        var save = AccentButton("Lưu tham số");
        save.Click += async (_, _) =>
        {
            if (!TimeSpan.TryParse(openTime.Text, out var opening)
                || !TimeSpan.TryParse(closeTime.Text, out var closing)
                || closing <= opening
                || !int.TryParse(cancellation.Text, out var cancellationHours)
                || cancellationHours < 0
                || !int.TryParse(freezeLimit.Text, out var maxFreezeDays)
                || maxFreezeDays < 0
                || !int.TryParse(duplicateScan.Text, out var duplicateSeconds)
                || duplicateSeconds < 0)
            {
                error.Text = "Thông tin không hợp lệ. Kiểm tra định dạng giờ (HH:mm), giờ đóng cửa phải sau giờ mở cửa và các giới hạn phải là số nguyên không âm.";
                return;
            }

            error.Text = "";
            _openingTime = openTime.Text.Trim();
            _closingTime = closeTime.Text.Trim();
            _cancellationHours = cancellation.Text.Trim();
            _maxFreezeDays = freezeLimit.Text.Trim();
            _duplicateScanSeconds = duplicateScan.Text.Trim();
            await ShowMessageAsync("Đã lưu tham số", $"Giờ hoạt động {openTime.Text}–{closeTime.Text}; hủy lịch trước {cancellationHours} giờ; bảo lưu tối đa {maxFreezeDays} ngày; chống quét trùng {duplicateSeconds} giây. Đây là trạng thái mẫu trong phiên hiện tại.");
        };
        footer.Children.Add(save);
        footer.Children.Add(new TextBlock { Text = "Cấu hình áp dụng tức thì cho UC02, UC03 và UC10.", FontSize = 10, Foreground = Brush(Muted), VerticalAlignment = VerticalAlignment.Center });
        intro.Children.Add(footer);
        _pageHost.Children.Add(intro);
    }

    private static TextBox SettingInput(Grid parent, string header, string value, int row, int column, string description)
    {
        var field = new StackPanel { Spacing = 4 };
        field.Children.Add(new TextBox { Header = header, Text = value });
        field.Children.Add(new TextBlock { Text = description, FontSize = 9, Foreground = Brush(Muted) });
        Grid.SetRow(field, row);
        Grid.SetColumn(field, column);
        parent.Children.Add(field);
        return (TextBox)field.Children[0];
    }

    private async Task ShowMessageAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "Đóng",
            XamlRoot = RootGrid.XamlRoot
        };
        await dialog.ShowAsync();
    }

    private static Button SmallActionButton(string label) => new()
    {
        Content = label,
        Background = Brush(White),
        Foreground = Brush(Ink),
        BorderBrush = Brush(Line),
        BorderThickness = new Thickness(1),
        Padding = new Thickness(8, 5, 8, 5),
        FontSize = 9,
        VerticalAlignment = VerticalAlignment.Center
    };

    private void RenderTrainerSchedule()
    {
        AddHeading("Lịch dạy của tôi", "Theo dõi ca tập và đăng ký khung giờ rảnh", "Xin chào, Quốc Bảo 👋");
        var stats = CreateGrid(3, 14);
        stats.Children.Add(MetricCard("Ca tập hôm nay", "5", "2 ca tiếp theo", "◷", Green));
        stats.Children.Add(MetricCard("Học viên phụ trách", "12", "Đang hoạt động", "♙", FromHex(0x5785F5)));
        stats.Children.Add(MetricCard("Đánh giá trung bình", "4,9 / 5", "Từ 38 đánh giá", "★", FromHex(0xF2A64A)));
        _pageHost.Children.Add(stats);
        var schedule = Panel("Lịch hôm nay", "Thứ Bảy, 17/10/2026");
        schedule.Children.Add(DataRow("09:00 – 10:00", "Nguyễn Minh Anh", "Gym · Hoàn thành", Green));
        schedule.Children.Add(DataRow("10:30 – 11:30", "Lê Hoàng Nam", "Tăng cơ · Sắp tới", FromHex(0x5785F5)));
        schedule.Children.Add(DataRow("14:00 – 15:00", "Trần Gia Huy", "Sức bền · Đã xác nhận", Green));
        schedule.Children.Add(DataRow("16:00 – 17:00", "Phạm Thu Hà", "Gym · Đã xác nhận", Green));
        _pageHost.Children.Add(schedule);
        var availability = Panel("Lịch tuần & giờ rảnh", "Các khung màu xanh là ca đã nhận");
        availability.Children.Add(WeeklySchedule());
        availability.Children.Add(OutlineButton("Lưu lịch rảnh tuần tới"));
        _pageHost.Children.Add(availability);
    }

    private void RenderTrainerMembers()
    {
        AddHeading("Học viên của tôi", "Danh sách hội viên được phân công huấn luyện", "12 học viên đang theo dõi");
        var panel = Panel("Học viên phụ trách", "Theo dõi gói PT và lịch tập");
        panel.Children.Add(RowHeader("HỌC VIÊN", "GÓI PT", "BUỔI CÒN LẠI", "LỊCH TIẾP THEO"));
        panel.Children.Add(DataRow("Nguyễn Minh Anh", "Gym + PT 24 buổi", "18 / 24 buổi", "Hôm nay · 09:00", Green));
        panel.Children.Add(DataRow("Lê Hoàng Nam", "Gym + PT 12 buổi", "7 / 12 buổi", "Hôm nay · 10:30", Green));
        panel.Children.Add(DataRow("Trần Gia Huy", "Gym + PT 24 buổi", "12 / 24 buổi", "Hôm nay · 14:00", Green));
        panel.Children.Add(DataRow("Phạm Thu Hà", "Gym + PT 12 buổi", "4 / 12 buổi", "Hôm nay · 16:00", FromHex(0xE7A943)));
        _pageHost.Children.Add(panel);
        _pageHost.Children.Add(OutlineButton("Mở hồ sơ & cập nhật tiến độ"));
    }

    private void RenderTrainerProgress()
    {
        AddHeading("Tiến độ học viên", "Ghi nhận chỉ số thể hình và xác nhận buổi tập", "Theo dõi kết quả tập luyện");
        var progress = Panel("Cập nhật InBody · Nguyễn Minh Anh", "Lần đo gần nhất: 03/10/2026");
        progress.Children.Add(DataRow("Cân nặng", "62,4 kg", "Tỷ lệ mỡ", "22,1%"));
        progress.Children.Add(DataRow("Khối lượng cơ", "25,8 kg", "Ghi chú", "Tăng sức bền"));
        var inputs = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, Margin = new Thickness(0, 10, 0, 0) };
        inputs.Children.Add(new TextBox { PlaceholderText = "Cân nặng (kg)", Width = 150 });
        inputs.Children.Add(new TextBox { PlaceholderText = "Tỷ lệ mỡ (%)", Width = 150 });
        inputs.Children.Add(new TextBox { PlaceholderText = "Khối lượng cơ (kg)", Width = 180 });
        progress.Children.Add(inputs);
        progress.Children.Add(new TextBox { PlaceholderText = "Ghi chú bài tập...", Margin = new Thickness(0, 10, 0, 0) });
        progress.Children.Add(AccentButton("Lưu chỉ số InBody"));
        _pageHost.Children.Add(progress);
        var sessions = Panel("Ca tập cần xác nhận", "Chỉ xác nhận sau khi buổi tập đã diễn ra");
        sessions.Children.Add(DataRow("10:30 – Lê Hoàng Nam", "Hôm nay", "7 buổi còn lại", "Đang tập", FromHex(0x5785F5)));
        sessions.Children.Add(AccentButton("Xác nhận hoàn thành buổi tập"));
        _pageHost.Children.Add(sessions);
    }

    private void RenderMemberHome()
    {
        AddHeading("Tổng quan hội viên", "Theo dõi gói tập và lịch tập cá nhân", "Chào Minh Anh 👋");
        var membership = new Border
        {
            Background = Brush(Navy),
            CornerRadius = new CornerRadius(20),
            Padding = new Thickness(25),
            Child = new Grid()
        };
        var cardGrid = (Grid)membership.Child;
        cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var cardInfo = new StackPanel { Spacing = 9 };
        cardInfo.Children.Add(new TextBlock { Text = "MÃ HỘI VIÊN  ·  MB-0248", FontSize = 10, CharacterSpacing = 120, Foreground = Brush(FromHex(0xAAB7C9)) });
        cardInfo.Children.Add(new TextBlock { Text = "Premium Fitness · 12 tháng", FontSize = 25, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(White) });
        cardInfo.Children.Add(new TextBlock { Text = "Còn hiệu lực đến 18/04/2027  ·  Còn 183 ngày", FontSize = 12, Foreground = Brush(FromHex(0xCCD5E1)) });
        cardInfo.Children.Add(new Border { Background = Brush(FromHex(0x28453F)), CornerRadius = new CornerRadius(11), Padding = new Thickness(10, 6, 10, 6), HorizontalAlignment = HorizontalAlignment.Left, Child = new TextBlock { Text = "●  ĐANG HOẠT ĐỘNG", FontSize = 9, Foreground = Brush(FromHex(0x7BE0B9)) } });
        cardGrid.Children.Add(cardInfo);
        var qr = new Border
        {
            Width = 82,
            Height = 82,
            CornerRadius = new CornerRadius(12),
            Background = Brush(White),
            Child = new TextBlock { Text = "▦", FontSize = 46, Foreground = Brush(Navy), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }
        };
        Grid.SetColumn(qr, 1);
        cardGrid.Children.Add(qr);
        _pageHost.Children.Add(membership);

        var stats = CreateGrid(3, 14);
        stats.Children.Add(MetricCard("Buổi PT còn lại", "8 / 12", "Với PT Quốc Bảo", "◷", Green));
        stats.Children.Add(MetricCard("Lịch tập sắp tới", "18/10", "09:00 · PT Quốc Bảo", "▦", FromHex(0x5785F5)));
        stats.Children.Add(MetricCard("Check-in tháng này", "12 buổi", "Tiếp tục duy trì nhé!", "↗", FromHex(0xF2A64A)));
        _pageHost.Children.Add(stats);
        var coming = Panel("Lịch hẹn sắp tới", "Buổi tập cá nhân của bạn");
        coming.Children.Add(DataRow("Chủ nhật, 18 tháng 10  ·  09:00", "PT Nguyễn Quốc Bảo", "Gym · Buổi 5", "Đã xác nhận", Green));
        coming.Children.Add(OutlineButton("Xem lịch / Đặt buổi tập"));
        _pageHost.Children.Add(coming);
    }

    private void RenderMembership()
    {
        AddHeading("Gói tập của tôi", "Gói hội viên chính và dịch vụ PT bổ sung", "Gói đang hoạt động");
        var details = Panel("Premium Fitness · 12 tháng", "Gói hội viên chính · Mã hội viên MB-0248");
        details.Children.Add(DataRow("Ngày bắt đầu", "19/04/2026", "Ngày hết hạn", "18/04/2027"));
        details.Children.Add(DataRow("Thời hạn còn lại", "183 ngày", "Trạng thái gói tập", "Đang hoạt động", Green));
        details.Children.Add(DataRow("Dịch vụ PT bổ sung", "Đã dùng 4 buổi", "Còn lại", "8 / 12 buổi", Green));
        details.Children.Add(OutlineButton("Xem lịch sử gói tập"));
        _pageHost.Children.Add(details);
        var history = Panel("Lịch sử đăng ký dịch vụ", "Gói PT là dịch vụ bổ sung, không phải gói hội viên thứ hai");
        history.Children.Add(DataRow("Gói hội viên Premium Fitness · 12 tháng", "Bắt đầu 19/04/2026", "6.900.000 ₫", "Đã thanh toán", Green));
        history.Children.Add(DataRow("Dịch vụ PT bổ sung · 12 buổi", "Đăng ký 19/04/2026", "2.400.000 ₫", "Đã thanh toán", Green));
        _pageHost.Children.Add(history);
    }

    private void RenderBookings()
    {
        AddHeading("Đặt lịch với PT", "Chọn huấn luyện viên và khung giờ còn trống", "Lịch tập cá nhân");
        var pt = Panel("PT của bạn", "Huấn luyện viên được phân công");
        pt.Children.Add(DataRow("Nguyễn Quốc Bảo", "Gym · Strength · 4,9 ★", "12 học viên", "Đang hoạt động", Green));
        _pageHost.Children.Add(pt);
        var slots = Panel("Chọn lịch tập", "Tuần 19 – 25 tháng 10 · Khung xanh còn trống");
        slots.Children.Add(WeeklySchedule());
        slots.Children.Add(new TextBlock { Text = "Thứ Hai, 19/10 · 09:00 – 10:00", FontSize = 12, Foreground = Brush(Ink), Margin = new Thickness(0, 12, 0, 0) });
        slots.Children.Add(AccentButton("Gửi yêu cầu đặt lịch"));
        _pageHost.Children.Add(slots);
        var upcoming = Panel("Lịch hẹn của tôi", "Có thể đổi hoặc hủy theo chính sách trước giờ tập 2 tiếng");
        upcoming.Children.Add(DataRow("18/10 · 09:00", "PT Quốc Bảo", "Đã xác nhận", "Đổi lịch  ·  Hủy lịch", Green));
        _pageHost.Children.Add(upcoming);
    }

    private void RenderProfile()
    {
        AddHeading("Hồ sơ cá nhân", "Thông tin liên hệ và tài khoản hội viên", "Nguyễn Minh Anh");
        var profile = Panel("Thông tin cá nhân", "Cập nhật thông tin liên hệ của bạn");
        profile.Children.Add(DataRow("Họ và tên", "Nguyễn Minh Anh", "Mã hội viên", "MB-0248"));
        var form = new StackPanel { Spacing = 16, Margin = new Thickness(18, 8, 18, 18) };
        var phoneField = new StackPanel { Spacing = 5 };
        phoneField.Children.Add(new TextBlock { Text = "Số điện thoại", FontSize = 12, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(Ink) });
        var phone = new TextBox { Text = _memberPhone, PlaceholderText = "Nhập số điện thoại", HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brush(White), Foreground = Brush(Ink), BorderBrush = Brush(Line) };
        phoneField.Children.Add(phone);
        var emailField = new StackPanel { Spacing = 5 };
        emailField.Children.Add(new TextBlock { Text = "Email", FontSize = 12, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(Ink) });
        var email = new TextBox { Text = _memberEmail, PlaceholderText = "name@example.com", HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brush(White), Foreground = Brush(Ink), BorderBrush = Brush(Line) };
        emailField.Children.Add(email);
        var addressField = new StackPanel { Spacing = 5 };
        addressField.Children.Add(new TextBlock { Text = "Địa chỉ nhà", FontSize = 12, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(Ink) });
        var address = new TextBox { Text = _memberAddress, PlaceholderText = "Nhập địa chỉ nhà", HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brush(White), Foreground = Brush(Ink), BorderBrush = Brush(Line) };
        addressField.Children.Add(address);
        phone.TextChanged += (_, _) => _memberPhone = phone.Text;
        email.TextChanged += (_, _) => _memberEmail = email.Text;
        address.TextChanged += (_, _) => _memberAddress = address.Text;
        form.Children.Add(phoneField);
        form.Children.Add(emailField);
        form.Children.Add(addressField);
        var error = new TextBlock { FontSize = 10, Foreground = Brush(FromHex(0xD75E55)), TextWrapping = TextWrapping.Wrap };
        form.Children.Add(error);
        var save = AccentButton("Lưu thay đổi");
        save.HorizontalAlignment = HorizontalAlignment.Left;
        save.Click += async (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_memberPhone)
                || _memberPhone.Replace(" ", "").Length < 9
                || !_memberEmail.Contains('@')
                || string.IsNullOrWhiteSpace(_memberAddress))
            {
                error.Text = "Vui lòng nhập số điện thoại, email hợp lệ và địa chỉ.";
                return;
            }

            error.Text = "";
            await ShowMessageAsync("Đã lưu thông tin", "Thông tin cá nhân đã được cập nhật trong bản mẫu hiện tại.");
        };
        form.Children.Add(save);
        profile.Children.Add(form);
        _pageHost.Children.Add(profile);
    }

    private void AddHeading(string eyebrow, string subtitle, string title)
    {
        var header = new Grid();
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var texts = new StackPanel { Spacing = 5 };
        texts.Children.Add(new TextBlock { Text = title, FontSize = 25, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(Ink) });
        texts.Children.Add(new TextBlock { Text = $"{eyebrow}  ·  {subtitle}", FontSize = 11, Foreground = Brush(Muted) });
        header.Children.Add(texts);
        var notice = new Button
        {
            Content = "♧   Thông báo",
            Background = Brush(White),
            Foreground = Brush(Ink),
            BorderBrush = Brush(Line),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(14, 9, 14, 9),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(notice, 1);
        header.Children.Add(notice);
        _pageHost.Children.Add(header);
    }

    private static CardPanel Panel(string title, string subtitle)
    {
        var panel = new CardPanel();
        panel.Children.Add(new TextBlock { Text = title, FontSize = 15, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(Ink), Margin = new Thickness(18, 17, 18, 4) });
        panel.Children.Add(new TextBlock { Text = subtitle, FontSize = 10, Foreground = Brush(Muted), Margin = new Thickness(18, 0, 18, 13) });
        return panel;
    }

    private static Border MetricCard(string label, string value, string note, string glyph, Color accent)
    {
        var card = new StackPanel { Spacing = 11 };
        var top = new Grid();
        top.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        top.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        top.Children.Add(new TextBlock { Text = label, FontSize = 10, Foreground = Brush(Muted), VerticalAlignment = VerticalAlignment.Center });
        var icon = new Border
        {
            Width = 31,
            Height = 31,
            CornerRadius = new CornerRadius(10),
            Background = Brush(WithAlpha(accent, 28)),
            Child = new TextBlock { Text = glyph, FontSize = 15, Foreground = Brush(accent), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }
        };
        Grid.SetColumn(icon, 1);
        top.Children.Add(icon);
        card.Children.Add(top);
        card.Children.Add(new TextBlock { Text = value, FontSize = 23, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(Ink) });
        card.Children.Add(new TextBlock { Text = note, FontSize = 10, Foreground = Brush(accent) });
        return new Border
        {
            Background = Brush(White),
            BorderBrush = Brush(Line),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(16),
            Child = card
        };
    }

    private static Grid CreateGrid(int columns, double spacing)
    {
        var grid = new Grid { ColumnSpacing = spacing };
        for (var i = 0; i < columns; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }
        return grid;
    }

    private static UIElement RevenueChart()
    {
        var chart = new Grid { Height = 160, Margin = new Thickness(18, 14, 18, 16) };
        for (var i = 0; i < 7; i++)
            chart.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var values = new[] { 62, 96, 77, 125, 105, 145, 116 };
        var days = new[] { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };
        for (var i = 0; i < values.Length; i++)
        {
            var column = new StackPanel { VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Center, Spacing = 8 };
            column.Children.Add(new Border
            {
                Width = 28,
                Height = values[i],
                CornerRadius = new CornerRadius(7, 7, 2, 2),
                Background = Brush(i == 5 ? Green : FromHex(0xDDEFE9))
            });
            column.Children.Add(new TextBlock { Text = days[i], FontSize = 9, Foreground = Brush(Muted), HorizontalAlignment = HorizontalAlignment.Center });
            Grid.SetColumn(column, i);
            chart.Children.Add(column);
        }
        return chart;
    }

    private static UIElement WeeklySchedule()
    {
        var grid = CreateGrid(6, 5);
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var days = new[] { "GIỜ", "T2", "T3", "T4", "T5", "T6" };
        for (var i = 0; i < days.Length; i++)
        {
            var day = new TextBlock { Text = days[i], FontSize = 9, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(Muted), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 8, 0, 8) };
            Grid.SetColumn(day, i);
            grid.Children.Add(day);
        }

        var times = new[] { "09:00", "11:00", "14:00" };
        var available = new HashSet<(int Row, int Col)> { (0, 1), (0, 3), (1, 2), (1, 5), (2, 1), (2, 4) };
        for (var row = 0; row < times.Length; row++)
        {
            var time = new TextBlock { Text = times[row], FontSize = 9, Foreground = Brush(Muted), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            Grid.SetRow(time, row + 1);
            grid.Children.Add(time);
            for (var col = 1; col < 6; col++)
            {
                var isAvailable = available.Contains((row, col));
                var slot = new Border
                {
                    Height = 30,
                    Margin = new Thickness(2),
                    CornerRadius = new CornerRadius(6),
                    Background = Brush(isAvailable ? GreenPale : FromHex(0xF3F5F8)),
                    Child = new TextBlock
                    {
                        Text = isAvailable ? "Trống" : "—",
                        FontSize = 8,
                        Foreground = Brush(isAvailable ? Green : Muted),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                Grid.SetRow(slot, row + 1);
                Grid.SetColumn(slot, col);
                grid.Children.Add(slot);
            }
        }
        return grid;
    }

    private static UIElement RowHeader(params string[] labels)
    {
        var columns = Math.Max(labels.Length, 2);
        var row = new Grid { Background = Brush(FromHex(0xF8FAFC)), Padding = new Thickness(15, 10, 15, 10) };
        for (var i = 0; i < columns; i++)
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        for (var i = 0; i < labels.Length; i++)
        {
            var label = new TextBlock { Text = labels[i], FontSize = 8, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(Muted), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(label, i);
            row.Children.Add(label);
        }
        return row;
    }

    private static UIElement DataRow(string first, string second, string third, string fourth, Color statusColor)
        => DataRowCore(new[] { first, second, third, fourth }, statusColor);

    private static UIElement DataRow(string first, string second, string third, string fourth, string fifth, Color statusColor)
        => DataRowCore(new[] { first, second, third, fourth, fifth }, statusColor);

    private static UIElement DataRow(string first, string second, string third, Color statusColor)
        => DataRowCore(new[] { first, second, third }, statusColor);

    private static UIElement DataRow(string first, string second, string third, string fourth)
        => DataRowCore(new[] { first, second, third, fourth }, null);

    private static UIElement DataRowCore(string[] values, Color? statusColor)
    {
        var row = new Grid { Padding = new Thickness(15, 12, 15, 12), BorderBrush = Brush(Line), BorderThickness = new Thickness(0, 0, 0, 1) };
        for (var i = 0; i < values.Length; i++)
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        for (var i = 0; i < values.Length; i++)
        {
            var text = new TextBlock
            {
                Text = values[i],
                FontSize = 10,
                Foreground = statusColor.HasValue && i == values.Length - 1 ? Brush(statusColor.Value) : Brush(i == 0 ? Ink : Muted),
                FontWeight = i == 0 ? Microsoft.UI.Text.FontWeights.SemiBold : Microsoft.UI.Text.FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            Grid.SetColumn(text, i);
            row.Children.Add(text);
        }
        return row;
    }

    private static Button AccentButton(string label) => new()
    {
        Content = label,
        Background = Brush(Green),
        Foreground = Brush(White),
        BorderThickness = new Thickness(0),
        Padding = new Thickness(14, 9, 14, 9),
        VerticalAlignment = VerticalAlignment.Center
    };

    private static Button OutlineButton(string label) => new()
    {
        Content = label,
        Background = Brush(White),
        Foreground = Brush(Ink),
        BorderBrush = Brush(Line),
        BorderThickness = new Thickness(1),
        Padding = new Thickness(14, 9, 14, 9),
        VerticalAlignment = VerticalAlignment.Center
    };

    private static string NavGlyph(string item) => item switch
    {
        "Tổng quan" => "▦",
        "Lịch dạy" => "▦",
        "Quản lý hội viên & gói tập" => "♙",
        "Học viên" => "♙",
        "Check-in" => "↗",
        "PT & lịch dạy" => "◷",
        "Tiến độ" => "↗",
        "Thanh toán" => "₫",
        "Báo cáo" => "▤",
        "Cài đặt" => "⚙",
        "Gói tập" => "▣",
        "Đặt lịch với PT" => "◷",
        "Hồ sơ cá nhân" => "◎",
        _ => "•"
    };

    private static Color FromHex(uint hex) => Color.FromArgb(255, (byte)(hex >> 16), (byte)(hex >> 8), (byte)hex);

    private static Color WithAlpha(Color color, byte alpha) => Color.FromArgb(alpha, color.R, color.G, color.B);

    private static SolidColorBrush Brush(Color color) => new(color);

    private sealed class UserAccount(string fullName, string email, string phone, string username, string role, bool isActive, bool isCurrentUser, string accountType = "Nhân viên", string? memberCode = null)
    {
        public string FullName { get; set; } = fullName;
        public string Email { get; set; } = email;
        public string Phone { get; set; } = phone;
        public string Username { get; set; } = username;
        public string Role { get; set; } = role;
        public string AccountType { get; } = accountType;
        public string? MemberCode { get; } = memberCode;
        public bool IsActive { get; set; } = isActive;
        public bool IsCurrentUser { get; } = isCurrentUser;
    }

    private sealed class MembershipPackage(string name, decimal price, string duration, int sessionCount, int maxFreezeDays, string benefits, int activeMemberCount, bool isActive)
    {
        public string Name { get; set; } = name;
        public decimal Price { get; set; } = price;
        public string Duration { get; set; } = duration;
        public int SessionCount { get; set; } = sessionCount;
        public int MaxFreezeDays { get; set; } = maxFreezeDays;
        public string Benefits { get; set; } = benefits;
        public int ActiveMemberCount { get; } = activeMemberCount;
        public bool IsActive { get; set; } = isActive;
    }

    private sealed class MemberEnrollment(string fullName, string memberCode, string phone, string citizenId, string packageName, DateOnly birthDate, DateOnly? endDate, string status, string email = "", string address = "")
    {
        public string FullName { get; } = fullName;
        public string MemberCode { get; } = memberCode;
        public string Phone { get; set; } = phone;
        public string CitizenId { get; } = citizenId;
        public string PackageName { get; } = packageName;
        public DateOnly BirthDate { get; } = birthDate;
        public DateOnly? EndDate { get; set; } = endDate;
        public string Status { get; set; } = status;
        public string Email { get; set; } = email;
        public string Address { get; set; } = address;
        public DateOnly? FreezeStart { get; set; }
        public DateOnly? FreezeEnd { get; set; }
        public string? FreezeReason { get; set; }
        public DateOnly? CanceledDate { get; set; }
        public string? CancellationReason { get; set; }
    }

    private sealed class CardPanel : UserControl
    {
        private readonly StackPanel _content = new() { Spacing = 0 };

        public CardPanel()
        {
            Content = new Border
            {
                Background = Brush(White),
                BorderBrush = Brush(Line),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(15),
                Child = _content
            };
        }

        public UIElementCollection Children => _content.Children;
    }
}
