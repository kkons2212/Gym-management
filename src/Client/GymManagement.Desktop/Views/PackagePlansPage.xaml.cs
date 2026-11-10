using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.Dialogs;
using GymManagement.Desktop.Services.Api;
using GymManagement.Desktop.ViewModels;
using GymManagement.Server.Core.DTOs.Packages;

namespace GymManagement.Desktop.Views;

public sealed partial class PackagePlansPage : Page
{
    public PackagePlansViewModel ViewModel
    {
        get
        {
            return (PackagePlansViewModel)DataContext;
        }
    }

    public PackagePlansPage()
    {
        DataContext = App.GetService<PackagePlansViewModel>();
        InitializeComponent();
        Loaded += async (s, e) =>
        {
            if (ViewModel != null)
            {
                await ViewModel.LoadPackagesCommand.ExecuteAsync(null);
            }
        };
    }

    private async void OnEditPackageClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is PackagePlanDto plan && ViewModel != null)
        {
            var dialog = new PackagePlanEditDialog(App.GetService<IApiClient>(), plan)
            {
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
            if (dialog.SavedPlan != null)
            {
                await ViewModel.LoadPackagesCommand.ExecuteAsync(null);
            }
        }
    }

    private async void OnDeletePackageClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is PackagePlanDto plan && ViewModel != null)
        {
            var confirmDialog = new ContentDialog
            {
                Title = "XÁC NHẬN XÓA GÓI TẬP",
                Content = $"Bạn có chắc chắn muốn xóa gói tập '{plan.Name}' ({plan.PlanId}) khỏi hệ thống không?\n\nLưu ý: Nếu gói tập đã có hội viên đăng ký hoặc phát sinh hóa đơn, hệ thống sẽ tự động chuyển sang trạng thái 'Ngừng kinh doanh' thay vì xóa vật lý.",
                PrimaryButtonText = "Xác nhận xóa",
                CloseButtonText = "Hủy bỏ",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var res = await confirmDialog.ShowAsync();
            if (res == ContentDialogResult.Primary)
            {
                var result = await ViewModel.DeletePackageAsync(plan.PlanId);
                if (result != null && result.IsDiscontinued)
                {
                    var notice = new ContentDialog
                    {
                        Title = "THÔNG BÁO RÀNG BUỘC DỮ LIỆU",
                        Content = result.Message,
                        CloseButtonText = "Đã hiểu",
                        DefaultButton = ContentDialogButton.Close,
                        XamlRoot = this.XamlRoot
                    };
                    await notice.ShowAsync();
                }
            }
        }
    }
}
