using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel
    {
        get
        {
            return (DashboardViewModel)DataContext;
        }
    }

    public DashboardPage()
    {
        DataContext = App.GetService<DashboardViewModel>();
        InitializeComponent();
        Loaded += async (s, e) =>
        {
            if (ViewModel != null)
            {
                await ViewModel.LoadDashboardCommand.ExecuteAsync(null);
            }
        };
    }

    private async void OnExportExcelClicked(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        try
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Excel Workbook (*.xlsx)", new List<string> { ".xlsx" });
            savePicker.SuggestedFileName = $"BaoCaoDoanhThu_{DateTime.Now:yyyyMMdd_HHmm}";

            var hwnd = App.WindowHandle;
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                var bytes = await ViewModel.GetExcelBytesAsync();
                await Windows.Storage.FileIO.WriteBytesAsync(file, bytes);

                var successDialog = new ContentDialog
                {
                    Title = "XUẤT BÁO CÁO THÀNH CÔNG",
                    Content = $"Đã lưu tệp báo cáo Excel thành công tại:\n\n{file.Path}",
                    CloseButtonText = "Đóng",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot
                };
                await successDialog.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            var errorDialog = new ContentDialog
            {
                Title = "LỖI XUẤT TỆP",
                Content = $"Không thể xuất tệp báo cáo Excel: {ex.Message}",
                CloseButtonText = "Đóng",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };
            await errorDialog.ShowAsync();
        }
    }

    private async void OnExportPdfClicked(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        try
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("PDF Document (*.pdf)", new List<string> { ".pdf" });
            savePicker.SuggestedFileName = $"BaoCaoDoanhThu_{DateTime.Now:yyyyMMdd_HHmm}";

            var hwnd = App.WindowHandle;
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                var bytes = await ViewModel.GetPdfBytesAsync();
                await Windows.Storage.FileIO.WriteBytesAsync(file, bytes);

                var successDialog = new ContentDialog
                {
                    Title = "XUẤT BÁO CÁO THÀNH CÔNG",
                    Content = $"Đã lưu tệp báo cáo PDF thành công tại:\n\n{file.Path}",
                    CloseButtonText = "Đóng",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot
                };
                await successDialog.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            var errorDialog = new ContentDialog
            {
                Title = "LỖI XUẤT TỆP",
                Content = $"Không thể xuất tệp báo cáo PDF: {ex.Message}",
                CloseButtonText = "Đóng",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };
            await errorDialog.ShowAsync();
        }
    }
}
