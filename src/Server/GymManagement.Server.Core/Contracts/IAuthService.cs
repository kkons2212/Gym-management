using GymManagement.Server.Core.DTOs.Identity;

namespace GymManagement.Server.Core.Contracts;

/// <summary>
/// Giao diện dịch vụ xác thực tài khoản và kiểm soát cổng truy cập bảo mật Authenticated.
/// Phục vụ UC01 (Đăng nhập và Xác thực) trong PRD.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Xác thực thông tin đăng nhập (Username hoặc Email + Password) và cấp phát JWT Token.
    /// </summary>
    /// <param name="request">Yêu cầu đăng nhập chứa Username/Email và Password.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Đối tượng phản hồi đăng nhập chứa JWT token, refresh token và thông tin người dùng.</returns>
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra tính hợp lệ của JWT Token và giải mã thông tin phiên làm việc người dùng.
    /// </summary>
    /// <param name="token">Chuỗi JWT Token cần kiểm tra.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Thông tin phiên làm việc của người dùng nếu token hợp lệ.</returns>
    Task<UserSessionDto> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Băm mật khẩu dạng văn bản thô theo thuật toán chuẩn BCrypt.
    /// </summary>
    /// <param name="plainPassword">Mật khẩu văn bản thô.</param>
    /// <returns>Chuỗi mã băm BCrypt an toàn.</returns>
    string HashPassword(string plainPassword);

    /// <summary>
    /// Kiểm tra mật khẩu văn bản thô khớp với mã băm BCrypt hay không.
    /// </summary>
    /// <param name="plainPassword">Mật khẩu văn bản thô.</param>
    /// <param name="passwordHash">Mã băm BCrypt lưu trong CSDL.</param>
    /// <returns>True nếu mật khẩu chính xác, ngược lại trả về False.</returns>
    bool VerifyPassword(string plainPassword, string passwordHash);
}
