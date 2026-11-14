using System.Linq.Expressions;
using FluentAssertions;
using GymManagement.Server.Core.Contracts.Data;
using GymManagement.Server.Core.DTOs.Identity;
using GymManagement.Server.Core.Entities;
using GymManagement.Server.Core.Exceptions;
using GymManagement.Server.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymManagement.Server.UnitTests.Services;

/// <summary>
/// Unit Tests cho AuthService — UC01, US 01.1 (Đăng nhập và Xác thực).
/// Tuân thủ dev_rule Ch.6: Naming = [MethodUnderTest]_[StateUnderTest]_[ExpectedBehavior]
/// Mô hình: Arrange – Act – Assert (AAA)
/// </summary>
public class AuthServiceTests
{
    // =========================================================================
    // 1. CONSTANTS & STATIC FIELDS
    // =========================================================================
    private const string TEST_SIGNING_KEY = "GymManagementTestSigningKey_ForUnitTestsMustBe32BytesLong!";
    private const string TEST_ISSUER = "GymManagementServer";
    private const string TEST_AUDIENCE = "GymManagementClients";

    // =========================================================================
    // 2. TEST FIXTURES & SUT
    // =========================================================================
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IGenericRepository<User>> _userRepoMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _userRepoMock = new Mock<IGenericRepository<User>>();
        _loggerMock = new Mock<ILogger<AuthService>>();
        _configMock = new Mock<IConfiguration>();

        _uowMock.Setup(u => u.Repository<User>()).Returns(_userRepoMock.Object);

        _configMock.Setup(c => c["Jwt:Issuer"]).Returns(TEST_ISSUER);
        _configMock.Setup(c => c["Jwt:Audience"]).Returns(TEST_AUDIENCE);
        _configMock.Setup(c => c["Jwt:SigningKey"]).Returns(TEST_SIGNING_KEY);
        _configMock.Setup(c => c["Jwt:ExpirationMinutes"]).Returns("60");

        _sut = new AuthService(_uowMock.Object, _configMock.Object, _loggerMock.Object);
    }

    // =========================================================================
    // 3. UNIT TESTS — LOGINASYNC (UC01 / US 01.1)
    // =========================================================================

    [Fact]
    public async Task LoginAsync_WhenValidCredentials_ReturnsLoginResponseWithJwt()
    {
        // Arrange
        string rawPassword = "SecurePassword123!";
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

        var existingUser = new User
        {
            UserName = "admin01",
            RoleId = "ADMIN",
            FullName = "Quản trị viên Hệ thống",
            Phone = "0987654321",
            Email = "admin@gymsystem.vn",
            PasswordHash = passwordHash,
            Status = "ACTIVE"
        };

        SetupUserRepoFindResult([existingUser]);

        var request = new LoginRequest("admin01", rawPassword);

        // Act
        var response = await _sut.LoginAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.UserId.Should().Be("admin01");
        response.FullName.Should().Be("Quản trị viên Hệ thống");
        response.Role.Should().Be("Admin");
        response.Token.Should().NotBeNullOrWhiteSpace();
        response.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task LoginAsync_WhenLoginWithEmail_ReturnsLoginResponseWithJwt()
    {
        // Arrange
        string rawPassword = "MemberPassword123!";
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

        var existingUser = new User
        {
            UserName = "mem001",
            RoleId = "MEMBER",
            FullName = "Nguyễn Văn Hội Viên",
            Phone = "0912345678",
            Email = "member@gymsystem.vn",
            PasswordHash = passwordHash,
            Status = "ACTIVE"
        };

        SetupUserRepoFindResult([existingUser]);

        var request = new LoginRequest("member@gymsystem.vn", rawPassword);

        // Act
        var response = await _sut.LoginAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.UserId.Should().Be("mem001");
        response.Role.Should().Be("Member");
        response.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task LoginAsync_WhenRoleIsTrainer_NormalizesToPT()
    {
        // Arrange
        string rawPassword = "TrainerPassword!";
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

        var existingUser = new User
        {
            UserName = "pt001",
            RoleId = "TRAINER",
            FullName = "Huấn Luyện Viên Thể Hình",
            Phone = "0933333333",
            Email = "pt@gymsystem.vn",
            PasswordHash = passwordHash,
            Status = "ACTIVE"
        };

        SetupUserRepoFindResult([existingUser]);

        var request = new LoginRequest("pt001", rawPassword);

        // Act
        var response = await _sut.LoginAsync(request);

        // Assert
        response.Role.Should().Be("PT");
    }

    [Fact]
    public async Task LoginAsync_WhenRoleIsSuperAdmin_NormalizesToSuperAdmin()
    {
        // Arrange
        string rawPassword = "SuperAdminPassword!";
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

        var existingUser = new User
        {
            UserName = "admin",
            RoleId = "SUPERADMIN",
            FullName = "Quản Trị Hệ Thống",
            Phone = "0901234567",
            Email = "admin@gymsystem.vn",
            PasswordHash = passwordHash,
            Status = "ACTIVE"
        };

        SetupUserRepoFindResult([existingUser]);

        var request = new LoginRequest("admin", rawPassword);

        // Act
        var response = await _sut.LoginAsync(request);

        // Assert
        response.Role.Should().Be("SuperAdmin");
    }

    [Fact]
    public async Task LoginAsync_WhenInvalidPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        string correctPassword = "CorrectPassword123!";
        string wrongPassword = "WrongPassword999!";
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(correctPassword);

        var existingUser = new User
        {
            UserName = "admin01",
            RoleId = "ADMIN",
            FullName = "Quản trị viên",
            Email = "admin@gymsystem.vn",
            PasswordHash = passwordHash,
            Status = "ACTIVE"
        };

        SetupUserRepoFindResult([existingUser]);

        var request = new LoginRequest("admin01", wrongPassword);

        // Act
        Func<Task> act = async () =>
        {
            await _sut.LoginAsync(request);
        };

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Tên đăng nhập hoặc mật khẩu không chính xác.*");
    }

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        SetupUserRepoFindResult([]);

        var request = new LoginRequest("nonexistent_user", "AnyPassword123!");

        // Act
        Func<Task> act = async () =>
        {
            await _sut.LoginAsync(request);
        };

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Tên đăng nhập hoặc mật khẩu không chính xác.*");
    }

    [Fact]
    public async Task LoginAsync_WhenUserIsLocked_ThrowsBusinessRuleException()
    {
        // Arrange
        string rawPassword = "ValidPassword123!";
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

        var lockedUser = new User
        {
            UserName = "locked_user",
            RoleId = "MEMBER",
            FullName = "Tài Khoản Bị Khóa",
            PasswordHash = passwordHash,
            Status = "LOCKED"
        };

        SetupUserRepoFindResult([lockedUser]);

        var request = new LoginRequest("locked_user", rawPassword);

        // Act
        Func<Task> act = async () =>
        {
            await _sut.LoginAsync(request);
        };

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Tài khoản đã bị khóa.*");
    }

    [Fact]
    public async Task LoginAsync_WhenUserIsInactive_ThrowsBusinessRuleException()
    {
        // Arrange
        string rawPassword = "ValidPassword123!";
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

        var inactiveUser = new User
        {
            UserName = "inactive_user",
            RoleId = "MEMBER",
            FullName = "Tài Khoản Ngừng Hoạt Động",
            PasswordHash = passwordHash,
            Status = "INACTIVE"
        };

        SetupUserRepoFindResult([inactiveUser]);

        var request = new LoginRequest("inactive_user", rawPassword);

        // Act
        Func<Task> act = async () =>
        {
            await _sut.LoginAsync(request);
        };

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Tài khoản chưa được kích hoạt hoặc đã ngừng hoạt động.*");
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordHashIsNull_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var userWithoutPassword = new User
        {
            UserName = "nopass_user",
            RoleId = "MEMBER",
            FullName = "Không có mật khẩu",
            PasswordHash = null,
            Status = "ACTIVE"
        };

        SetupUserRepoFindResult([userWithoutPassword]);

        var request = new LoginRequest("nopass_user", "AnyPassword!");

        // Act
        Func<Task> act = async () =>
        {
            await _sut.LoginAsync(request);
        };

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Tên đăng nhập hoặc mật khẩu không chính xác.*");
    }

    [Fact]
    public async Task LoginAsync_WhenUsernameIsEmpty_ThrowsValidationException()
    {
        // Arrange
        var request = new LoginRequest("", "ValidPassword123!");

        // Act
        Func<Task> act = async () =>
        {
            await _sut.LoginAsync(request);
        };

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Tên đăng nhập không được để trống.*");
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsEmpty_ThrowsValidationException()
    {
        // Arrange
        var request = new LoginRequest("valid_user", "");

        // Act
        Func<Task> act = async () =>
        {
            await _sut.LoginAsync(request);
        };

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Mật khẩu không được để trống.*");
    }

    // =========================================================================
    // 4. UNIT TESTS — VALIDATETOKENASYNC
    // =========================================================================

    [Fact]
    public async Task ValidateTokenAsync_WhenValidToken_ReturnsUserSessionDto()
    {
        // Arrange
        string rawPassword = "AdminPassword!";
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

        var adminUser = new User
        {
            UserName = "admin99",
            RoleId = "ADMIN",
            FullName = "Admin Quản Trị",
            Email = "admin99@gymsystem.vn",
            PasswordHash = passwordHash,
            Status = "ACTIVE"
        };

        SetupUserRepoFindResult([adminUser]);

        var loginResult = await _sut.LoginAsync(new LoginRequest("admin99", rawPassword));

        // Act
        var session = await _sut.ValidateTokenAsync(loginResult.Token);

        // Assert
        session.Should().NotBeNull();
        session.UserId.Should().Be("admin99");
        session.FullName.Should().Be("Admin Quản Trị");
        session.Email.Should().Be("admin99@gymsystem.vn");
        session.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task ValidateTokenAsync_WhenTokenIsEmpty_ThrowsValidationException()
    {
        // Act
        Func<Task> act = async () =>
        {
            await _sut.ValidateTokenAsync("");
        };

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Token không được để trống.*");
    }

    [Fact]
    public async Task ValidateTokenAsync_WhenInvalidToken_ThrowsUnauthorizedAccessException()
    {
        // Act
        Func<Task> act = async () =>
        {
            await _sut.ValidateTokenAsync("invalid.malformed.jwttokenstring");
        };

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Token không hợp lệ hoặc đã hết hạn.*");
    }

    // =========================================================================
    // 5. UNIT TESTS — HASHPASSWORD & VERIFYPASSWORD
    // =========================================================================

    [Fact]
    public void HashPassword_WhenGivenPlainText_ReturnsValidBCryptHash()
    {
        // Arrange
        string plainText = "MyStrongSecretPass123!";

        // Act
        string hash = _sut.HashPassword(plainText);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().StartWith("$2");
        BCrypt.Net.BCrypt.Verify(plainText, hash).Should().BeTrue();
    }

    [Fact]
    public void HashPassword_WhenEmpty_ThrowsValidationException()
    {
        // Act
        Action act = () =>
        {
            _sut.HashPassword("");
        };

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("*Mật khẩu không được để trống.*");
    }

    [Fact]
    public void VerifyPassword_WhenHashMatches_ReturnsTrue()
    {
        // Arrange
        string plainText = "MatchedPassword!";
        string hash = BCrypt.Net.BCrypt.HashPassword(plainText);

        // Act
        bool result = _sut.VerifyPassword(plainText, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WhenHashMismatch_ReturnsFalse()
    {
        // Arrange
        string correctPass = "CorrectPass123!";
        string wrongPass = "WrongPass999!";
        string hash = BCrypt.Net.BCrypt.HashPassword(correctPass);

        // Act
        bool result = _sut.VerifyPassword(wrongPass, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WhenInputsAreEmpty_ReturnsFalse()
    {
        // Act & Assert
        _sut.VerifyPassword("", "any_hash").Should().BeFalse();
        _sut.VerifyPassword("any_pass", "").Should().BeFalse();
    }

    // =========================================================================
    // 6. PRIVATE HELPER METHODS
    // =========================================================================

    private void SetupUserRepoFindResult(IReadOnlyList<User> users)
    {
        _userRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);
    }
}
