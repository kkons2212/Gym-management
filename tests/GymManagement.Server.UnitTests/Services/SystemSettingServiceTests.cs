using System.Linq.Expressions;
using FluentAssertions;
using GymManagement.Server.Core.Contracts.Data;
using GymManagement.Server.Core.DTOs.Settings;
using GymManagement.Server.Core.Entities;
using GymManagement.Server.Core.Exceptions;
using GymManagement.Server.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymManagement.Server.UnitTests.Services;

/// <summary>
/// Unit Tests cho SystemSettingService (Task 2.6 - UC07: US 07.1, 07.2, 07.3, 07.4, 07.5, 07.6).
/// Tuân thủ quy chuẩn dev_rule Ch.6: Naming = [MethodUnderTest]_[StateUnderTest]_[ExpectedBehavior]
/// Mô hình: Arrange – Act – Assert (AAA)
/// </summary>
public class SystemSettingServiceTests
{
    // =========================================================================
    // 1. TEST FIXTURES & SHARED SETUP
    // =========================================================================

    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ILogger<SystemSettingService>> _loggerMock;
    private readonly Mock<IGenericRepository<User>> _userRepoMock;
    private readonly Mock<IGenericRepository<Role>> _roleRepoMock;
    private readonly Mock<IGenericRepository<PackagePlan>> _packageRepoMock;
    private readonly Mock<IGenericRepository<MemberSubscription>> _subRepoMock;
    private readonly SystemSettingService _sut;

    public SystemSettingServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<SystemSettingService>>();
        _userRepoMock = new Mock<IGenericRepository<User>>();
        _roleRepoMock = new Mock<IGenericRepository<Role>>();
        _packageRepoMock = new Mock<IGenericRepository<PackagePlan>>();
        _subRepoMock = new Mock<IGenericRepository<MemberSubscription>>();

        _uowMock.Setup(u => u.Repository<User>()).Returns(_userRepoMock.Object);
        _uowMock.Setup(u => u.Repository<Role>()).Returns(_roleRepoMock.Object);
        _uowMock.Setup(u => u.Repository<PackagePlan>()).Returns(_packageRepoMock.Object);
        _uowMock.Setup(u => u.Repository<MemberSubscription>()).Returns(_subRepoMock.Object);

        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _sut = new SystemSettingService(_uowMock.Object, _loggerMock.Object);
    }

    // =========================================================================
    // 2. NHÓM 1: QUẢN TRỊ TÀI KHOẢN VÀ PHÂN QUYỀN (UC 7.1.1, 7.1.2, 7.1.3)
    // =========================================================================

    [Fact]
    public async Task CreateUserAsync_WhenValidInput_CreatesUserSuccessfully()
    {
        // Arrange
        var request = new CreateUserRequest(
            "staff01", "ADMIN", "Nguyễn Văn Admin", "0901234567",
            "admin@gym.vn", "P@ssword123", 1);

        _roleRepoMock.Setup(r => r.GetByIdAsync("ADMIN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { RoleID = "ADMIN", RoleName = "Quản trị viên" });

        _userRepoMock.Setup(u => u.GetByIdAsync("superadmin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { UserName = "superadmin", RoleId = "SUPERADMIN" });

        _userRepoMock.Setup(u => u.GetByIdAsync("staff01", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepoMock.Setup(u => u.FindSingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.CreateUserAsync(request, "superadmin");

        // Assert
        result.Should().NotBeNull();
        result.UserName.Should().Be("staff01");
        result.RoleId.Should().Be("ADMIN");
        result.RoleName.Should().Be("Quản trị viên");
        result.FullName.Should().Be("Nguyễn Văn Admin");
        result.Phone.Should().Be("0901234567");
        result.Status.Should().Be("ACTIVE");

        _userRepoMock.Verify(u => u.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WhenRoleIsAdminAndCreatorIsNotSuperAdmin_ThrowsBusinessRuleException()
    {
        // Arrange
        var request = new CreateUserRequest(
            "staff02", "ADMIN", "Admin Mới", "0901234568",
            "admin2@gym.vn", "P@ssword123", 1);

        _roleRepoMock.Setup(r => r.GetByIdAsync("ADMIN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { RoleID = "ADMIN", RoleName = "Quản trị viên" });

        _userRepoMock.Setup(u => u.GetByIdAsync("admin01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { UserName = "admin01", RoleId = "ADMIN" });

        // Act
        Func<Task> act = async () =>
        {
            await _sut.CreateUserAsync(request, "admin01");
        };

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Chỉ Quản trị viên cấp cao (SuperAdmin) mới có quyền tạo tài khoản Quản trị viên.*");
    }

    [Fact]
    public async Task CreateUserAsync_WhenRoleIsPtAndCreatorIsAdmin_CreatesSuccessfully()
    {
        // Arrange
        var request = new CreateUserRequest(
            "pt99", "PT", "HLV Mới", "0909999999",
            "pt99@gym.vn", "P@ssword123", 1);

        _roleRepoMock.Setup(r => r.GetByIdAsync("PT", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { RoleID = "PT", RoleName = "Huấn luyện viên" });

        _userRepoMock.Setup(u => u.GetByIdAsync("pt99", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepoMock.Setup(u => u.FindSingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.CreateUserAsync(request, "admin01");

        // Assert
        result.Should().NotBeNull();
        result.UserName.Should().Be("pt99");
        result.RoleId.Should().Be("PT");
    }

    [Fact]
    public async Task CreateUserAsync_WhenDuplicateUserName_ThrowsBusinessRuleException()
    {
        // Arrange
        var request = new CreateUserRequest(
            "admin", "ADMIN", "Họ Tên", "0901234567",
            "admin@gym.vn", "P@ssword123", 1);

        _roleRepoMock.Setup(r => r.GetByIdAsync("ADMIN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { RoleID = "ADMIN", RoleName = "Quản trị viên" });

        _userRepoMock.Setup(u => u.GetByIdAsync("superadmin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { UserName = "superadmin", RoleId = "SUPERADMIN" });

        _userRepoMock.Setup(u => u.GetByIdAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { UserName = "admin", RoleId = "ADMIN" });

        // Act
        Func<Task> act = async () =>
        {
            await _sut.CreateUserAsync(request, "superadmin");
        };

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*đã tồn tại*");
    }

    [Fact]
    public async Task CreateUserAsync_WhenDuplicatePhone_ThrowsBusinessRuleException()
    {
        // Arrange
        var request = new CreateUserRequest(
            "newuser", "MEMBER", "Hội viên mới", "0909999999",
            "new@gym.vn", "P@ssword123", 1);

        _roleRepoMock.Setup(r => r.GetByIdAsync("MEMBER", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { RoleID = "MEMBER", RoleName = "Hội viên" });

        _userRepoMock.Setup(u => u.GetByIdAsync("newuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepoMock.Setup(u => u.FindSingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { UserName = "olduser", Phone = "0909999999" });

        // Act
        Func<Task> act = async () =>
        {
            await _sut.CreateUserAsync(request);
        };

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Số điện thoại đã được sử dụng*");
    }

    [Fact]
    public async Task CreateUserAsync_WhenRoleNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var request = new CreateUserRequest(
            "user99", "INVALID_ROLE", "Tên", "0901234567",
            "u@gym.vn", "123456", 1);

        _roleRepoMock.Setup(r => r.GetByIdAsync("INVALID_ROLE", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        // Act
        Func<Task> act = async () =>
        {
            await _sut.CreateUserAsync(request);
        };

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*không tồn tại*");
    }

    [Fact]
    public async Task CreateUserAsync_WhenUserNameExceedsMaxLength_ThrowsValidationException()
    {
        // Arrange (MaxUserNameLength = 10 theo DDL SQL Server)
        var request = new CreateUserRequest(
            "username_that_is_too_long", "ADMIN", "Tên", "0901234567",
            "u@gym.vn", "123456", 1);

        // Act
        Func<Task> act = async () =>
        {
            await _sut.CreateUserAsync(request);
        };

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*không được vượt quá 10 ký tự*");
    }

    [Fact]
    public async Task UpdateUserAsync_WhenSelfDemotingAdminRole_ThrowsBusinessRuleException()
    {
        // Arrange: Quản trị viên 'admin01' cố gắng tự đổi vai trò của mình sang 'MEMBER'
        var request = new UpdateUserRequest("Admin Tên", "0901111111", "admin@gym.vn", "MEMBER", 1);

        var existingAdmin = new User
        {
            UserName = "admin01",
            RoleId = "ADMIN",
            FullName = "Admin Ban Đầu"
        };

        _userRepoMock.Setup(u => u.GetByIdAsync("admin01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAdmin);

        // Act
        Func<Task> act = async () =>
        {
            await _sut.UpdateUserAsync("admin01", request, "admin01");
        };

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Không thể tự thay đổi quyền Quản trị*");
    }

    [Fact]
    public async Task UpdateUserAsync_WhenUserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var request = new UpdateUserRequest("Tên Mới", "0901234567", "email@gym.vn", "PT", 1);

        _userRepoMock.Setup(u => u.GetByIdAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () =>
        {
            await _sut.UpdateUserAsync("nonexistent", request, "admin");
        };

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Không tìm thấy tài khoản*");
    }

    [Fact]
    public async Task UpdateUserAsync_WhenValidInput_UpdatesSuccessfully()
    {
        // Arrange
        var request = new UpdateUserRequest("Trần Văn PT", "0908888888", "pt@gym.vn", "TRAINER", 1);

        var existingUser = new User
        {
            UserName = "pt01",
            RoleId = "TRAINER",
            FullName = "Tên Cũ",
            Phone = "0901111111",
            Email = "old@gym.vn",
            Status = "ACTIVE"
        };

        _userRepoMock.Setup(u => u.GetByIdAsync("pt01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _roleRepoMock.Setup(r => r.GetByIdAsync("TRAINER", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { RoleID = "TRAINER", RoleName = "Huấn luyện viên" });

        _userRepoMock.Setup(u => u.FindSingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.UpdateUserAsync("pt01", request, "admin");

        // Assert
        result.Should().NotBeNull();
        result.FullName.Should().Be("Trần Văn PT");
        result.Phone.Should().Be("0908888888");
        result.Email.Should().Be("pt@gym.vn");

        _userRepoMock.Verify(u => u.Update(It.IsAny<User>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenPromotingToAdminAndCallerIsNotSuperAdmin_ThrowsBusinessRuleException()
    {
        // Arrange
        var request = new UpdateUserRequest("Trần Văn PT", "0908888888", "pt@gym.vn", "ADMIN", 1);

        var existingUser = new User
        {
            UserName = "pt01",
            RoleId = "PT",
            FullName = "Tên Cũ",
            Phone = "0901111111",
            Email = "old@gym.vn",
            Status = "ACTIVE"
        };

        _userRepoMock.Setup(u => u.GetByIdAsync("pt01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _roleRepoMock.Setup(r => r.GetByIdAsync("ADMIN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { RoleID = "ADMIN", RoleName = "Quản trị viên" });

        _userRepoMock.Setup(u => u.GetByIdAsync("admin01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { UserName = "admin01", RoleId = "ADMIN" });

        // Act
        Func<Task> act = async () =>
        {
            await _sut.UpdateUserAsync("pt01", request, "admin01");
        };

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Chỉ Quản trị viên cấp cao (SuperAdmin) mới có quyền phân quyền Quản trị viên.*");
    }

    [Fact]
    public async Task SetUserStatusAsync_WhenAdminSelfLockout_ThrowsBusinessRuleException()
    {
        // Arrange: Quản trị viên 'admin01' cố gắng tự khóa tài khoản của chính mình
        // Act
        Func<Task> act = async () =>
        {
            await _sut.SetUserStatusAsync("admin01", "LOCKED", "admin01");
        };

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Không thể tự khóa hoặc vô hiệu hóa*");
    }

    [Fact]
    public async Task SetUserStatusAsync_WhenValidInput_UpdatesStatusSuccessfully()
    {
        // Arrange
        var targetUser = new User
        {
            UserName = "staff02",
            RoleId = "TRAINER",
            FullName = "Nhân viên B",
            Status = "ACTIVE"
        };

        _userRepoMock.Setup(u => u.GetByIdAsync("staff02", It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetUser);

        _roleRepoMock.Setup(r => r.GetByIdAsync("TRAINER", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { RoleID = "TRAINER", RoleName = "Huấn luyện viên" });

        // Act
        var result = await _sut.SetUserStatusAsync("staff02", "LOCKED", "admin");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("LOCKED");
        targetUser.Status.Should().Be("LOCKED");

        _userRepoMock.Verify(u => u.Update(targetUser), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // =========================================================================
    // 3. NHÓM 2: QUẢN LÝ DANH MỤC GÓI TẬP (UC 7.2.1, 7.2.2, 7.2.3)
    // =========================================================================

    [Fact]
    public async Task CreatePackagePlanAsync_WhenPriceIsNegativeOrZero_ThrowsValidationException()
    {
        // Arrange
        var request = new CreatePackagePlanRequest(
            "PKG01", "Gói Tập 1 Tháng", "Mô tả", 0m, 30);

        // Act
        Func<Task> act = async () =>
        {
            await _sut.CreatePackagePlanAsync(request);
        };

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Đơn giá gói tập phải lớn hơn 0*");
    }

    [Fact]
    public async Task CreatePackagePlanAsync_WhenDuplicatePlanId_ThrowsBusinessRuleException()
    {
        // Arrange
        var request = new CreatePackagePlanRequest(
            "VIP1", "Gói Tập VIP", "Mô tả", 1000000m, 30);

        _packageRepoMock.Setup(p => p.GetByIdAsync("VIP1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PackagePlan { PlanId = "VIP1", Name = "Gói VIP Cũ" });

        // Act
        Func<Task> act = async () =>
        {
            await _sut.CreatePackagePlanAsync(request);
        };

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*đã tồn tại*");
    }

    [Fact]
    public async Task CreatePackagePlanAsync_WhenValidInput_CreatesPackageSuccessfully()
    {
        // Arrange
        var request = new CreatePackagePlanRequest(
            "GOLD1M", "Gói Vàng 1 Tháng", "Gym không giới hạn + 2 buổi PT",
            800000m, 30, null, true, 2);

        _packageRepoMock.Setup(p => p.GetByIdAsync("GOLD1M", It.IsAny<CancellationToken>()))
            .ReturnsAsync((PackagePlan?)null);

        _packageRepoMock.Setup(p => p.FindSingleOrDefaultAsync(It.IsAny<Expression<Func<PackagePlan, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PackagePlan?)null);

        // Act
        var result = await _sut.CreatePackagePlanAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.PlanId.Should().Be("GOLD1M");
        result.Name.Should().Be("Gói Vàng 1 Tháng");
        result.Price.Should().Be(800000m);
        result.DurationDays.Should().Be(30);
        result.IsPtIncluded.Should().BeTrue();
        result.TotalPtSessions.Should().Be(2);
        result.Status.Should().Be("ACTIVE");

        _packageRepoMock.Verify(p => p.AddAsync(It.IsAny<PackagePlan>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePackagePlanAsync_WhenPackageNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var request = new UpdatePackagePlanRequest("Tên Mới", "Mô tả", 500000m, 30);

        _packageRepoMock.Setup(p => p.GetByIdAsync("NONEXIST", It.IsAny<CancellationToken>()))
            .ReturnsAsync((PackagePlan?)null);

        // Act
        Func<Task> act = async () =>
        {
            await _sut.UpdatePackagePlanAsync("NONEXIST", request);
        };

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Không tìm thấy gói tập*");
    }

    [Fact]
    public async Task DeletePackagePlanAsync_WhenPackageHasNoSubscriptions_PerformsHardDelete()
    {
        // Arrange: Gói tập chưa từng có hội viên đăng ký
        var plan = new PackagePlan
        {
            PlanId = "TEMP1",
            Name = "Gói Thử Nghiệm",
            Price = 100000m,
            DurationDays = 7,
            IsActive = 1
        };

        _packageRepoMock.Setup(p => p.GetByIdAsync("TEMP1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _subRepoMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<MemberSubscription, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.DeletePackagePlanAsync("TEMP1");

        // Assert
        result.Should().NotBeNull();
        result.IsHardDeleted.Should().BeTrue();
        result.IsDiscontinued.Should().BeFalse();
        result.Message.Should().Contain("Đã xóa hoàn toàn");

        _packageRepoMock.Verify(p => p.Remove(plan), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeletePackagePlanAsync_WhenPackageHasSubscriptions_DiscontinuesPackageAndPreventsHardDelete()
    {
        // Arrange: Gói tập đã có hội viên đăng ký sở hữu thẻ
        var plan = new PackagePlan
        {
            PlanId = "VIP1Y",
            Name = "Gói VIP 1 Năm",
            Price = 5000000m,
            DurationDays = 365,
            IsActive = 1
        };

        _packageRepoMock.Setup(p => p.GetByIdAsync("VIP1Y", It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _subRepoMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<MemberSubscription, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.DeletePackagePlanAsync("VIP1Y");

        // Assert
        result.Should().NotBeNull();
        result.IsHardDeleted.Should().BeFalse();
        result.IsDiscontinued.Should().BeTrue();
        result.Message.Should().Contain("Ngừng kinh doanh (DISCONTINUED)");
        plan.IsActive.Should().Be(0);

        _packageRepoMock.Verify(p => p.Update(plan), Times.Once);
        _packageRepoMock.Verify(p => p.Remove(It.IsAny<PackagePlan>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPackagePlansAsync_WhenIncludeInactiveFalse_ReturnsOnlyActivePlans()
    {
        // Arrange
        var activePlan = new PackagePlan { PlanId = "ACT", Name = "Active", Price = 500000m, DurationDays = 30, IsActive = 1 };

        _packageRepoMock.Setup(p => p.FindAsync(It.IsAny<Expression<Func<PackagePlan, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PackagePlan> { activePlan });

        // Act
        var result = await _sut.GetPackagePlansAsync(includeInactive: false);

        // Assert
        result.Should().HaveCount(1);
        result[0].PlanId.Should().Be("ACT");
        result[0].Status.Should().Be("ACTIVE");
    }
}
