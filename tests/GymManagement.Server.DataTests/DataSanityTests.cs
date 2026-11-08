namespace GymManagement.Server.DataTests;

using Xunit;

public class DataSanityTests
{
    private const string LAYER_NAME = "GymManagement.Server.Data";

    [Fact]
    public void DataLayer_WhenLoaded_ShouldVerifyLayerIdentity()
    {
        // Arrange & Act
        var layerName = LAYER_NAME;

        // Assert
        Assert.Equal("GymManagement.Server.Data", layerName);
    }
}

