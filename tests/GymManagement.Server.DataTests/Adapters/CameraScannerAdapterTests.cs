using System.Text;
using FluentAssertions;
using GymManagement.Server.Data.Adapters;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymManagement.Server.DataTests.Adapters;

public class CameraScannerAdapterTests
{
    private readonly Mock<ILogger<CameraScannerAdapter>> _loggerMock;
    private readonly CameraScannerAdapter _adapter;

    public CameraScannerAdapterTests()
    {
        _loggerMock = new Mock<ILogger<CameraScannerAdapter>>();
        _adapter = new CameraScannerAdapter(_loggerMock.Object);
    }

    [Fact]
    public async Task DecodeQrCodeFromBytesAsync_WhenEmptyBytes_ShouldReturnFailure()
    {
        // Arrange
        byte[] emptyBytes = Array.Empty<byte>();

        // Act
        var result = await _adapter.DecodeQrCodeFromBytesAsync(emptyBytes);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("trống");
    }

    [Fact]
    public async Task DecodeQrCodeFromBytesAsync_WhenPlainTextToken_ShouldReturnSuccess()
    {
        // Arrange
        string token = $"GYM-USER-001-{DateTime.UtcNow.Ticks}-CHECKSUM";
        byte[] bytes = Encoding.UTF8.GetBytes(token);

        // Act
        var result = await _adapter.DecodeQrCodeFromBytesAsync(bytes);

        // Assert
        result.Success.Should().BeTrue();
        result.RawPayload.Should().Be(token);
    }

    [Fact]
    public void ValidateCheckInToken_WhenValidTokenWithin60Seconds_ShouldReturnValid()
    {
        // Arrange
        long nowTicks = DateTime.UtcNow.Ticks;
        string token = $"GYM-MEMBER-999-{nowTicks}-VALID-SIGNATURE";

        // Act
        var result = _adapter.ValidateCheckInToken(token, validDurationSeconds: 60);

        // Assert
        result.IsValid.Should().BeTrue();
        result.UserId.Should().Be("MEMBER-999");
        result.IsExpired.Should().BeFalse();
        result.FailureReason.Should().BeNull();
    }

    [Fact]
    public void ValidateCheckInToken_WhenTokenExpiredBeyond60Seconds_ShouldReturnExpired()
    {
        // Arrange: Token tạo từ 120 giây trước
        long expiredTicks = DateTime.UtcNow.AddSeconds(-120).Ticks;
        string token = $"GYM-MEMBER-123-{expiredTicks}-OLD-SIGNATURE";

        // Act
        var result = _adapter.ValidateCheckInToken(token, validDurationSeconds: 60);

        // Assert
        result.IsValid.Should().BeFalse();
        result.IsExpired.Should().BeTrue();
        result.FailureReason.Should().Contain("hết hiệu lực");
    }

    [Fact]
    public void ValidateCheckInToken_WhenTokenMissingGymPrefix_ShouldReturnInvalid()
    {
        // Arrange
        string invalidToken = "UNKNOWN-PREFIX-123-456";

        // Act
        var result = _adapter.ValidateCheckInToken(invalidToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailureReason.Should().Contain("thiếu tiền tố GYM-");
    }
}

