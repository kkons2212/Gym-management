using GymManagement.Server.Core.Entities;
using GymManagement.Server.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GymManagement.Server.DataTests.Persistence;

public sealed class GymDbContextBillingModelTests
{
    [Fact]
    public void Model_WhenBillingEntitiesAreLoaded_MapsToPhysicalBillingTables()
    {
        var options = new DbContextOptionsBuilder<GymDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=GymManagementDb")
            .Options;
        using var context = new GymDbContext(options);

        var invoice = context.Model.FindEntityType(typeof(Invoice));
        var plan = context.Model.FindEntityType(typeof(PackagePlan));
        var subscription = context.Model.FindEntityType(typeof(MemberSubscription));
        var user = context.Model.FindEntityType(typeof(User));

        Assert.NotNull(invoice);
        Assert.NotNull(plan);
        Assert.NotNull(subscription);
        Assert.NotNull(user);
        Assert.Equal("Invoice", invoice!.GetTableName());
        Assert.Equal("PackagePlan", plan!.GetTableName());
        Assert.Equal("MemberSubscription", subscription!.GetTableName());
        Assert.Equal("User", user!.GetTableName());
        Assert.Equal(nameof(User.UserName), user.FindPrimaryKey()!.Properties.Single().Name);

        var invoiceTable = StoreObjectIdentifier.Table(invoice.GetTableName()!, invoice.GetSchema());
        var planTable = StoreObjectIdentifier.Table(plan.GetTableName()!, plan.GetSchema());
        Assert.Equal("paid_at", invoice.FindProperty(nameof(Invoice.PaidAt))!.GetColumnName(invoiceTable));
        Assert.Equal("is_Active", plan.FindProperty(nameof(PackagePlan.IsActive))!.GetColumnName(planTable));
    }
}
