using Microsoft.UI.Xaml.Controls;
using GymManagement.Server.Core.Enums;

namespace GymManagement.Desktop.Dialogs;

public sealed partial class SessionReviewDialog : ContentDialog
{
    private DateTime _scheduledDate;
    private TimeSpan _endTime;

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

        _scheduledDate = scheduledDate;
        _endTime = endTime;

        TxtSessionDetails.Text = $"HLV: {ptName} • Hội viên: {memberName}";
        TxtScheduledTime.Text = $"Thời gian: {scheduledDate:dd/MM/yyyy} ({startTime:hh\\:mm} - {endTime:hh\\:mm})";

        // Disable confirm button if session hasn't ended yet
        UpdatePrimaryButtonState();
    }

    private void UpdatePrimaryButtonState()
    {
        // Calculate end time
        var sessionEndDateTime = _scheduledDate.Add(_endTime);
        var now = DateTime.Now;

        // If session hasn't ended, disable the button
        IsPrimaryButtonEnabled = now >= sessionEndDateTime;
    }
}

