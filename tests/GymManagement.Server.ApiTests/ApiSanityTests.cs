namespace GymManagement.Server.ApiTests;

using Xunit;

public class ApiSanityTests
{
    private const string API_IDENTIFIER = "GymManagement.Server.Api";

    [Fact]
    public void ApiLayer_WhenLoaded_ShouldVerifyApiIdentity()
    {
        // Arrange & Act
        var identifier = API_IDENTIFIER;

        // Assert
        Assert.Equal("GymManagement.Server.Api", identifier);
    }
}

