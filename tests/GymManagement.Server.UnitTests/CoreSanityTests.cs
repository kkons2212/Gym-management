namespace GymManagement.Server.UnitTests;

using GymManagement.Server.Core.DTOs.Common;
using GymManagement.Server.Core.Exceptions;
using Xunit;

public class CoreSanityTests
{
    private const string SAMPLE_ERROR_MESSAGE = "Quy tắc nghiệp vụ không hợp lệ.";

    [Fact]
    public void ApiResponse_WhenInitializedWithData_ShouldStoreValuesCorrectly()
    {
        // Arrange
        const string expectedData = "OK";

        // Act
        var response = new ApiResponse<string>(true, expectedData);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(expectedData, response.Data);
        Assert.Null(response.ErrorCode);
    }

    [Fact]
    public void BusinessRuleException_WhenInstantiatedWithMessage_ShouldRetainMessage()
    {
        // Arrange & Act
        var exception = new BusinessRuleException(SAMPLE_ERROR_MESSAGE);

        // Assert
        Assert.Equal(SAMPLE_ERROR_MESSAGE, exception.Message);
    }
}

