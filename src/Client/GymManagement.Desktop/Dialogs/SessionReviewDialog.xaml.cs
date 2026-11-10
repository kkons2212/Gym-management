using Microsoft.UI.Xaml.Controls;
using GymManagement.Server.Core.Enums;

namespace GymManagement.Desktop.Dialogs;

public sealed partial class SessionReviewDialog : ContentDialog
{
    public PtSessionStatus SelectedOutcome
    {
        get
        {
            if (CboOutcome.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                if (Enum.TryParse<PtSessionStatus>(item.Tag.ToString(), out var status))
                {
                    return status;
                }
            }
            return PtSessionStatus.Completed;
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

