using Microsoft.UI.Xaml.Controls;

namespace GymManagement.Desktop.Dialogs;

public sealed partial class SessionReviewDialog : ContentDialog
{
    public string SelectedOutcome
    {
        get
        {
            ComboBoxItem? item;
            item = CboOutcome.SelectedItem as ComboBoxItem;
            if (item != null && item.Tag != null)
            {
                return item.Tag.ToString()!;
            }
            return "COMPLETED";
        }
    }

    public string ReviewNotes
    {
        get
        {
            return TxtReviewNotes.Text.Trim();
        }
    }

    public SessionReviewDialog(string ptName, string memberName, DateTime scheduledDate, TimeSpan startTime, TimeSpan endTime)
    {
        InitializeComponent();

        TxtSessionDetails.Text = $"HLV: {ptName} • Hội viên: {memberName}";
        TxtScheduledTime.Text = $"Thời gian: {scheduledDate:dd/MM/yyyy} ({startTime:hh\\:mm} - {endTime:hh\\:mm})";
    }
}

