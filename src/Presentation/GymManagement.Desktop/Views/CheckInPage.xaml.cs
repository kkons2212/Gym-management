using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using ZXing;
using ZXing.Common;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class CheckInPage : Page
{
    public CheckInViewModel ViewModel { get; }

    public CheckInPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<CheckInViewModel>();
    }

    private void OnInputScanCodeKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && ViewModel.ProcessCheckInCommand.CanExecute(null))
        {
            ViewModel.ProcessCheckInCommand.Execute(null);
        }
    }

    private async void OnOpenQrScannerClick(object sender, RoutedEventArgs e)
    {
        var scannerDialog = new QrCameraScannerDialog
        {
            XamlRoot = this.XamlRoot
        };

        scannerDialog.QrCodeDetected += async (qrCode) =>
        {
            await ViewModel.ProcessQrCheckInAsync(qrCode);
        };

        await scannerDialog.ShowAsync();
    }
}

public sealed class QrCameraScannerDialog : ContentDialog
{
    private readonly CameraScannerService _cameraScanner = new();
    private SoftwareBitmapSource? _bitmapSource;
    private readonly Image _cameraPreviewImage;
    private readonly InfoBar _statusBar;
    private readonly ComboBox _cameraDeviceComboBox;
    private readonly Grid _cameraSelectGrid;
    private IReadOnlyList<CameraDeviceInfo> _availableCameras = [];
    private bool _isHandlingDetection;

    public event Action<string>? QrCodeDetected;

    public QrCameraScannerDialog()
    {
        Title = "📷 Quét mã QR Hội viên (Camera Admin)";
        CloseButtonText = "Đóng";
        DefaultButton = ContentDialogButton.None;

        var rootPanel = new StackPanel
        {
            Spacing = 14,
            Width = 480
        };

        // Camera selector
        _cameraSelectGrid = new Grid
        {
            ColumnSpacing = 8,
            Visibility = Visibility.Collapsed
        };
        _cameraSelectGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        _cameraSelectGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var cameraLabel = new TextBlock
        {
            Text = "Thiết bị camera:",
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
        };
        Grid.SetColumn(cameraLabel, 0);

        _cameraDeviceComboBox = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _cameraDeviceComboBox.SelectionChanged += OnCameraDeviceSelectionChanged;
        Grid.SetColumn(_cameraDeviceComboBox, 1);

        _cameraSelectGrid.Children.Add(cameraLabel);
        _cameraSelectGrid.Children.Add(_cameraDeviceComboBox);
        rootPanel.Children.Add(_cameraSelectGrid);

        // Status bar
        _statusBar = new InfoBar
        {
            IsOpen = true,
            IsClosable = false,
            Severity = InfoBarSeverity.Informational,
            Message = "Đang khởi động camera..."
        };
        rootPanel.Children.Add(_statusBar);

        // Camera Feed Container
        var previewContainer = new Border
        {
            CornerRadius = new CornerRadius(8),
            Background = new SolidColorBrush(Color.FromArgb(255, 15, 23, 42)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(255, 51, 65, 85)),
            BorderThickness = new Thickness(1),
            Height = 340,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var previewGrid = new Grid();

        _cameraPreviewImage = new Image
        {
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        previewGrid.Children.Add(_cameraPreviewImage);

        // Reticle box overlay
        var reticleGrid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Width = 220,
            Height = 220
        };

        var reticleBorder = new Border
        {
            BorderBrush = new SolidColorBrush(Color.FromArgb(255, 36, 185, 138)),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Opacity = 0.85
        };
        reticleGrid.Children.Add(reticleBorder);

        var laserLine = new Border
        {
            Height = 2,
            Background = new SolidColorBrush(Color.FromArgb(255, 36, 185, 138)),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Opacity = 0.75
        };
        reticleGrid.Children.Add(laserLine);

        var reticleText = new TextBlock
        {
            Text = "ĐẶT MÃ QR VÀO KHUNG",
            FontSize = 11,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 36, 185, 138)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 10)
        };
        reticleGrid.Children.Add(reticleText);

        previewGrid.Children.Add(reticleGrid);
        previewContainer.Child = previewGrid;
        rootPanel.Children.Add(previewContainer);

        // Footer / Demo button
        var footerGrid = new Grid();
        var instructionText = new TextBlock
        {
            Text = "💡 Đưa màn hình điện thoại có mã QR vào trước camera.",
            Foreground = new SolidColorBrush(Color.FromArgb(255, 148, 163, 184)),
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };
        footerGrid.Children.Add(instructionText);

        var demoButton = new Button
        {
            Content = "⚡ Thử quét demo (MB-0248)",
            HorizontalAlignment = HorizontalAlignment.Right
        };
        demoButton.Click += OnSimulateDemoClick;
        footerGrid.Children.Add(demoButton);

        rootPanel.Children.Add(footerGrid);

        Content = rootPanel;

        // Events
        _cameraScanner.FrameReceived += OnFrameReceived;
        _cameraScanner.QrCodeDetected += OnQrDetected;
        Closing += OnDialogClosing;
        Loaded += OnDialogLoaded;
    }

    private async void OnDialogLoaded(object sender, RoutedEventArgs e)
    {
        _bitmapSource?.Dispose();
        _bitmapSource = new SoftwareBitmapSource();
        _cameraPreviewImage.Source = _bitmapSource;

        _availableCameras = await CameraScannerService.GetAvailableCamerasAsync();
        if (_availableCameras.Count > 1)
        {
            _cameraSelectGrid.Visibility = Visibility.Visible;
            _cameraDeviceComboBox.ItemsSource = _availableCameras.Select(c => c.Name).ToList();
            _cameraDeviceComboBox.SelectedIndex = 0;
        }

        try
        {
            var initialCamera = _availableCameras.FirstOrDefault();
            await _cameraScanner.StartAsync(initialCamera);
            _statusBar.Severity = InfoBarSeverity.Success;
            _statusBar.Message = "Camera đã sẵn sàng! Đưa mã QR vào khung quét.";
        }
        catch (Exception ex)
        {
            _statusBar.Severity = InfoBarSeverity.Error;
            _statusBar.Message = $"Không thể mở camera: {ex.Message}. Vui lòng cấp quyền camera trong Windows Settings hoặc dùng nút thử demo.";
        }
    }

    private async void OnCameraDeviceSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_cameraDeviceComboBox.SelectedIndex >= 0 && _cameraDeviceComboBox.SelectedIndex < _availableCameras.Count)
        {
            var selected = _availableCameras[_cameraDeviceComboBox.SelectedIndex];
            try
            {
                _statusBar.Severity = InfoBarSeverity.Informational;
                _statusBar.Message = $"Đang chuyển camera sang {selected.Name}...";
                await _cameraScanner.StartAsync(selected);
                _statusBar.Severity = InfoBarSeverity.Success;
                _statusBar.Message = $"Đang phát từ {selected.Name}.";
            }
            catch (Exception ex)
            {
                _statusBar.Severity = InfoBarSeverity.Error;
                _statusBar.Message = $"Lỗi đổi camera: {ex.Message}";
            }
        }
    }

    private void OnFrameReceived(SoftwareBitmap bitmap)
    {
        DispatcherQueue?.TryEnqueue(async () =>
        {
            try
            {
                if (_bitmapSource != null && bitmap != null)
                {
                    await _bitmapSource.SetBitmapAsync(bitmap);
                }
            }
            catch { }
        });
    }

    private void OnQrDetected(string qrCode)
    {
        DispatcherQueue?.TryEnqueue(async () =>
        {
            if (_isHandlingDetection) return;
            _isHandlingDetection = true;

            try
            {
                _statusBar.Severity = InfoBarSeverity.Success;
                _statusBar.Message = $"✅ Đã nhận diện mã: {qrCode}! Đang xác nhận check-in...";

                await Task.Delay(400);

                Hide();
                QrCodeDetected?.Invoke(qrCode);
            }
            finally
            {
                _isHandlingDetection = false;
            }
        });
    }

    private async void OnSimulateDemoClick(object sender, RoutedEventArgs e)
    {
        if (_isHandlingDetection) return;
        _isHandlingDetection = true;

        const string demoQr = "GH-MB0248-2026";
        _statusBar.Severity = InfoBarSeverity.Success;
        _statusBar.Message = $"⚡ Mô phỏng quét mã thành công: {demoQr}!";

        await Task.Delay(400);

        Hide();
        QrCodeDetected?.Invoke(demoQr);
        _isHandlingDetection = false;
    }

    private async void OnDialogClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        var deferral = args.GetDeferral();
        try
        {
            await _cameraScanner.StopAsync();
            _bitmapSource?.Dispose();
            _bitmapSource = null;
            _cameraPreviewImage.Source = null;
        }
        finally
        {
            deferral.Complete();
        }
    }
}

public record CameraDeviceInfo(string Id, string Name, MediaFrameSourceGroup? FrameSourceGroup);

public sealed class CameraScannerService : IDisposable
{
    private MediaCapture? _mediaCapture;
    private MediaFrameReader? _frameReader;
    private readonly BarcodeReaderGeneric _barcodeReader;
    private int _isProcessingFrame;
    private bool _isDisposed;
    private CancellationTokenSource? _scanCts;

    public event Action<SoftwareBitmap>? FrameReceived;
    public event Action<string>? QrCodeDetected;

    public CameraScannerService()
    {
        _barcodeReader = new BarcodeReaderGeneric
        {
            AutoRotate = true,
            Options = new DecodingOptions
            {
                PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE },
                TryHarder = true
            }
        };
    }

    public static async Task<IReadOnlyList<CameraDeviceInfo>> GetAvailableCamerasAsync()
    {
        var result = new List<CameraDeviceInfo>();
        try
        {
            var groups = await MediaFrameSourceGroup.FindAllAsync();
            foreach (var group in groups)
            {
                var hasColor = group.SourceInfos.Any(info => info.SourceKind == MediaFrameSourceKind.Color);
                if (hasColor)
                {
                    result.Add(new CameraDeviceInfo(group.Id, group.DisplayName, group));
                }
            }
        }
        catch
        {
            // Fallback device enumeration
        }

        if (result.Count == 0)
        {
            try
            {
                var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                foreach (var device in devices)
                {
                    result.Add(new CameraDeviceInfo(device.Id, device.Name, null));
                }
            }
            catch
            {
                // No cameras found or permission denied
            }
        }

        return result;
    }

    public async Task StartAsync(CameraDeviceInfo? cameraInfo = null)
    {
        await StopAsync();

        _scanCts = new CancellationTokenSource();
        _mediaCapture = new MediaCapture();

        var settings = new MediaCaptureInitializationSettings
        {
            StreamingCaptureMode = StreamingCaptureMode.Video,
            MemoryPreference = MediaCaptureMemoryPreference.Cpu
        };

        if (cameraInfo?.FrameSourceGroup != null)
        {
            settings.SourceGroup = cameraInfo.FrameSourceGroup;
        }
        else if (!string.IsNullOrEmpty(cameraInfo?.Id))
        {
            settings.VideoDeviceId = cameraInfo.Id;
        }

        await _mediaCapture.InitializeAsync(settings);

        // Try using MediaFrameReader first
        bool frameReaderStarted = false;
        try
        {
            var colorSource = _mediaCapture.FrameSources?.Values
                .FirstOrDefault(s => s.Info.SourceKind == MediaFrameSourceKind.Color);

            if (colorSource != null)
            {
                try
                {
                    _frameReader = await _mediaCapture.CreateFrameReaderAsync(colorSource, MediaEncodingSubtypes.Bgra8);
                }
                catch
                {
                    _frameReader = await _mediaCapture.CreateFrameReaderAsync(colorSource);
                }

                if (_frameReader != null)
                {
                    _frameReader.FrameArrived += OnFrameArrived;
                    var status = await _frameReader.StartAsync();
                    frameReaderStarted = (status == MediaFrameReaderStartStatus.Success);
                }
            }
        }
        catch
        {
            frameReaderStarted = false;
        }

        // Fallback: Periodic capture loop if MediaFrameReader is not supported by driver
        if (!frameReaderStarted)
        {
            _ = RunPeriodicCaptureLoopAsync(_scanCts.Token);
        }
    }

    private void OnFrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
    {
        if (Interlocked.CompareExchange(ref _isProcessingFrame, 1, 0) != 0)
            return;

        try
        {
            using var frameRef = sender.TryAcquireLatestFrame();
            var softwareBitmap = frameRef?.VideoMediaFrame?.SoftwareBitmap;
            if (softwareBitmap != null)
            {
                ProcessSoftwareBitmap(softwareBitmap);
            }
        }
        catch
        {
            // Frame skip
        }
        finally
        {
            Interlocked.Exchange(ref _isProcessingFrame, 0);
        }
    }

    private async Task RunPeriodicCaptureLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _mediaCapture != null)
        {
            try
            {
                using var stream = new InMemoryRandomAccessStream();
                await _mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateBmp(), stream);
                stream.Seek(0);

                var decoder = await BitmapDecoder.CreateAsync(stream);
                var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                ProcessSoftwareBitmap(softwareBitmap);
            }
            catch
            {
                // Ignore transient capture errors
            }

            try
            {
                await Task.Delay(150, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private void ProcessSoftwareBitmap(SoftwareBitmap bitmap)
    {
        SoftwareBitmap converted;
        if (bitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || bitmap.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
        {
            converted = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
        }
        else
        {
            converted = bitmap;
        }

        FrameReceived?.Invoke(converted);

        // QR decoding
        try
        {
            var buffer = new Windows.Storage.Streams.Buffer((uint)(converted.PixelWidth * converted.PixelHeight * 4));
            converted.CopyToBuffer(buffer);

            byte[] bytes = new byte[buffer.Length];
            using (var reader = DataReader.FromBuffer(buffer))
            {
                reader.ReadBytes(bytes);
            }

            var luminanceSource = new RGBLuminanceSource(bytes, converted.PixelWidth, converted.PixelHeight, RGBLuminanceSource.BitmapFormat.BGRA32);
            var decodeResult = _barcodeReader.Decode(luminanceSource);

            if (decodeResult != null && !string.IsNullOrWhiteSpace(decodeResult.Text))
            {
                QrCodeDetected?.Invoke(decodeResult.Text);
            }
        }
        catch
        {
            // Decode error or unsupported buffer
        }
    }

    public async Task StopAsync()
    {
        _scanCts?.Cancel();
        _scanCts?.Dispose();
        _scanCts = null;

        if (_frameReader != null)
        {
            try
            {
                _frameReader.FrameArrived -= OnFrameArrived;
                await _frameReader.StopAsync();
            }
            catch { }
            _frameReader.Dispose();
            _frameReader = null;
        }

        if (_mediaCapture != null)
        {
            _mediaCapture.Dispose();
            _mediaCapture = null;
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _ = StopAsync();
    }
}
