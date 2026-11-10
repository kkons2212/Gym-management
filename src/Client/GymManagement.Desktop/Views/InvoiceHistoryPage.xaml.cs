using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.Dialogs;
using GymManagement.Desktop.ViewModels;
using GymManagement.Server.Core.DTOs.Billing;

namespace GymManagement.Desktop.Views;

public sealed partial class InvoiceHistoryPage : Page
{
    public InvoiceHistoryViewModel ViewModel
    {
        get
        {
            return (InvoiceHistoryViewModel)DataContext;
        }
    }

    public InvoiceHistoryPage()
    {
        DataContext = App.GetService<InvoiceHistoryViewModel>();
        InitializeComponent();
        Loaded += async (s, e) =>
        {
            if (ViewModel != null)
            {
                await ViewModel.InitializeAsync();
            }
        };
    }

    private static InvoiceDto? ExtractInvoiceFromSender(object sender)
    {
        if (sender is FrameworkElement fe)
        {
            if (fe.DataContext is InvoiceDto invoice)
            {
                return invoice;
            }
        }
        return null;
    }

    private async void OnSearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        await ViewModel.LoadInvoicesAsync();
    }

    private async void OnFilterClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string status)
        {
            ViewModel.SelectedStatus = status;
            await ViewModel.LoadInvoicesAsync();
        }
    }

    private async void OnRefreshClicked(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadInvoicesAsync();
    }

    private async void OnPrintReceiptClicked(object sender, RoutedEventArgs e)
    {
        var invoice = ExtractInvoiceFromSender(sender);
        if (invoice == null)
        {
            return;
        }

        var dialog = new ReceiptPreviewDialog(invoice)
        {
            XamlRoot = this.XamlRoot
        };

        await dialog.ShowAsync();
    }

    private async void OnRefundInvoiceClicked(object sender, RoutedEventArgs e)
    {
        var invoice = ExtractInvoiceFromSender(sender);
        if (invoice == null)
        {
            return;
        }

        if (invoice.Status == "REFUNDED")
        {
            var alert = new ContentDialog
            {
                Title = "THÔNG BÁO",
                Content = "Hóa đơn này đã được hoàn tiền trước đó, không thể tiếp tục hoàn tiền.",
                CloseButtonText = "Đóng",
                XamlRoot = this.XamlRoot
            };
            await alert.ShowAsync();
            return;
        }

        var dialog = new RefundInvoiceDialog(invoice.InvoiceCode, invoice.TotalAmount)
        {
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await ViewModel.ExecuteRefundAsync(dialog.InvoiceCode, dialog.RefundAmount, dialog.Reason);
        }
    }
}

