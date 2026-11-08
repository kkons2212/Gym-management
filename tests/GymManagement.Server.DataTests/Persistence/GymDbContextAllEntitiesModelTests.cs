using GymManagement.Server.Core.Entities;
using GymManagement.Server.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace GymManagement.Server.DataTests.Persistence;

/// <summary>
/// Kiểm thử cấu hình ánh xạ CSDL cho toàn bộ 9 thực thể vật lý trong GymDbContext (Task 1.1).
/// Đối soát 100% với file DDL GymManagement_Physical_Model.sql.
/// </summary>
public sealed class GymDbContextAllEntitiesModelTests
{
    private static GymDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<GymDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=GymManagementDb;Trusted_Connection=True;")
            .Options;

        return new GymDbContext(options);
    }

    [Fact]
    public void Model_WhenLoaded_ShouldContainAll9PhysicalEntitiesWithCorrectTableNames()
    {
        // Arrange & Act
        using var context = CreateContext();
        var model = context.Model;

        // Assert: 9 Thực thể bắt buộc theo DDL
        var role = model.FindEntityType(typeof(Role));
        var user = model.FindEntityType(typeof(User));
        var plan = model.FindEntityType(typeof(PackagePlan));
        var subscription = model.FindEntityType(typeof(MemberSubscription));
        var availability = model.FindEntityType(typeof(PTAvailability));
        var session = model.FindEntityType(typeof(PTTrainingSession));
        var checkInLog = model.FindEntityType(typeof(CheckInLog));
        var invoice = model.FindEntityType(typeof(Invoice));
        var metric = model.FindEntityType(typeof(MemberBodyMetric));

        Assert.NotNull(role);
        Assert.NotNull(user);
        Assert.NotNull(plan);
        Assert.NotNull(subscription);
        Assert.NotNull(availability);
        Assert.NotNull(session);
        Assert.NotNull(checkInLog);
        Assert.NotNull(invoice);
        Assert.NotNull(metric);

        Assert.Equal("Role", role!.GetTableName());
        Assert.Equal("User", user!.GetTableName());
        Assert.Equal("PackagePlan", plan!.GetTableName());
        Assert.Equal("MemberSubscription", subscription!.GetTableName());
        Assert.Equal("PTAvailability", availability!.GetTableName());
        Assert.Equal("PTTrainingSession", session!.GetTableName());
        Assert.Equal("CheckInLog", checkInLog!.GetTableName());
        Assert.Equal("Invoice", invoice!.GetTableName());
        Assert.Equal("MemberBodyMetric", metric!.GetTableName());
    }

    [Fact]
    public void Model_WhenLoaded_ShouldHaveConfiguredPrimaryKeys()
    {
        // Arrange & Act
        using var context = CreateContext();
        var model = context.Model;

        // Assert
        Assert.Equal(nameof(Role.RoleID), model.FindEntityType(typeof(Role))!.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(nameof(User.UserName), model.FindEntityType(typeof(User))!.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(nameof(PackagePlan.PlanId), model.FindEntityType(typeof(PackagePlan))!.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(nameof(MemberSubscription.SubscriptionId), model.FindEntityType(typeof(MemberSubscription))!.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(nameof(PTAvailability.AvailabilityId), model.FindEntityType(typeof(PTAvailability))!.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(nameof(PTTrainingSession.PTSessionId), model.FindEntityType(typeof(PTTrainingSession))!.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(nameof(CheckInLog.CheckInId), model.FindEntityType(typeof(CheckInLog))!.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(nameof(Invoice.InvoiceId), model.FindEntityType(typeof(Invoice))!.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(nameof(MemberBodyMetric.BodyMetricId), model.FindEntityType(typeof(MemberBodyMetric))!.FindPrimaryKey()!.Properties.Single().Name);
    }

    [Fact]
    public void Model_WhenLoaded_ShouldHaveConfiguredPhysicalColumnNamesAndTypes()
    {
        // Arrange & Act
        using var context = CreateContext();
        var model = context.Model;

        var plan = model.FindEntityType(typeof(PackagePlan))!;
        var planTable = StoreObjectIdentifier.Table(plan.GetTableName()!, plan.GetSchema());
        Assert.Equal("is_Active", plan.FindProperty(nameof(PackagePlan.IsActive))!.GetColumnName(planTable));
        Assert.Equal("is_PtIncluded", plan.FindProperty(nameof(PackagePlan.IsPtIncluded))!.GetColumnName(planTable));

        var availability = model.FindEntityType(typeof(PTAvailability))!;
        var availTable = StoreObjectIdentifier.Table(availability.GetTableName()!, availability.GetSchema());
        Assert.Equal("is_Booked", availability.FindProperty(nameof(PTAvailability.IsBooked))!.GetColumnName(availTable));
        Assert.Equal("PtID", availability.FindProperty(nameof(PTAvailability.PtId))!.GetColumnName(availTable));

        var session = model.FindEntityType(typeof(PTTrainingSession))!;
        var sessionTable = StoreObjectIdentifier.Table(session.GetTableName()!, session.GetSchema());
        Assert.Equal("MemberID", session.FindProperty(nameof(PTTrainingSession.MemberId))!.GetColumnName(sessionTable));
        Assert.Equal("PtID", session.FindProperty(nameof(PTTrainingSession.PtId))!.GetColumnName(sessionTable));
        Assert.Equal("completed_at", session.FindProperty(nameof(PTTrainingSession.CompletedAt))!.GetColumnName(sessionTable));

        var invoice = model.FindEntityType(typeof(Invoice))!;
        var invoiceTable = StoreObjectIdentifier.Table(invoice.GetTableName()!, invoice.GetSchema());
        Assert.Equal("paid_at", invoice.FindProperty(nameof(Invoice.PaidAt))!.GetColumnName(invoiceTable));

        var checkIn = model.FindEntityType(typeof(CheckInLog))!;
        var checkInTable = StoreObjectIdentifier.Table(checkIn.GetTableName()!, checkIn.GetSchema());
        Assert.Equal("check_in_at", checkIn.FindProperty(nameof(CheckInLog.CheckInAt))!.GetColumnName(checkInTable));

        var metric = model.FindEntityType(typeof(MemberBodyMetric))!;
        var metricTable = StoreObjectIdentifier.Table(metric.GetTableName()!, metric.GetSchema());
        Assert.Equal("measured_at", metric.FindProperty(nameof(MemberBodyMetric.MeasuredAt))!.GetColumnName(metricTable));
    }

    [Fact]
    public void Model_WhenLoaded_ShouldEnforceUniqueConstraintsOnQrCodeAndInvoiceCode()
    {
        // Arrange & Act
        using var context = CreateContext();
        var model = context.Model;

        // Assert: UQ_MemberSubscription_QrCode
        var subscription = model.FindEntityType(typeof(MemberSubscription))!;
        var qrIndex = subscription.GetIndexes()
            .FirstOrDefault(idx => idx.Properties.Any(p => p.Name == nameof(MemberSubscription.QrCode)));

        Assert.NotNull(qrIndex);
        Assert.True(qrIndex!.IsUnique);
        Assert.Equal("UQ_MemberSubscription_QrCode", qrIndex.GetDatabaseName());

        // Assert: UQ_Invoice_InvoiceCode
        var invoice = model.FindEntityType(typeof(Invoice))!;
        var invoiceIndex = invoice.GetIndexes()
            .FirstOrDefault(idx => idx.Properties.Any(p => p.Name == nameof(Invoice.InvoiceCode)));

        Assert.NotNull(invoiceIndex);
        Assert.True(invoiceIndex!.IsUnique);
        Assert.Equal("UQ_Invoice_InvoiceCode", invoiceIndex.GetDatabaseName());
    }

    [Fact]
    public void Model_WhenLoaded_ShouldConfigureAllForeignKeysWithoutCascadeConflict()
    {
        // Arrange & Act
        using var context = CreateContext();
        var model = context.Model;

        // Foreign keys của MemberSubscription (User, PackagePlan, PTTrainingSession)
        var subscription = model.FindEntityType(typeof(MemberSubscription))!;
        var subFks = subscription.GetForeignKeys().ToList();
        Assert.Contains(subFks, fk => fk.PrincipalEntityType.ClrType == typeof(User) && fk.Properties.Any(p => p.Name == nameof(MemberSubscription.UserName)));
        Assert.Contains(subFks, fk => fk.PrincipalEntityType.ClrType == typeof(PackagePlan) && fk.Properties.Any(p => p.Name == nameof(MemberSubscription.PlanId)));
        Assert.Contains(subFks, fk => fk.PrincipalEntityType.ClrType == typeof(PTTrainingSession) && fk.Properties.Any(p => p.Name == nameof(MemberSubscription.PTSessionId)));

        // Foreign keys của PTTrainingSession
        var session = model.FindEntityType(typeof(PTTrainingSession))!;
        var sessionFks = session.GetForeignKeys().ToList();
        Assert.Contains(sessionFks, fk => fk.PrincipalEntityType.ClrType == typeof(User) && fk.Properties.Any(p => p.Name == nameof(PTTrainingSession.MemberId)));
        Assert.Contains(sessionFks, fk => fk.PrincipalEntityType.ClrType == typeof(User) && fk.Properties.Any(p => p.Name == nameof(PTTrainingSession.PtId)));
        Assert.Contains(sessionFks, fk => fk.PrincipalEntityType.ClrType == typeof(PTAvailability) && fk.Properties.Any(p => p.Name == nameof(PTTrainingSession.AvailabilityId)));

        // Toàn bộ delete behavior là Restrict để bảo vệ tính toàn vẹn dữ liệu
        foreach (var fk in subFks)
        {
            Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
        }
        foreach (var fk in sessionFks)
        {
            Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
        }
    }
}

