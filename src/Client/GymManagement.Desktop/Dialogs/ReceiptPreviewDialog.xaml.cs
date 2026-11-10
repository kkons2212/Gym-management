using Microsoft.UI.Xaml.Controls;
using GymManagement.Server.Core.DTOs.Billing;

namespace GymManagement.Desktop.Dialogs;

public sealed partial class ReceiptPreviewDialog : ContentDialog
{
    public ReceiptPreviewDialog(InvoiceDto invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        InitializeComponent();

        TxtInvoiceCode.Text = invoice.InvoiceCode;
        TxtCreatedAt.Text = (invoice.PaidAt ?? DateTime.Now).ToString("dd/MM/yyyy HH:mm");
        TxtMemberName.Text = invoice.MemberName ?? "Khách hàng vãng lai";
        TxtStaffName.Text = invoice.CreatedByAdminName ?? "Thu ngân";
        TxtItemName.Text = invoice.PlanName ?? "Gói tập gym";
        TxtItemPrice.Text = $"{invoice.SubtotalAmount:N0} đ";
        TxtTotalAmount.Text = $"{invoice.TotalAmount:N0} đ";

        if (invoice.PaymentMethod == "VIETQR")
        {
            TxtPaymentMethod.Text = "Chuyển khoản VietQR";
        }
        else if (invoice.PaymentMethod == "CREDIT_CARD")
        {
            TxtPaymentMethod.Text = "Thẻ POS";
        }
        else
        {
            TxtPaymentMethod.Text = "Tiền mặt";
        }

        if (invoice.Status == "PAID")
        {
            TxtStatus.Text = "ĐÃ THANH TOÁN";
            TxtStatus.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 4, 120, 87));
        }
        else if (invoice.Status == "REFUNDED")
        {
            TxtStatus.Text = "ĐÃ HOÀN TIỀN";
            TxtStatus.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 220, 38, 38));
        }
        else
        {
            TxtStatus.Text = invoice.Status;
        }

        PrimaryButtonClick += ReceiptPreviewDialog_PrimaryButtonClick;
    }

    private void ReceiptPreviewDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Khi bấm in: lệnh gửi tới máy in hoặc hoàn tất xem trước
    }
}

