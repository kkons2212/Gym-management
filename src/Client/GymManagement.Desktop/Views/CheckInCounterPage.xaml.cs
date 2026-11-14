using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using GymManagement.Desktop.Dialogs;
using GymManagement.Desktop.Services.Camera;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

/// <summary>
/// Màn hình Quầy Điểm Danh & Cổng Tự Động (CheckInCounterPage).
/// Tích hợp luồng xem trực tiếp webcam, giải mã QR và can thiệp thủ công.
/// Tuân thủ quy chuẩn dev_rule.md (0 toán tử 3 ngôi, flat guard clauses).
/// </summary>
public sealed partial class CheckInCounterPage : Page
{
    private SoftwareBitmapSource? _bitmapSource;
    private int _isRenderingFrameFlag;

    public CheckInCounterViewModel ViewModel
    {
        get
        {
            return (CheckInCounterViewModel)DataContext;
        }
    }

    public CheckInCounterPage()
    {
        DataContext = App.GetService<CheckInCounterViewModel>();
        InitializeComponent();
        Loaded += CheckInCounterPage_Loaded;
        Unloaded += CheckInCounterPage_Unloaded;
    }

    private async void CheckInCounterPage_Loaded(object sender, RoutedEventArgs e)
    {
        TxtSearchInput.Focus(FocusState.Programmatic);

        _bitmapSource = new SoftwareBitmapSource();
        ImgCameraPreview.Source = _bitmapSource;

        ViewModel.VideoFrameReady += OnViewModelVideoFrameReady;

        await ViewModel.InitializeAsync();
    }

    private async void CheckInCounterPage_Unloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.VideoFrameReady -= OnViewModelVideoFrameReady;

        await ViewModel.StopCameraAsync();

        if (_bitmapSource != null)
        {
            _bitmapSource.Dispose();
            _bitmapSource = null;
        }
    }

    private void OnViewModelVideoFrameReady(object? sender, SoftwareBitmap bitmap)
    {
        if (bitmap == null || _bitmapSource == null)
        {
            return;
        }

        // Bỏ qua frame nếu frame trước đó vẫn đang trong quá trình render lên UI
        if (Interlocked.CompareExchange(ref _isRenderingFrameFlag, 1, 0) != 0)
        {
            return;
        }

        DispatcherQueue.TryEnqueue(async () =>
        {
            try
            {
                if (_bitmapSource == null)
                {
                    return;
                }

                SoftwareBitmap displayBitmap;
                bool needsDispose;

                needsDispose = false;
                if (bitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || bitmap.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
                {
                    displayBitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                    needsDispose = true;
                }
                else
                {
                    displayBitmap = bitmap;
                }

                await _bitmapSource.SetBitmapAsync(displayBitmap);

                if (needsDispose)
                {
                    displayBitmap.Dispose();
                }
            }
            catch (Exception)
            {
                // Bỏ qua lỗi vẽ frame khi camera đang dừng
            }
            finally
            {
                Interlocked.Exchange(ref _isRenderingFrameFlag, 0);
            }
        });
    }

    private async void CboCamera_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CboCamera.SelectedItem is CameraDeviceInfo device)
        {
            if (device.DeviceId != ViewModel.SelectedCamera?.DeviceId)
            {
                await ViewModel.SwitchCameraAsync(device);
            }
        }
    }

    private async void BtnToggleCamera_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ToggleCameraAsync();
    }

    private async void BtnRetryCamera_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadCamerasAndStartAsync();
    }

    private async void BtnScanFromFile_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            FileOpenPicker picker;
            picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".bmp");

            WinRT.Interop.InitializeWithWindow.Initialize(picker, App.WindowHandle);

            StorageFile? file;
            file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                await ViewModel.ScanFromImageFileAsync(file);
            }
        }
        catch (Exception ex)
        {
            ViewModel.ErrorMessage = "Lỗi khi mở file ảnh: " + ex.Message;
            ViewModel.HasResult = true;
            ViewModel.IsSuccess = false;
        }
    }

    private void TxtSearchInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            ViewModel.CheckInManualCommand.Execute(null);
            e.Handled = true;
        }
    }

    private async void BtnOpenOverrideDialog_Click(object sender, RoutedEventArgs e)
    {
        OverrideCheckInDialog dialog;
        ContentDialogResult result;
        string prefill;

        prefill = ViewModel.SearchQuery;
        dialog = new OverrideCheckInDialog(prefill);
        dialog.XamlRoot = this.XamlRoot;

        result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await ViewModel.ExecuteOverrideCheckInAsync(dialog.MemberIdentifier, dialog.FullOverrideReason);
        }
    }

    private void CboShift_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBox? combo;
        ComboBoxItem? selectedItem;
        string shift;

        combo = sender as ComboBox;
        if (combo != null)
        {
            selectedItem = combo.SelectedItem as ComboBoxItem;
            if (selectedItem != null && selectedItem.Tag != null)
            {
                shift = selectedItem.Tag.ToString()!;
                if (ViewModel != null)
                {
                    ViewModel.SelectedShift = shift;
                }
            }
        }
    }
}
