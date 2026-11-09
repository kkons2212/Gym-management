using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GymManagement.Desktop.Dialogs;

public sealed partial class RefundInvoiceDialog : ContentDialog
{
    private readonly decimal _originalAmount;
    private readonly string _invoiceCode;

    public string InvoiceCode
    {
        get
        {
            return _invoiceCode;
        }
    }

    public decimal RefundAmount
    {
        get
        {
            decimal val;
            if (decimal.TryParse(TxtRefundAmount.Text.Trim(), out val))
            {
                return val;
            }
            return 0m;
        }
    }

    public string RefundMethod
    {
        get
        {
            ComboBoxItem? item;
            item = CboRefundMethod.SelectedItem as ComboBoxItem;
            if (item != null && item.Tag != null)
            {
                return item.Tag.ToString()!;
            }
            return "CASH";
        }
    }

    public string Reason
    {
        get
        {
            return TxtReason.Text.Trim();
        }
    }

    public RefundInvoiceDialog(string invoiceCode, decimal originalAmount)
    {
        InitializeComponent();
        _invoiceCode = invoiceCode;
        _originalAmount = originalAmount;

        TxtInvoiceCode.Text = invoiceCode;
        TxtOriginalAmount.Text = $"{originalAmount:N0} đ";
        TxtRefundAmount.Text = originalAmount.ToString("F0");

        PrimaryButtonClick += RefundInvoiceDialog_PrimaryButtonClick;
    }

    private void RefundInvoiceDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        decimal amount;
        string reason;

        amount = RefundAmount;
        reason = Reason;

        if (amount <= 0)
        {
            TxtValidationError.Text = "Số tiền hoàn phải lớn hơn 0 đ!";
            TxtValidationError.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        if (amount > _originalAmount)
        {
            TxtValidationError.Text = "Số tiền hoàn không được vượt quá số tiền gốc của hóa đơn!";
            TxtValidationError.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            TxtValidationError.Text = "Bắt buộc phải nhập lý do hoàn tiền!";
            TxtValidationError.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        TxtValidationError.Visibility = Visibility.Collapsed;
    }
}

