using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace GymManagement.Desktop.Dialogs;

public sealed partial class InBodyEntryDialog : ContentDialog
{
    public decimal HeightCm
    {
        get
        {
            return (decimal)NbHeight.Value;
        }
    }

    public decimal WeightKg
    {
        get
        {
            return (decimal)NbWeight.Value;
        }
    }

    public decimal? BodyFatPercentage
    {
        get
        {
            if (double.IsNaN(NbBodyFat.Value))
            {
                return null;
            }

            return (decimal)NbBodyFat.Value;
        }
    }

    public decimal? SkeletalMuscleMass
    {
        get
        {
            if (double.IsNaN(NbMuscle.Value))
            {
                return null;
            }

            return (decimal)NbMuscle.Value;
        }
    }

    public string? Notes
    {
        get
        {
            if (string.IsNullOrWhiteSpace(TxtNotes.Text))
            {
                return null;
            }

            return TxtNotes.Text.Trim();
        }
    }

    public InBodyEntryDialog(string memberName)
    {
        InitializeComponent();
        TxtMemberInfo.Text = $"Hội viên: {memberName}";
        UpdateBmi();
    }

    private void OnInputMetricChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        UpdateBmi();
    }

    private void UpdateBmi()
    {
        if (TxtBmiValue == null || TxtBmiClassification == null || BrdBmiTag == null)
        {
            return;
        }

        var h = (decimal)NbHeight.Value;
        var w = (decimal)NbWeight.Value;

        if (h <= 0 || w <= 0)
        {
            TxtBmiValue.Text = "--";
            TxtBmiClassification.Text = "Chưa hợp lệ";
            return;
        }

        var hM = h / 100m;
        var bmi = Math.Round(w / (hM * hM), 1);
        TxtBmiValue.Text = bmi.ToString("F1");

        if (bmi < 18.5m)
        {
            TxtBmiClassification.Text = "Thiếu cân (Gầy)";
            BrdBmiTag.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 56, 189, 248)); // Sky 400
            TxtBmiValue.Foreground = BrdBmiTag.Background;
        }
        else if (bmi < 25.0m)
        {
            TxtBmiClassification.Text = "Bình thường (Chuẩn)";
            BrdBmiTag.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 22, 163, 74)); // Green 600
            TxtBmiValue.Foreground = BrdBmiTag.Background;
        }
        else if (bmi < 30.0m)
        {
            TxtBmiClassification.Text = "Thừa cân";
            BrdBmiTag.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 217, 119, 6)); // Amber 600
            TxtBmiValue.Foreground = BrdBmiTag.Background;
        }
        else
        {
            TxtBmiClassification.Text = "Béo phì";
            BrdBmiTag.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 220, 38, 38)); // Red 600
            TxtBmiValue.Foreground = BrdBmiTag.Background;
        }
    }
}
