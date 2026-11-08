using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.Contracts.Data;
using GymManagement.Server.Core.DTOs.Identity;
using GymManagement.Server.Core.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace GymManagement.Server.Core.Services;

/// <summary>
/// Dịch vụ quản lý xác thực tài khoản và kiểm soát cổng truy cập bảo mật Authenticated.
/// Phục vụ UC01 (Đăng nhập và Xác thực) trong PRD.
/// </summary>
public sealed class AuthService : IAuthService
{
    // ====================================================================
    // 1. CONSTANTS & STATIC FIELDS (Khai báo đầu tiên)
    // ====================================================================
    public const int DEFAULT_TOKEN_EXPIRATION_MINUTES = 120;
    private const string DEFAULT_JWT_ISSUER = "GymManagementServer";
    private const string DEFAULT_JWT_AUDIENCE = "GymManagementClients";
    private const string FALLBACK_SIGNING_KEY = "GymManagementSystemSecretSigningKey_Minimum32Bytes!";

    // ====================================================================
    // 2. INJECTED DEPENDENCIES & PRIVATE FIELDS (Ngay sau constants)
    // ====================================================================
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    // ====================================================================
    // 3. CONSTRUCTOR (Khởi tạo và inject dependencies)
    // ====================================================================
    public AuthService(
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    // ====================================================================
    // 4. PUBLIC METHODS / INTERFACE IMPLEMENTATIONS (Nghiệp vụ chính)
    // ====================================================================

    /// <inheritdoc />
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new ValidationException("Tên đăng nhập không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("Mật khẩu không được để trống.");
        }

        string credential = request.Username.Trim();

        // 1. Tìm tài khoản người dùng theo UserName hoặc Email (US 01.1)
        var users = await _unitOfWork.Repository<User>().FindAsync(
            u => u.UserName == credential || (u.Email != null && u.Email == credential),
            cancellationToken);

        var user = users.FirstOrDefault();
        if (user == null)
        {
            _logger.LogWarning("AuthService.LoginAsync: Không tìm thấy tài khoản với thông tin {Credential}.", credential);
            throw new UnauthorizedAccessException("Tên đăng nhập hoặc mật khẩu không chính xác.");
        }

        // 2. Kiểm tra trạng thái tài khoản (Status == ACTIVE)
        if (string.Equals(user.Status, "LOCKED", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("AuthService.LoginAsync: Tài khoản {UserName} đang ở trạng thái LOCKED.", user.UserName);
            throw new BusinessRuleException("Tài khoản đã bị khóa.");
        }

        if (!string.Equals(user.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("AuthService.LoginAsync: Tài khoản {UserName} không ở trạng thái ACTIVE (hiện tại: {Status}).", user.UserName, user.Status);
            throw new BusinessRuleException("Tài khoản chưa được kích hoạt hoặc đã ngừng hoạt động.");
        }

        // 3. Kiểm tra mã băm mật khẩu qua BCrypt
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            _logger.LogWarning("AuthService.LoginAsync: Tài khoản {UserName} chưa có mã băm mật khẩu.", user.UserName);
            throw new UnauthorizedAccessException("Tên đăng nhập hoặc mật khẩu không chính xác.");
        }

        bool isPasswordValid = VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            _logger.LogWarning("AuthService.LoginAsync: Mật khẩu không chính xác cho tài khoản {UserName}.", user.UserName);
            throw new UnauthorizedAccessException("Tên đăng nhập hoặc mật khẩu không chính xác.");
        }

        // 4. Chuẩn hóa vai trò người dùng và sinh JWT Token
        string normalizedRole = NormalizeRole(user.RoleId);
        string token = GenerateJwtToken(user, normalizedRole);
        string refreshToken = GenerateSecureRefreshToken();

        string? fullName = user.FullName;
        if (string.IsNullOrWhiteSpace(fullName))
        {
            fullName = user.UserName;
        }

        _logger.LogInformation("AuthService.LoginAsync: Người dùng {UserName} ({Role}) đăng nhập thành công.", user.UserName, normalizedRole);

        return new LoginResponse(
            token,
            refreshToken,
            user.UserName,
            fullName!,
            normalizedRole);
    }

    /// <inheritdoc />
    public async Task<UserSessionDto> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ValidationException("Token không được để trống.");
        }

        var session = DecodeJwtToken(token);
        return await Task.FromResult(session);
    }

    /// <inheritdoc />
    public string HashPassword(string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
        {
            throw new ValidationException("Mật khẩu không được để trống.");
        }

        return BCrypt.Net.BCrypt.HashPassword(plainPassword);
    }

    /// <inheritdoc />
    public bool VerifyPassword(string plainPassword, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, passwordHash);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AuthService.VerifyPassword: Lỗi giải mã mật khẩu BCrypt.");
            return false;
        }
    }

    // ====================================================================
    // 5. PRIVATE HELPER METHODS (Luôn đặt ở cuối cùng của file)
    // ====================================================================

    private static string NormalizeRole(string? roleId)
    {
        if (string.IsNullOrWhiteSpace(roleId))
        {
            return "Member";
        }

        string trimmed = roleId.Trim();
        if (string.Equals(trimmed, "ADMIN", StringComparison.OrdinalIgnoreCase))
        {
            return "Admin";
        }

        if (string.Equals(trimmed, "PT", StringComparison.OrdinalIgnoreCase))
        {
            return "PT";
        }

        if (string.Equals(trimmed, "TRAINER", StringComparison.OrdinalIgnoreCase))
        {
            return "PT";
        }

        return "Member";
    }

    private string GenerateJwtToken(User user, string role)
    {
        string? issuer = _configuration["Jwt:Issuer"];
        if (string.IsNullOrWhiteSpace(issuer))
        {
            issuer = DEFAULT_JWT_ISSUER;
        }

        string? audience = _configuration["Jwt:Audience"];
        if (string.IsNullOrWhiteSpace(audience))
        {
            audience = DEFAULT_JWT_AUDIENCE;
        }

        string? signingKey = _configuration["Jwt:SigningKey"];
        if (string.IsNullOrWhiteSpace(signingKey))
        {
            signingKey = FALLBACK_SIGNING_KEY;
        }

        if (Encoding.UTF8.GetByteCount(signingKey) < 32)
        {
            signingKey = FALLBACK_SIGNING_KEY;
        }

        int expirationMinutes = DEFAULT_TOKEN_EXPIRATION_MINUTES;
        string? expirySetting = _configuration["Jwt:ExpirationMinutes"];
        if (!string.IsNullOrWhiteSpace(expirySetting))
        {
            if (int.TryParse(expirySetting, out int parsedExpiry))
            {
                if (parsedExpiry > 0)
                {
                    expirationMinutes = parsedExpiry;
                }
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        string? displayName = user.FullName;
        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = user.UserName;
        }

        string? email = user.Email;
        if (string.IsNullOrWhiteSpace(email))
        {
            email = string.Empty;
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserName),
            new(ClaimTypes.NameIdentifier, user.UserName),
            new(ClaimTypes.Name, displayName),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(securityToken);
    }

    private UserSessionDto DecodeJwtToken(string token)
    {
        string? issuer = _configuration["Jwt:Issuer"];
        if (string.IsNullOrWhiteSpace(issuer))
        {
            issuer = DEFAULT_JWT_ISSUER;
        }

        string? audience = _configuration["Jwt:Audience"];
        if (string.IsNullOrWhiteSpace(audience))
        {
            audience = DEFAULT_JWT_AUDIENCE;
        }

        string? signingKey = _configuration["Jwt:SigningKey"];
        if (string.IsNullOrWhiteSpace(signingKey))
        {
            signingKey = FALLBACK_SIGNING_KEY;
        }

        if (Encoding.UTF8.GetByteCount(signingKey) < 32)
        {
            signingKey = FALLBACK_SIGNING_KEY;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            string userId = string.Empty;
            var subClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
            if (subClaim != null)
            {
                userId = subClaim.Value;
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                var nameIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                if (nameIdClaim != null)
                {
                    userId = nameIdClaim.Value;
                }
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException("Token không chứa định danh người dùng hợp lệ.");
            }

            string fullName = string.Empty;
            var nameClaim = principal.FindFirst(ClaimTypes.Name);
            if (nameClaim != null)
            {
                fullName = nameClaim.Value;
            }

            string email = string.Empty;
            var emailClaim = principal.FindFirst(ClaimTypes.Email);
            if (emailClaim != null)
            {
                email = emailClaim.Value;
            }

            string role = "Member";
            var roleClaim = principal.FindFirst(ClaimTypes.Role);
            if (roleClaim != null)
            {
                role = roleClaim.Value;
            }

            return new UserSessionDto(userId, fullName, email, role);
        }
        catch (Exception ex) when (ex is not UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "AuthService.ValidateToken: Token không hợp lệ hoặc đã hết hạn.");
            throw new UnauthorizedAccessException("Token không hợp lệ hoặc đã hết hạn.");
        }
    }

    private static string GenerateSecureRefreshToken()
    {
        byte[] randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }
}
