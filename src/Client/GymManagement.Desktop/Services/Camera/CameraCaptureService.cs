using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;

namespace GymManagement.Desktop.Services.Camera;

/// <summary>
/// Dịch vụ thu nhận hình ảnh từ Camera/Webcam sử dụng Windows App SDK MediaCapture.
/// Tuân thủ quy chuẩn dev_rule.md (0 toán tử 3 ngôi, flat guard clauses, tiếng Việt trong comment).
/// </summary>
public sealed class CameraCaptureService : ICameraCaptureService
{
    private MediaCapture? _mediaCapture;
    private MediaFrameReader? _frameReader;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _isDisposed;

    public bool IsRunning { get; private set; }

    public bool HasCamera { get; private set; }

    public string? CurrentDeviceId { get; private set; }

    public event EventHandler<SoftwareBitmap>? FrameArrived;

    /// <summary>
    /// Liệt kê danh sách camera video khả dụng trên hệ thống.
    /// </summary>
    public async Task<IReadOnlyList<CameraDeviceInfo>> GetAvailableCamerasAsync()
    {
        List<CameraDeviceInfo> result;
        DeviceInformationCollection devices;

        result = new List<CameraDeviceInfo>();
        devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

        foreach (DeviceInformation device in devices)
        {
            if (device.IsEnabled)
            {
                result.Add(new CameraDeviceInfo(
                    DeviceId: device.Id,
                    DisplayName: device.Name,
                    IsDefault: device.IsDefault
                ));
            }
        }

        if (result.Count > 0)
        {
            HasCamera = true;
        }
        else
        {
            HasCamera = false;
        }

        return result;
    }

    /// <summary>
    /// Khởi tạo camera với thiết bị được chọn.
    /// </summary>
    public async Task<bool> InitializeAsync(string? deviceId = null)
    {
        await _lock.WaitAsync();
        try
        {
            IReadOnlyList<CameraDeviceInfo> cameras;
            string? targetDeviceId;

            cameras = await GetAvailableCamerasAsync();
            if (cameras.Count == 0)
            {
                HasCamera = false;
                return false;
            }

            targetDeviceId = deviceId;
            if (string.IsNullOrWhiteSpace(targetDeviceId))
            {
                targetDeviceId = cameras[0].DeviceId;
            }

            // Dọn dẹp phiên làm việc cũ nếu có
            await CleanupInternalAsync();

            MediaCaptureInitializationSettings settings;
            settings = new MediaCaptureInitializationSettings
            {
                VideoDeviceId = targetDeviceId,
                StreamingCaptureMode = StreamingCaptureMode.Video,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu
            };

            _mediaCapture = new MediaCapture();
            await _mediaCapture.InitializeAsync(settings);

            CurrentDeviceId = targetDeviceId;
            HasCamera = true;
            return true;
        }
        catch (Exception)
        {
            await CleanupInternalAsync();
            HasCamera = false;
            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Bắt đầu luồng thu nhận hình ảnh thời gian thực.
    /// </summary>
    public async Task<bool> StartPreviewAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (_mediaCapture == null)
            {
                return false;
            }

            if (IsRunning)
            {
                return true;
            }

            // Tìm nguồn Video Frame màu (Color Frame Source)
            MediaFrameSource? colorFrameSource = null;
            foreach (MediaFrameSource source in _mediaCapture.FrameSources.Values)
            {
                if (source.Info.SourceKind == MediaFrameSourceKind.Color)
                {
                    colorFrameSource = source;
                    break;
                }
            }

            if (colorFrameSource == null)
            {
                return false;
            }

            _frameReader = await _mediaCapture.CreateFrameReaderAsync(colorFrameSource, MediaEncodingSubtypes.Bgra8);
            _frameReader.FrameArrived += OnFrameReaderFrameArrived;

            MediaFrameReaderStartStatus status;
            status = await _frameReader.StartAsync();

            if (status == MediaFrameReaderStartStatus.Success)
            {
                IsRunning = true;
                return true;
            }

            return false;
        }
        catch (Exception)
        {
            IsRunning = false;
            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Dừng luồng thu nhận hình ảnh.
    /// </summary>
    public async Task StopPreviewAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (_frameReader != null)
            {
                _frameReader.FrameArrived -= OnFrameReaderFrameArrived;
                await _frameReader.StopAsync();
                _frameReader.Dispose();
                _frameReader = null;
            }

            IsRunning = false;
        }
        catch (Exception)
        {
            // Bỏ qua lỗi dừng an toàn
        }
        finally
        {
            _lock.Release();
        }
    }

    private void OnFrameReaderFrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
    {
        try
        {
            using MediaFrameReference? frame = sender.TryAcquireLatestFrame();
            if (frame == null)
            {
                return;
            }

            VideoMediaFrame? videoMediaFrame = frame.VideoMediaFrame;
            if (videoMediaFrame == null)
            {
                return;
            }

            SoftwareBitmap? softwareBitmap = videoMediaFrame.SoftwareBitmap;
            if (softwareBitmap == null)
            {
                return;
            }

            FrameArrived?.Invoke(this, softwareBitmap);
        }
        catch (Exception)
        {
            // Ngăn chặn exception làm crash luồng nền
        }
    }

    private async Task CleanupInternalAsync()
    {
        if (_frameReader != null)
        {
            try
            {
                _frameReader.FrameArrived -= OnFrameReaderFrameArrived;
                await _frameReader.StopAsync();
                _frameReader.Dispose();
            }
            catch (Exception)
            {
                // Bỏ qua lỗi khi giải phóng frame reader
            }
            _frameReader = null;
        }

        if (_mediaCapture != null)
        {
            try
            {
                _mediaCapture.Dispose();
            }
            catch (Exception)
            {
                // Bỏ qua lỗi khi giải phóng media capture
            }
            _mediaCapture = null;
        }

        IsRunning = false;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _ = StopPreviewAsync();
        _lock.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        await StopPreviewAsync();
        await CleanupInternalAsync();
        _lock.Dispose();
    }
}
