namespace GymManagement.Desktop.Services.Hardware;

public interface IAudioFeedbackService
{
    void PlaySuccessBeep();
    void PlayErrorBuzz();
}

public class AudioFeedbackService : IAudioFeedbackService
{
    public void PlaySuccessBeep()
    {
        Task.Run(() =>
        {
            try
            {
                // Âm thanh Beep ngắn tần số cao (1800Hz, 150ms)
                Console.Beep(1800, 150);
            }
            catch
            {
                // Fallback nếu môi trường không hỗ trợ Console.Beep
            }
        });
    }

    public void PlayErrorBuzz()
    {
        Task.Run(() =>
        {
            try
            {
                // Âm thanh Buzz trầm báo lỗi (400Hz, 350ms)
                Console.Beep(400, 350);
            }
            catch
            {
                // Fallback nếu môi trường không hỗ trợ Console.Beep
            }
        });
    }
}
