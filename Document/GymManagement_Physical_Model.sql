-- ====================================================================
-- GYM MANAGEMENT SYSTEM - DDL SCRIPT (CHUẨN 100% THEO ẢNH ERD)
-- Hệ quản trị đích: SQL Server
-- ====================================================================

-- 1. XÓA BẢNG NẾU ĐÃ TỒN TẠI (Theo thứ tự từ bảng con đến bảng cha)
IF OBJECT_ID('dbo.MemberBodyMetric', 'U') IS NOT NULL DROP TABLE dbo.MemberBodyMetric;
IF OBJECT_ID('dbo.Invoice', 'U') IS NOT NULL DROP TABLE dbo.Invoice;
IF OBJECT_ID('dbo.CheckInLog', 'U') IS NOT NULL DROP TABLE dbo.CheckInLog;
IF OBJECT_ID('dbo.MemberSubscription', 'U') IS NOT NULL DROP TABLE dbo.MemberSubscription;
IF OBJECT_ID('dbo.PTTrainingSession', 'U') IS NOT NULL DROP TABLE dbo.PTTrainingSession;
IF OBJECT_ID('dbo.PTAvailability', 'U') IS NOT NULL DROP TABLE dbo.PTAvailability;
IF OBJECT_ID('dbo.PackagePlan', 'U') IS NOT NULL DROP TABLE dbo.PackagePlan;
IF OBJECT_ID('dbo.[User]', 'U') IS NOT NULL DROP TABLE dbo.[User];
IF OBJECT_ID('dbo.Role', 'U') IS NOT NULL DROP TABLE dbo.Role;
GO

-- --------------------------------------------------------------------
-- 2. TẠO CÁC BẢNG (Đầy đủ thuộc tính, kiểu dữ liệu và cờ Nullable)
-- --------------------------------------------------------------------

-- 1. Bảng Role
CREATE TABLE dbo.Role (
    RoleID VARCHAR(10) NOT NULL,
    RoleName VARCHAR(50) NULL,
    CONSTRAINT PK_Role PRIMARY KEY (RoleID)
);
GO

-- 2. Bảng User
CREATE TABLE dbo.[User] (
    UserName VARCHAR(10) NOT NULL,
    RoleID VARCHAR(10) NOT NULL,
    FullName VARCHAR(50) NULL,
    Phone VARCHAR(10) NULL,
    Email VARCHAR(255) NULL,
    PasswordHash VARCHAR(255) NULL,
    Gender TINYINT NULL,
    Status VARCHAR(20) NULL,
    created_at DATE NULL,
    CONSTRAINT PK_User PRIMARY KEY (UserID)
);
GO

-- 3. Bảng PackagePlan
CREATE TABLE dbo.PackagePlan (
    PlanId VARCHAR(10) NOT NULL,
    Name VARCHAR(50) NULL,
    Description VARCHAR(255) NULL,
    DurationDays INT NULL,
    Price DECIMAL(19, 0) NULL,
    MaxCheckInSessions INT NULL,
    is_PtIncluded TINYINT NULL,
    TotalPtSessions INT NULL,
    is_Active TINYINT NULL,
    CONSTRAINT PK_PackagePlan PRIMARY KEY (PlanId)
);
GO

-- 4. Bảng PTAvailability
CREATE TABLE dbo.PTAvailability (
    AvailabilityId VARCHAR(10) NOT NULL,
    AvailableDate DATE NULL,
    StartTime TIME(7) NULL,
    EndTime TIME(7) NULL,
    is_Booked TINYINT NULL,
    PtID VARCHAR(10) NOT NULL,
    CONSTRAINT PK_PTAvailability PRIMARY KEY (AvailabilityId)
);
GO

-- 5. Bảng PTTrainingSession
CREATE TABLE dbo.PTTrainingSession (
    PTSessionId VARCHAR(10) NOT NULL,
    SessionStartTime DATE NULL,
    SessionEndTime DATE NULL,
    Status VARCHAR(20) NULL,
    WorkoutNotes VARCHAR(255) NULL,
    RatingStars TINYINT NULL,
    ReviewComment VARCHAR(255) NULL,
    completed_at DATE NULL,
    MemberID VARCHAR(10) NOT NULL,
    PtID VARCHAR(10) NOT NULL,
    AvailabilityId VARCHAR(10) NULL,
    CONSTRAINT PK_PTTrainingSession PRIMARY KEY (PTSessionId)
);
GO

-- 6. Bảng MemberSubscription
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
    UserID VARCHAR(10) NOT NULL,
    AssignedPtId VARCHAR(10) NULL,
    PlanId VARCHAR(10) NOT NULL,
    PTSessionId VARCHAR(10) NULL,
    CONSTRAINT PK_MemberSubscription PRIMARY KEY (SubscriptionId),
    CONSTRAINT UQ_MemberSubscription_QrCode UNIQUE (QrCode)
);
GO

-- 7. Bảng CheckInLog
CREATE TABLE dbo.CheckInLog (
    CheckInId VARCHAR(10) NOT NULL,
    check_in_at DATE NULL,
    Status VARCHAR(20) NULL,
    DenialReason VARCHAR(100) NULL,
    SubscriptionId VARCHAR(10) NOT NULL,
    VerifiedByAdminId VARCHAR(10) NULL,
    UserID VARCHAR(10) NOT NULL,
    CONSTRAINT PK_CheckInLog PRIMARY KEY (CheckInId)
);
GO

-- 8. Bảng Invoice
CREATE TABLE dbo.Invoice (
    InvoiceId VARCHAR(10) NOT NULL,
    InvoiceCode VARCHAR(255) NULL,
    TransactionType VARCHAR(50) NULL,
    SubtotalAmount DECIMAL(19, 0) NULL,
    TotalAmount INT NULL,
    PaymentMethod VARCHAR(100) NULL,
    Status VARCHAR(20) NULL,
    RefundAmount DECIMAL(19, 0) NULL,
    RefundReason VARCHAR(255) NULL,
    paid_at DATE NULL,
    UserID VARCHAR(10) NOT NULL,
    SubscriptionId VARCHAR(10) NOT NULL,
    CreatedByAdminId VARCHAR(10) NULL,
    CheckInLogCheckInId VARCHAR(10) NULL,
    CONSTRAINT PK_Invoice PRIMARY KEY (InvoiceId),
    CONSTRAINT UQ_Invoice_InvoiceCode UNIQUE (InvoiceCode)
);
GO

-- 9. Bảng MemberBodyMetric
CREATE TABLE dbo.MemberBodyMetric (
    BodyMetricId VARCHAR(10) NOT NULL,
    WeightKg DECIMAL(19, 0) NULL,
    HeightCm DECIMAL(19, 0) NULL,
    SkeletalMuscleMass DECIMAL(19, 0) NULL,
    BodyFatPercentage DECIMAL(19, 0) NULL,
    Bmi DECIMAL(19, 0) NULL,
    Notes VARCHAR(255) NULL,
    measured_at DATE NULL,
    UserID VARCHAR(10) NOT NULL,
    PTSessionId VARCHAR(10) NULL,
    RecordedByPtId VARCHAR(10) NULL,
    CONSTRAINT PK_MemberBodyMetric PRIMARY KEY (BodyMetricId)
);
GO

-- --------------------------------------------------------------------
-- 3. RÀNG BUỘC KHÓA NGOẠI (FOREIGN KEY CONSTRAINTS)
-- --------------------------------------------------------------------

-- User -> Role
ALTER TABLE dbo.[User]
    ADD CONSTRAINT FK_User_Role FOREIGN KEY (RoleID) REFERENCES dbo.Role (RoleID);

-- PTAvailability -> User
ALTER TABLE dbo.PTAvailability
    ADD CONSTRAINT FK_PTAvailability_User FOREIGN KEY (PtID) REFERENCES dbo.[User] (UserID);

-- PTTrainingSession -> User & PTAvailability
ALTER TABLE dbo.PTTrainingSession
    ADD CONSTRAINT FK_PTTrainingSession_Member FOREIGN KEY (MemberID) REFERENCES dbo.[User] (UserID),
    CONSTRAINT FK_PTTrainingSession_PT FOREIGN KEY (PtID) REFERENCES dbo.[User] (UserID),
    CONSTRAINT FK_PTTrainingSession_Availability FOREIGN KEY (AvailabilityId) REFERENCES dbo.PTAvailability (AvailabilityId);

-- MemberSubscription -> User, PackagePlan & PTTrainingSession
ALTER TABLE dbo.MemberSubscription
    ADD CONSTRAINT FK_MemberSubscription_User FOREIGN KEY (UserID) REFERENCES dbo.[User] (UserID),
    CONSTRAINT FK_MemberSubscription_AssignedPt FOREIGN KEY (AssignedPtId) REFERENCES dbo.[User] (UserID),
    CONSTRAINT FK_MemberSubscription_PackagePlan FOREIGN KEY (PlanId) REFERENCES dbo.PackagePlan (PlanId),
    CONSTRAINT FK_MemberSubscription_PTSession FOREIGN KEY (PTSessionId) REFERENCES dbo.PTTrainingSession (PTSessionId);

-- CheckInLog -> MemberSubscription & User
ALTER TABLE dbo.CheckInLog
    ADD CONSTRAINT FK_CheckInLog_Subscription FOREIGN KEY (SubscriptionId) REFERENCES dbo.MemberSubscription (SubscriptionId),
    CONSTRAINT FK_CheckInLog_User FOREIGN KEY (UserID) REFERENCES dbo.[User] (UserID),
    CONSTRAINT FK_CheckInLog_Admin FOREIGN KEY (VerifiedByAdminId) REFERENCES dbo.[User] (UserID);

-- Invoice -> User, MemberSubscription & CheckInLog
ALTER TABLE dbo.Invoice
    ADD CONSTRAINT FK_Invoice_User FOREIGN KEY (UserID) REFERENCES dbo.[User] (UserID),
    CONSTRAINT FK_Invoice_Subscription FOREIGN KEY (SubscriptionId) REFERENCES dbo.MemberSubscription (SubscriptionId),
    CONSTRAINT FK_Invoice_Admin FOREIGN KEY (CreatedByAdminId) REFERENCES dbo.[User] (UserID),
    CONSTRAINT FK_Invoice_CheckInLog FOREIGN KEY (CheckInLogCheckInId) REFERENCES dbo.CheckInLog (CheckInId);

-- MemberBodyMetric -> User & PTTrainingSession
ALTER TABLE dbo.MemberBodyMetric
    ADD CONSTRAINT FK_MemberBodyMetric_User FOREIGN KEY (UserID) REFERENCES dbo.[User] (UserID),
    CONSTRAINT FK_MemberBodyMetric_PTSession FOREIGN KEY (PTSessionId) REFERENCES dbo.PTTrainingSession (PTSessionId),
    CONSTRAINT FK_MemberBodyMetric_RecordedByPt FOREIGN KEY (RecordedByPtId) REFERENCES dbo.[User] (UserID);
GO
