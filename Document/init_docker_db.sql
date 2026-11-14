-- ====================================================================
-- HỆ THỐNG QUẢN LÝ PHÒNG TẬP THỂ THAO (GYM MANAGEMENT SYSTEM)
-- SCRIPT KHỞI TẠO CƠ SỞ DỮ LIỆU DOCKER SQL SERVER (INIT DOCKER DB)
-- Chuẩn hóa: 9 bảng vật lý DDL + Ràng buộc Foreign Keys + Dữ liệu nền tảng
-- ====================================================================

-- 1. TẠO CƠ SỞ DỮ LIỆU NẾU CHƯA TỒN TẠI
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'GymManagementDb')
BEGIN
    CREATE DATABASE GymManagementDb;
END
GO

USE GymManagementDb;
GO

-- 2. TẠO CẤU TRÚC 9 BẢNG VẬT LÝ THEO CHUẨN DDL

-- 2.1. Bảng Role (Phân quyền)
IF OBJECT_ID('dbo.Role', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Role (
        RoleID VARCHAR(10) NOT NULL,
        RoleName NVARCHAR(50) NULL,
        CONSTRAINT PK_Role PRIMARY KEY (RoleID)
    );
END
GO

-- 2.2. Bảng User (Người dùng: Admin, PT, Member)
IF OBJECT_ID('dbo.[User]', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.[User] (
        UserName VARCHAR(10) NOT NULL,
        RoleID VARCHAR(10) NOT NULL,
        FullName NVARCHAR(50) NULL,
        Phone VARCHAR(20) NULL,
        Email VARCHAR(255) NULL,
        PasswordHash VARCHAR(255) NULL,
        Gender TINYINT NULL,
        Status VARCHAR(20) NULL,
        created_at DATE NULL,
        CONSTRAINT PK_User PRIMARY KEY (UserName)
    );
END
GO

-- 2.3. Bảng PackagePlan (Danh mục gói tập)
IF OBJECT_ID('dbo.PackagePlan', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PackagePlan (
        PlanId VARCHAR(10) NOT NULL,
        Name NVARCHAR(50) NULL,
        Description NVARCHAR(255) NULL,
        DurationDays INT NULL,
        Price DECIMAL(19, 0) NULL,
        MaxCheckInSessions INT NULL,
        is_PtIncluded TINYINT NULL,
        TotalPtSessions INT NULL,
        is_Active TINYINT NULL,
        CONSTRAINT PK_PackagePlan PRIMARY KEY (PlanId)
    );
END
GO

-- 2.4. Bảng PTAvailability (Lịch rảnh của Huấn luyện viên)
IF OBJECT_ID('dbo.PTAvailability', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PTAvailability (
        AvailabilityId VARCHAR(10) NOT NULL,
        AvailableDate DATE NULL,
        StartTime TIME(7) NULL,
        EndTime TIME(7) NULL,
        is_Booked TINYINT NULL,
        PtID VARCHAR(10) NOT NULL,
        CONSTRAINT PK_PTAvailability PRIMARY KEY (AvailabilityId)
    );
END
GO

-- 2.5. Bảng PTTrainingSession (Ca huấn luyện 1-1 giữa Hội viên và PT)
IF OBJECT_ID('dbo.PTTrainingSession', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PTTrainingSession (
        PTSessionId VARCHAR(10) NOT NULL,
        SessionStartTime DATE NULL,
        SessionEndTime DATE NULL,
        Status VARCHAR(20) NULL,
        WorkoutNotes NVARCHAR(255) NULL,
        RatingStars TINYINT NULL,
        ReviewComment NVARCHAR(255) NULL,
        completed_at DATE NULL,
        MemberID VARCHAR(10) NOT NULL,
        PtID VARCHAR(10) NOT NULL,
        AvailabilityId VARCHAR(10) NULL,
        CONSTRAINT PK_PTTrainingSession PRIMARY KEY (PTSessionId)
    );
END
GO

-- 2.6. Bảng MemberSubscription (Hợp đồng gói tập của Hội viên)
IF OBJECT_ID('dbo.MemberSubscription', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.MemberSubscription (
        SubscriptionId VARCHAR(10) NOT NULL,
        QrCode VARCHAR(255) NULL,
        StartDate DATE NULL,
        EndDate DATE NULL,
        RemainingCheckInSessions INT NULL,
        RemainingPtSessions INT NULL,
        FreezeStartDate DATE NULL,
        FreezeEndDate DATE NULL,
        Status VARCHAR(20) NULL,
        created_at DATE NULL,
        UserName VARCHAR(10) NOT NULL,
        AssignedPtId VARCHAR(10) NULL,
        PlanId VARCHAR(10) NOT NULL,
        PTSessionId VARCHAR(10) NULL,
        CONSTRAINT PK_MemberSubscription PRIMARY KEY (SubscriptionId)
    );
    CREATE UNIQUE NONCLUSTERED INDEX UQ_MemberSubscription_QrCode ON dbo.MemberSubscription (QrCode) WHERE QrCode IS NOT NULL;
END
GO

-- 2.7. Bảng CheckInLog (Nhật ký quét mã vào phòng tập)
IF OBJECT_ID('dbo.CheckInLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CheckInLog (
        CheckInId VARCHAR(10) NOT NULL,
        check_in_at DATE NULL,
        Status VARCHAR(20) NULL,
        DenialReason NVARCHAR(100) NULL,
        SubscriptionId VARCHAR(10) NOT NULL,
        VerifiedByAdminId VARCHAR(10) NULL,
        UserName VARCHAR(10) NOT NULL,
        CONSTRAINT PK_CheckInLog PRIMARY KEY (CheckInId)
    );
END
GO

-- 2.8. Bảng Invoice (Hóa đơn thanh toán POS & VietQR)
IF OBJECT_ID('dbo.Invoice', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Invoice (
        InvoiceId VARCHAR(10) NOT NULL,
        InvoiceCode VARCHAR(255) NULL,
        TransactionType NVARCHAR(50) NULL,
        SubtotalAmount DECIMAL(19, 0) NULL,
        TotalAmount INT NULL,
        PaymentMethod NVARCHAR(100) NULL,
        Status VARCHAR(20) NULL,
        RefundAmount DECIMAL(19, 0) NULL,
        RefundReason NVARCHAR(255) NULL,
        paid_at DATE NULL,
        UserName VARCHAR(10) NOT NULL,
        SubscriptionId VARCHAR(10) NOT NULL,
        CreatedByAdminId VARCHAR(10) NULL,
        CheckInLogCheckInId VARCHAR(10) NULL,
        CONSTRAINT PK_Invoice PRIMARY KEY (InvoiceId),
        CONSTRAINT UQ_Invoice_InvoiceCode UNIQUE (InvoiceCode)
    );
END
GO

-- 2.9. Bảng MemberBodyMetric (Chỉ số thể hình InBody của Hội viên)
IF OBJECT_ID('dbo.MemberBodyMetric', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.MemberBodyMetric (
        BodyMetricId VARCHAR(10) NOT NULL,
        WeightKg DECIMAL(19, 0) NULL,
        HeightCm DECIMAL(19, 0) NULL,
        SkeletalMuscleMass DECIMAL(19, 0) NULL,
        BodyFatPercentage DECIMAL(19, 0) NULL,
        Bmi DECIMAL(19, 0) NULL,
        Notes NVARCHAR(255) NULL,
        measured_at DATE NULL,
        UserName VARCHAR(10) NOT NULL,
        PTSessionId VARCHAR(10) NULL,
        RecordedByPtId VARCHAR(10) NULL,
        CONSTRAINT PK_MemberBodyMetric PRIMARY KEY (BodyMetricId)
    );
END
GO

-- 3. THIẾT LẬP RÀNG BUỘC KHÓA NGOẠI (FOREIGN KEYS) NẾU CHƯA CÓ

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_User_Role')
    ALTER TABLE dbo.[User] ADD CONSTRAINT FK_User_Role FOREIGN KEY (RoleID) REFERENCES dbo.Role (RoleID);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PTAvailability_User')
    ALTER TABLE dbo.PTAvailability ADD CONSTRAINT FK_PTAvailability_User FOREIGN KEY (PtID) REFERENCES dbo.[User] (UserName);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PTTrainingSession_Member')
    ALTER TABLE dbo.PTTrainingSession ADD CONSTRAINT FK_PTTrainingSession_Member FOREIGN KEY (MemberID) REFERENCES dbo.[User] (UserName) ON UPDATE CASCADE;

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PTTrainingSession_PT')
    ALTER TABLE dbo.PTTrainingSession ADD CONSTRAINT FK_PTTrainingSession_PT FOREIGN KEY (PtID) REFERENCES dbo.[User] (UserName);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PTTrainingSession_Availability')
    ALTER TABLE dbo.PTTrainingSession ADD CONSTRAINT FK_PTTrainingSession_Availability FOREIGN KEY (AvailabilityId) REFERENCES dbo.PTAvailability (AvailabilityId);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MemberSubscription_User')
    ALTER TABLE dbo.MemberSubscription ADD CONSTRAINT FK_MemberSubscription_User FOREIGN KEY (UserName) REFERENCES dbo.[User] (UserName) ON UPDATE CASCADE;

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MemberSubscription_AssignedPt')
    ALTER TABLE dbo.MemberSubscription ADD CONSTRAINT FK_MemberSubscription_AssignedPt FOREIGN KEY (AssignedPtId) REFERENCES dbo.[User] (UserName);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MemberSubscription_PackagePlan')
    ALTER TABLE dbo.MemberSubscription ADD CONSTRAINT FK_MemberSubscription_PackagePlan FOREIGN KEY (PlanId) REFERENCES dbo.PackagePlan (PlanId);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MemberSubscription_PTSession')
    ALTER TABLE dbo.MemberSubscription ADD CONSTRAINT FK_MemberSubscription_PTSession FOREIGN KEY (PTSessionId) REFERENCES dbo.PTTrainingSession (PTSessionId);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CheckInLog_Subscription')
    ALTER TABLE dbo.CheckInLog ADD CONSTRAINT FK_CheckInLog_Subscription FOREIGN KEY (SubscriptionId) REFERENCES dbo.MemberSubscription (SubscriptionId);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CheckInLog_User')
    ALTER TABLE dbo.CheckInLog ADD CONSTRAINT FK_CheckInLog_User FOREIGN KEY (UserName) REFERENCES dbo.[User] (UserName) ON UPDATE CASCADE;

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CheckInLog_Admin')
    ALTER TABLE dbo.CheckInLog ADD CONSTRAINT FK_CheckInLog_Admin FOREIGN KEY (VerifiedByAdminId) REFERENCES dbo.[User] (UserName);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Invoice_User')
    ALTER TABLE dbo.Invoice ADD CONSTRAINT FK_Invoice_User FOREIGN KEY (UserName) REFERENCES dbo.[User] (UserName) ON UPDATE CASCADE;

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Invoice_Subscription')
    ALTER TABLE dbo.Invoice ADD CONSTRAINT FK_Invoice_Subscription FOREIGN KEY (SubscriptionId) REFERENCES dbo.MemberSubscription (SubscriptionId);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Invoice_Admin')
    ALTER TABLE dbo.Invoice ADD CONSTRAINT FK_Invoice_Admin FOREIGN KEY (CreatedByAdminId) REFERENCES dbo.[User] (UserName);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Invoice_CheckInLog')
    ALTER TABLE dbo.Invoice ADD CONSTRAINT FK_Invoice_CheckInLog FOREIGN KEY (CheckInLogCheckInId) REFERENCES dbo.CheckInLog (CheckInId);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MemberBodyMetric_User')
    ALTER TABLE dbo.MemberBodyMetric ADD CONSTRAINT FK_MemberBodyMetric_User FOREIGN KEY (UserName) REFERENCES dbo.[User] (UserName) ON UPDATE CASCADE;

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MemberBodyMetric_PTSession')
    ALTER TABLE dbo.MemberBodyMetric ADD CONSTRAINT FK_MemberBodyMetric_PTSession FOREIGN KEY (PTSessionId) REFERENCES dbo.PTTrainingSession (PTSessionId);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MemberBodyMetric_RecordedByPt')
    ALTER TABLE dbo.MemberBodyMetric ADD CONSTRAINT FK_MemberBodyMetric_RecordedByPt FOREIGN KEY (RecordedByPtId) REFERENCES dbo.[User] (UserName);
GO

-- 4. NẠP DỮ LIỆU NỀN TẢNG (SEED DATA) & CẬP NHẬT UNICODE (NVARCHAR)

-- 4.0. Tự động chuyển đổi các cột văn bản sang NVARCHAR nếu bảng đã được tạo trước đó
IF OBJECT_ID('dbo.Role', 'U') IS NOT NULL
    ALTER TABLE dbo.Role ALTER COLUMN RoleName NVARCHAR(50) NULL;

IF OBJECT_ID('dbo.[User]', 'U') IS NOT NULL
    ALTER TABLE dbo.[User] ALTER COLUMN FullName NVARCHAR(50) NULL;

IF OBJECT_ID('dbo.PackagePlan', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.PackagePlan ALTER COLUMN Name NVARCHAR(50) NULL;
    ALTER TABLE dbo.PackagePlan ALTER COLUMN Description NVARCHAR(255) NULL;
END

IF OBJECT_ID('dbo.PTTrainingSession', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.PTTrainingSession ALTER COLUMN WorkoutNotes NVARCHAR(255) NULL;
    ALTER TABLE dbo.PTTrainingSession ALTER COLUMN ReviewComment NVARCHAR(255) NULL;
END

IF OBJECT_ID('dbo.CheckInLog', 'U') IS NOT NULL
    ALTER TABLE dbo.CheckInLog ALTER COLUMN DenialReason NVARCHAR(100) NULL;

IF OBJECT_ID('dbo.Invoice', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Invoice ALTER COLUMN TransactionType NVARCHAR(50) NULL;
    ALTER TABLE dbo.Invoice ALTER COLUMN PaymentMethod NVARCHAR(100) NULL;
    ALTER TABLE dbo.Invoice ALTER COLUMN RefundReason NVARCHAR(255) NULL;
END

IF OBJECT_ID('dbo.MemberBodyMetric', 'U') IS NOT NULL
    ALTER TABLE dbo.MemberBodyMetric ALTER COLUMN Notes NVARCHAR(255) NULL;
GO

-- 4.1. Nạp danh mục vai trò (Roles)
IF NOT EXISTS (SELECT 1 FROM dbo.Role WHERE RoleID = 'ADMIN')
    INSERT INTO dbo.Role (RoleID, RoleName) VALUES ('ADMIN', N'Quản trị viên');
ELSE
    UPDATE dbo.Role SET RoleName = N'Quản trị viên' WHERE RoleID = 'ADMIN';

IF NOT EXISTS (SELECT 1 FROM dbo.Role WHERE RoleID = 'PT')
    INSERT INTO dbo.Role (RoleID, RoleName) VALUES ('PT', N'Huấn luyện viên');
ELSE
    UPDATE dbo.Role SET RoleName = N'Huấn luyện viên' WHERE RoleID = 'PT';

IF NOT EXISTS (SELECT 1 FROM dbo.Role WHERE RoleID = 'MEMBER')
    INSERT INTO dbo.Role (RoleID, RoleName) VALUES ('MEMBER', N'Hội viên');
ELSE
    UPDATE dbo.Role SET RoleName = N'Hội viên' WHERE RoleID = 'MEMBER';
GO

-- 4.2. Nạp người dùng mặc định (Mật khẩu băm BCrypt chuẩn xác cho 'admin123')
-- Hash BCrypt: $2a$11$zHVoiwSnJ8lNoqdDi11jCubfv5Q7aiecT7kpeSCTOY4FKMeYlWjyu (Verify("admin123") == true)
IF NOT EXISTS (SELECT 1 FROM dbo.[User] WHERE UserName = 'admin')
BEGIN
    INSERT INTO dbo.[User] (UserName, RoleID, FullName, Phone, Email, PasswordHash, Gender, Status, created_at)
    VALUES (
        'admin',
        'ADMIN',
        N'Quản Trị Hệ Thống',
        '0901234567',
        'admin@gymsystem.vn',
        '$2a$11$zHVoiwSnJ8lNoqdDi11jCubfv5Q7aiecT7kpeSCTOY4FKMeYlWjyu',
        1,
        'ACTIVE',
        CAST(GETDATE() AS DATE)
    );
END
ELSE
BEGIN
    UPDATE dbo.[User] SET FullName = N'Quản Trị Hệ Thống' WHERE UserName = 'admin';
END

IF NOT EXISTS (SELECT 1 FROM dbo.[User] WHERE UserName = 'pt01')
BEGIN
    INSERT INTO dbo.[User] (UserName, RoleID, FullName, Phone, Email, PasswordHash, Gender, Status, created_at)
    VALUES (
        'pt01',
        'PT',
        N'Nguyễn Văn Thành (PT)',
        '0912345678',
        'thanh.pt@gymsystem.vn',
        '$2a$11$zHVoiwSnJ8lNoqdDi11jCubfv5Q7aiecT7kpeSCTOY4FKMeYlWjyu',
        1,
        'ACTIVE',
        CAST(GETDATE() AS DATE)
    );
END
ELSE
BEGIN
    UPDATE dbo.[User] SET FullName = N'Nguyễn Văn Thành (PT)' WHERE UserName = 'pt01';
END

IF NOT EXISTS (SELECT 1 FROM dbo.[User] WHERE UserName = 'member01')
BEGIN
    INSERT INTO dbo.[User] (UserName, RoleID, FullName, Phone, Email, PasswordHash, Gender, Status, created_at)
    VALUES (
        'member01',
        'MEMBER',
        N'Trần Văn An',
        '0987654321',
        'an.tran@example.com',
        '$2a$11$zHVoiwSnJ8lNoqdDi11jCubfv5Q7aiecT7kpeSCTOY4FKMeYlWjyu',
        1,
        'ACTIVE',
        CAST(GETDATE() AS DATE)
    );
END
ELSE
BEGIN
    UPDATE dbo.[User] SET FullName = N'Trần Văn An' WHERE UserName = 'member01';
END
GO

-- 4.3. Nạp danh mục gói tập chuẩn (Package Plans)
IF NOT EXISTS (SELECT 1 FROM dbo.PackagePlan WHERE PlanId = 'PLAN01')
BEGIN
    INSERT INTO dbo.PackagePlan (PlanId, Name, Description, DurationDays, Price, MaxCheckInSessions, is_PtIncluded, TotalPtSessions, is_Active)
    VALUES ('PLAN01', N'Gói 1 Tháng Tiêu Chuẩn', N'Tập gym không giới hạn giờ trong 30 ngày', 30, 500000, 30, 0, 0, 1);
END
ELSE
BEGIN
    UPDATE dbo.PackagePlan SET Name = N'Gói 1 Tháng Tiêu Chuẩn', Description = N'Tập gym không giới hạn giờ trong 30 ngày' WHERE PlanId = 'PLAN01';
END

IF NOT EXISTS (SELECT 1 FROM dbo.PackagePlan WHERE PlanId = 'PLAN02')
BEGIN
    INSERT INTO dbo.PackagePlan (PlanId, Name, Description, DurationDays, Price, MaxCheckInSessions, is_PtIncluded, TotalPtSessions, is_Active)
    VALUES ('PLAN02', N'Gói 3 Tháng Thể Thao', N'Gồm 90 ngày tập gym kèm 5 buổi PT định hướng', 90, 1350000, 90, 1, 5, 1);
END
ELSE
BEGIN
    UPDATE dbo.PackagePlan SET Name = N'Gói 3 Tháng Thể Thao', Description = N'Gồm 90 ngày tập gym kèm 5 buổi PT định hướng' WHERE PlanId = 'PLAN02';
END

IF NOT EXISTS (SELECT 1 FROM dbo.PackagePlan WHERE PlanId = 'PLAN03')
BEGIN
    INSERT INTO dbo.PackagePlan (PlanId, Name, Description, DurationDays, Price, MaxCheckInSessions, is_PtIncluded, TotalPtSessions, is_Active)
    VALUES ('PLAN03', N'Gói 6 Tháng Platinum', N'Tập gym 180 ngày kèm 10 buổi PT và tủ locker riêng', 180, 2500000, 180, 1, 10, 1);
END
ELSE
BEGIN
    UPDATE dbo.PackagePlan SET Name = N'Gói 6 Tháng Platinum', Description = N'Tập gym 180 ngày kèm 10 buổi PT và tủ locker riêng' WHERE PlanId = 'PLAN03';
END

IF NOT EXISTS (SELECT 1 FROM dbo.PackagePlan WHERE PlanId = 'PLAN04')
BEGIN
    INSERT INTO dbo.PackagePlan (PlanId, Name, Description, DurationDays, Price, MaxCheckInSessions, is_PtIncluded, TotalPtSessions, is_Active)
    VALUES ('PLAN04', N'Gói 1 Năm VIP', N'Tập gym 365 ngày kèm 20 buổi PT và phòng xông hơi', 365, 4500000, 365, 1, 20, 1);
END
ELSE
BEGIN
    UPDATE dbo.PackagePlan SET Name = N'Gói 1 Năm VIP', Description = N'Tập gym 365 ngày kèm 20 buổi PT và phòng xông hơi' WHERE PlanId = 'PLAN04';
END
GO

PRINT N'✅ Khởi tạo CSDL GymManagementDb và nạp dữ liệu nền tảng hoàn tất thành công!';
GO
