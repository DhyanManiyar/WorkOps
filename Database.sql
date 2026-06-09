-- ====================================================
-- WORKOPS DATABASE CREATION SCRIPT
-- Database: WorkOpsDB
-- Server: Your Server Instance Name
-- Authentication: Windows Authentication
-- Created: WorkOps Smart Company Management System
-- ====================================================

-- ====================================================
-- 1. CREATE DATABASE
-- ====================================================
USE master;
GO

IF EXISTS(SELECT * FROM sys.databases WHERE name = 'WorkOpsDB')
BEGIN
    ALTER DATABASE WorkOpsDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE WorkOpsDB;
END
GO

CREATE DATABASE WorkOpsDB;
GO

USE WorkOpsDB;
GO

-- ====================================================
-- 2. CREATE TABLES (SEPARATE BATCHES)
-- ====================================================

-- Roles Table
CREATE TABLE Roles (
    RoleID INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(200),
    CreatedDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);
GO

-- Users Table (Authentication)
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    RoleID INT NOT NULL,
    IsActive BIT DEFAULT 1,
    LastLoginDate DATETIME,
    CreatedDate DATETIME DEFAULT GETDATE(),
    FailedLoginAttempts INT DEFAULT 0,
    IsLocked BIT DEFAULT 0,
    CONSTRAINT CHK_Users_Email CHECK (Email LIKE '%@%.%')
);
GO

-- Departments Table
CREATE TABLE Departments (
    DepartmentID INT PRIMARY KEY IDENTITY(1,1),
    DepartmentName NVARCHAR(100) NOT NULL,
    DepartmentCode NVARCHAR(20) UNIQUE NOT NULL,
    Description NVARCHAR(500),
    DepartmentHeadID INT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);
GO

-- Employees Table (Extended User Information)
CREATE TABLE Employees (
    EmployeeID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT UNIQUE NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Phone NVARCHAR(20),
    DepartmentID INT NOT NULL,
    ManagerID INT NULL,
    Position NVARCHAR(100),
    JoiningDate DATE NOT NULL,
    Salary DECIMAL(18,2) NULL,
    Address NVARCHAR(500),
    DateOfBirth DATE,
    ProfilePicture NVARCHAR(255),
    EmergencyContact NVARCHAR(100),
    EmergencyPhone NVARCHAR(20),
    CreatedDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);
GO

-- WorkTasks Table (Renamed from Tasks)
CREATE TABLE WorkTasks (
    TaskID INT PRIMARY KEY IDENTITY(1,1),
    TaskCode NVARCHAR(50) UNIQUE NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Priority INT NOT NULL DEFAULT 2,
    Status INT NOT NULL DEFAULT 1,
    AssignedTo INT NOT NULL,
    AssignedBy INT NOT NULL,
    DepartmentID INT NOT NULL,
    StartDate DATE,
    DueDate DATE NOT NULL,
    CompletedDate DATETIME NULL,
    EstimatedHours DECIMAL(5,2),
    ActualHours DECIMAL(5,2),
    CreatedDate DATETIME DEFAULT GETDATE(),
    LastUpdatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT CHK_WorkTasks_Priority CHECK (Priority BETWEEN 1 AND 4),
    CONSTRAINT CHK_WorkTasks_Status CHECK (Status BETWEEN 1 AND 5),
    CONSTRAINT CHK_WorkTasks_DueDate CHECK (DueDate >= StartDate)
);
GO

-- Task Comments Table
CREATE TABLE TaskComments (
    CommentID INT PRIMARY KEY IDENTITY(1,1),
    TaskID INT NOT NULL,
    EmployeeID INT NOT NULL,
    CommentText NVARCHAR(MAX) NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    IsEdited BIT DEFAULT 0,
    EditedDate DATETIME NULL
);
GO

-- Task Documents Table
CREATE TABLE TaskDocuments (
    DocumentID INT PRIMARY KEY IDENTITY(1,1),
    TaskID INT NOT NULL,
    EmployeeID INT NOT NULL,
    DocumentName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    FileSize BIGINT,
    FileType NVARCHAR(50),
    UploadDate DATETIME DEFAULT GETDATE(),
    Description NVARCHAR(500)
);
GO

-- Attendance Table
CREATE TABLE Attendance (
    AttendanceID INT PRIMARY KEY IDENTITY(1,1),
    EmployeeID INT NOT NULL,
    AttendanceDate DATE NOT NULL,
    CheckInTime DATETIME NULL,
    CheckOutTime DATETIME NULL,
    TotalHours DECIMAL(5,2),
    Status INT NOT NULL DEFAULT 1,
    LateMinutes INT DEFAULT 0,
    OvertimeHours DECIMAL(5,2) DEFAULT 0,
    Notes NVARCHAR(500),
    CreatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT UNQ_Attendance_EmployeeDate UNIQUE (EmployeeID, AttendanceDate),
    CONSTRAINT CHK_Attendance_Status CHECK (Status BETWEEN 1 AND 5)
);
GO

-- Leave Types Table
CREATE TABLE LeaveTypes (
    LeaveTypeID INT PRIMARY KEY IDENTITY(1,1),
    TypeName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(200),
    MaxDaysPerYear INT NOT NULL,
    IsPaid BIT DEFAULT 1,
    RequiresDocument BIT DEFAULT 0,
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO

-- Leave Requests Table
CREATE TABLE LeaveRequests (
    LeaveRequestID INT PRIMARY KEY IDENTITY(1,1),
    EmployeeID INT NOT NULL,
    LeaveTypeID INT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    TotalDays INT NOT NULL,
    Reason NVARCHAR(500),
    Status INT NOT NULL DEFAULT 1,
    ApprovedBy INT NULL,
    ApprovedDate DATETIME NULL,
    RejectionReason NVARCHAR(500),
    AttachmentPath NVARCHAR(500),
    AppliedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT CHK_LeaveRequests_EndDate CHECK (EndDate >= StartDate),
    CONSTRAINT CHK_LeaveRequests_Status CHECK (Status BETWEEN 1 AND 4)
);
GO

-- Leave Balances Table
CREATE TABLE LeaveBalances (
    BalanceID INT PRIMARY KEY IDENTITY(1,1),
    EmployeeID INT NOT NULL,
    LeaveTypeID INT NOT NULL,
    Year INT NOT NULL,
    TotalDays INT NOT NULL,
    UsedDays INT DEFAULT 0,
    RemainingDays AS (TotalDays - UsedDays),
    CONSTRAINT UNQ_LeaveBalance_EmployeeTypeYear UNIQUE (EmployeeID, LeaveTypeID, Year)
);
GO

-- Notifications Table
CREATE TABLE Notifications (
    NotificationID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(500) NOT NULL,
    NotificationType INT NOT NULL,
    IsRead BIT DEFAULT 0,
    Link NVARCHAR(500),
    CreatedDate DATETIME DEFAULT GETDATE(),
    ReadDate DATETIME NULL
);
GO

-- Activity Logs Table
CREATE TABLE ActivityLogs (
    LogID BIGINT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    ActivityType NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    IPAddress NVARCHAR(50),
    UserAgent NVARCHAR(500),
    LogDate DATETIME DEFAULT GETDATE(),
    AffectedRecordID INT NULL,
    AffectedTable NVARCHAR(100)
);
GO

-- Holidays Table
CREATE TABLE Holidays (
    HolidayID INT PRIMARY KEY IDENTITY(1,1),
    HolidayName NVARCHAR(100) NOT NULL,
    HolidayDate DATE NOT NULL UNIQUE,
    Description NVARCHAR(500),
    IsRecurring BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO

-- Settings Table
CREATE TABLE Settings (
    SettingID INT PRIMARY KEY IDENTITY(1,1),
    SettingKey NVARCHAR(100) NOT NULL UNIQUE,
    SettingValue NVARCHAR(MAX),
    SettingType NVARCHAR(50),
    Category NVARCHAR(100),
    Description NVARCHAR(500),
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME DEFAULT GETDATE()
);
GO

-- Performance Reviews Table
CREATE TABLE PerformanceReviews (
    ReviewID INT PRIMARY KEY IDENTITY(1,1),
    EmployeeID INT NOT NULL,
    ReviewDate DATE NOT NULL,
    ReviewerID INT NOT NULL,
    Rating DECIMAL(3,1) NOT NULL,
    Comments NVARCHAR(MAX),
    Strengths NVARCHAR(MAX),
    AreasForImprovement NVARCHAR(MAX),
    Goals NVARCHAR(MAX),
    NextReviewDate DATE,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT CHK_PerformanceReviews_Rating CHECK (Rating BETWEEN 1 AND 5)
);
GO

-- ====================================================
-- 3. ADD FOREIGN KEY CONSTRAINTS (AFTER ALL TABLES)
-- ====================================================

-- Users foreign keys
ALTER TABLE Users ADD CONSTRAINT FK_Users_Roles 
FOREIGN KEY (RoleID) REFERENCES Roles(RoleID);
GO

-- Employees foreign keys
ALTER TABLE Employees ADD CONSTRAINT FK_Employees_Users 
FOREIGN KEY (UserID) REFERENCES Users(UserID);
GO

ALTER TABLE Employees ADD CONSTRAINT FK_Employees_Departments 
FOREIGN KEY (DepartmentID) REFERENCES Departments(DepartmentID);
GO

ALTER TABLE Employees ADD CONSTRAINT FK_Employees_Managers 
FOREIGN KEY (ManagerID) REFERENCES Employees(EmployeeID);
GO

-- Departments foreign key for Department Head
ALTER TABLE Departments ADD CONSTRAINT FK_Departments_DepartmentHead 
FOREIGN KEY (DepartmentHeadID) REFERENCES Employees(EmployeeID);
GO

-- WorkTasks foreign keys
ALTER TABLE WorkTasks ADD CONSTRAINT FK_WorkTasks_AssignedTo 
FOREIGN KEY (AssignedTo) REFERENCES Employees(EmployeeID);
GO

ALTER TABLE WorkTasks ADD CONSTRAINT FK_WorkTasks_AssignedBy 
FOREIGN KEY (AssignedBy) REFERENCES Employees(EmployeeID);
GO

ALTER TABLE WorkTasks ADD CONSTRAINT FK_WorkTasks_Departments 
FOREIGN KEY (DepartmentID) REFERENCES Departments(DepartmentID);
GO

-- Task Comments foreign keys
ALTER TABLE TaskComments ADD CONSTRAINT FK_TaskComments_WorkTasks 
FOREIGN KEY (TaskID) REFERENCES WorkTasks(TaskID);
GO

ALTER TABLE TaskComments ADD CONSTRAINT FK_TaskComments_Employees 
FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID);
GO

-- Task Documents foreign keys
ALTER TABLE TaskDocuments ADD CONSTRAINT FK_TaskDocuments_WorkTasks 
FOREIGN KEY (TaskID) REFERENCES WorkTasks(TaskID);
GO

ALTER TABLE TaskDocuments ADD CONSTRAINT FK_TaskDocuments_Employees 
FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID);
GO

-- Attendance foreign keys
ALTER TABLE Attendance ADD CONSTRAINT FK_Attendance_Employees 
FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID);
GO

-- Leave Requests foreign keys
ALTER TABLE LeaveRequests ADD CONSTRAINT FK_LeaveRequests_Employees 
FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID);
GO

ALTER TABLE LeaveRequests ADD CONSTRAINT FK_LeaveRequests_LeaveTypes 
FOREIGN KEY (LeaveTypeID) REFERENCES LeaveTypes(LeaveTypeID);
GO

ALTER TABLE LeaveRequests ADD CONSTRAINT FK_LeaveRequests_ApprovedBy 
FOREIGN KEY (ApprovedBy) REFERENCES Employees(EmployeeID);
GO

-- Leave Balances foreign keys
ALTER TABLE LeaveBalances ADD CONSTRAINT FK_LeaveBalances_Employees 
FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID);
GO

ALTER TABLE LeaveBalances ADD CONSTRAINT FK_LeaveBalances_LeaveTypes 
FOREIGN KEY (LeaveTypeID) REFERENCES LeaveTypes(LeaveTypeID);
GO

-- Notifications foreign keys
ALTER TABLE Notifications ADD CONSTRAINT FK_Notifications_Users 
FOREIGN KEY (UserID) REFERENCES Users(UserID);
GO

-- Activity Logs foreign keys
ALTER TABLE ActivityLogs ADD CONSTRAINT FK_ActivityLogs_Users 
FOREIGN KEY (UserID) REFERENCES Users(UserID);
GO

-- Performance Reviews foreign keys
ALTER TABLE PerformanceReviews ADD CONSTRAINT FK_PerformanceReviews_Employees 
FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID);
GO

ALTER TABLE PerformanceReviews ADD CONSTRAINT FK_PerformanceReviews_Reviewers 
FOREIGN KEY (ReviewerID) REFERENCES Employees(EmployeeID);
GO

-- ====================================================
-- 4. CREATE INDEXES
-- ====================================================

-- Users Indexes
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_RoleID ON Users(RoleID);
GO

-- Employees Indexes
CREATE INDEX IX_Employees_UserID ON Employees(UserID);
CREATE INDEX IX_Employees_DepartmentID ON Employees(DepartmentID);
CREATE INDEX IX_Employees_ManagerID ON Employees(ManagerID);
CREATE INDEX IX_Employees_IsActive ON Employees(IsActive);
GO

-- WorkTasks Indexes
CREATE INDEX IX_WorkTasks_AssignedTo ON WorkTasks(AssignedTo);
CREATE INDEX IX_WorkTasks_AssignedBy ON WorkTasks(AssignedBy);
CREATE INDEX IX_WorkTasks_DepartmentID ON WorkTasks(DepartmentID);
CREATE INDEX IX_WorkTasks_Status ON WorkTasks(Status);
CREATE INDEX IX_WorkTasks_Priority ON WorkTasks(Priority);
CREATE INDEX IX_WorkTasks_DueDate ON WorkTasks(DueDate);
CREATE INDEX IX_WorkTasks_CompletedDate ON WorkTasks(CompletedDate);
GO

-- Attendance Indexes
CREATE INDEX IX_Attendance_EmployeeID ON Attendance(EmployeeID);
CREATE INDEX IX_Attendance_AttendanceDate ON Attendance(AttendanceDate);
CREATE INDEX IX_Attendance_Status ON Attendance(Status);
GO

-- Leave Requests Indexes
CREATE INDEX IX_LeaveRequests_EmployeeID ON LeaveRequests(EmployeeID);
CREATE INDEX IX_LeaveRequests_Status ON LeaveRequests(Status);
CREATE INDEX IX_LeaveRequests_StartDate ON LeaveRequests(StartDate);
GO

-- Notifications Indexes
CREATE INDEX IX_Notifications_UserID ON Notifications(UserID);
CREATE INDEX IX_Notifications_IsRead ON Notifications(IsRead);
CREATE INDEX IX_Notifications_CreatedDate ON Notifications(CreatedDate);
GO

-- Activity Logs Indexes
CREATE INDEX IX_ActivityLogs_UserID ON ActivityLogs(UserID);
CREATE INDEX IX_ActivityLogs_LogDate ON ActivityLogs(LogDate);
CREATE INDEX IX_ActivityLogs_ActivityType ON ActivityLogs(ActivityType);
GO

-- ====================================================
-- 5. CREATE FUNCTIONS (SEPARATE BATCHES)
-- ====================================================

-- Function to Calculate Employee Experience
CREATE FUNCTION fn_CalculateExperience(@EmployeeID INT)
RETURNS NVARCHAR(100)
AS
BEGIN
    DECLARE @JoiningDate DATE;
    DECLARE @Years INT, @Months INT, @Days INT;
    DECLARE @Result NVARCHAR(100);
    
    SELECT @JoiningDate = JoiningDate 
    FROM Employees 
    WHERE EmployeeID = @EmployeeID;
    
    IF @JoiningDate IS NULL
        RETURN 'N/A';
    
    SET @Years = DATEDIFF(YEAR, @JoiningDate, GETDATE());
    SET @Months = DATEDIFF(MONTH, DATEADD(YEAR, @Years, @JoiningDate), GETDATE());
    SET @Days = DATEDIFF(DAY, DATEADD(MONTH, @Months, DATEADD(YEAR, @Years, @JoiningDate)), GETDATE());
    
    SET @Result = CAST(@Years AS NVARCHAR(10)) + ' Years, ' + 
                  CAST(@Months AS NVARCHAR(10)) + ' Months, ' + 
                  CAST(@Days AS NVARCHAR(10)) + ' Days';
    
    RETURN @Result;
END
GO

-- Function to Get Task Priority Color
CREATE FUNCTION fn_GetPriorityColor(@Priority INT)
RETURNS NVARCHAR(20)
AS
BEGIN
    DECLARE @Color NVARCHAR(20);
    
    SET @Color = CASE @Priority
        WHEN 1 THEN 'success'
        WHEN 2 THEN 'info'
        WHEN 3 THEN 'warning'
        WHEN 4 THEN 'danger'
        ELSE 'secondary'
    END;
    
    RETURN @Color;
END
GO

-- Function to Get Task Status Text
CREATE FUNCTION fn_GetStatusText(@Status INT)
RETURNS NVARCHAR(20)
AS
BEGIN
    DECLARE @StatusText NVARCHAR(20);
    
    SET @StatusText = CASE @Status
        WHEN 1 THEN 'Pending'
        WHEN 2 THEN 'In Progress'
        WHEN 3 THEN 'Submitted'
        WHEN 4 THEN 'Approved'
        WHEN 5 THEN 'Completed'
        ELSE 'Unknown'
    END;
    
    RETURN @StatusText;
END
GO

-- ====================================================
-- 6. CREATE STORED PROCEDURES (SEPARATE BATCHES)
-- ====================================================

-- Procedure for User Authentication
CREATE PROCEDURE sp_AuthenticateUser
    @Email NVARCHAR(100),
    @Password NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UserID INT, @PasswordHash NVARCHAR(255), @IsActive BIT, @IsLocked BIT, @FailedAttempts INT;
    
    SELECT 
        @UserID = UserID,
        @PasswordHash = PasswordHash,
        @IsActive = IsActive,
        @IsLocked = IsLocked,
        @FailedAttempts = FailedLoginAttempts
    FROM Users 
    WHERE Email = @Email;
    
    IF @UserID IS NULL
    BEGIN
        SELECT 
            'UserNotFound' AS Result,
            NULL AS UserID,
            NULL AS Email,
            NULL AS RoleID,
            NULL AS RoleName;
        RETURN;
    END
    
    IF @IsLocked = 1
    BEGIN
        SELECT 
            'AccountLocked' AS Result,
            NULL AS UserID,
            NULL AS Email,
            NULL AS RoleID,
            NULL AS RoleName;
        RETURN;
    END
    
    IF @IsActive = 0
    BEGIN
        SELECT 
            'AccountInactive' AS Result,
            NULL AS UserID,
            NULL AS Email,
            NULL AS RoleID,
            NULL AS RoleName;
        RETURN;
    END
    
    IF @PasswordHash = @Password
    BEGIN
        UPDATE Users 
        SET LastLoginDate = GETDATE(),
            FailedLoginAttempts = 0,
            IsLocked = 0
        WHERE UserID = @UserID;
        
        SELECT 
            'Success' AS Result,
            u.UserID,
            u.Email,
            u.RoleID,
            r.RoleName
        FROM Users u
        INNER JOIN Roles r ON u.RoleID = r.RoleID
        WHERE u.UserID = @UserID;
    END
    ELSE
    BEGIN
        UPDATE Users 
        SET FailedLoginAttempts = @FailedAttempts + 1,
            IsLocked = CASE WHEN @FailedAttempts + 1 >= 5 THEN 1 ELSE 0 END
        WHERE UserID = @UserID;
        
        SELECT 
            'InvalidPassword' AS Result,
            NULL AS UserID,
            NULL AS Email,
            NULL AS RoleID,
            NULL AS RoleName;
    END
END
GO

-- Procedure to Get Employee Dashboard Data
CREATE PROCEDURE sp_GetEmployeeDashboardData
    @EmployeeID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        e.EmployeeID,
        e.FirstName + ' ' + e.LastName AS FullName,
        d.DepartmentName,
        e.Position,
        e.ProfilePicture
    FROM Employees e
    INNER JOIN Departments d ON e.DepartmentID = d.DepartmentID
    WHERE e.EmployeeID = @EmployeeID;
    
    SELECT 
        COUNT(CASE WHEN Status = 1 THEN 1 END) AS PendingTasks,
        COUNT(CASE WHEN Status = 2 THEN 1 END) AS InProgressTasks,
        COUNT(CASE WHEN Status = 3 THEN 1 END) AS SubmittedTasks,
        COUNT(CASE WHEN Status IN (4,5) THEN 1 END) AS CompletedTasks,
        COUNT(CASE WHEN DueDate < GETDATE() AND Status NOT IN (4,5) THEN 1 END) AS OverdueTasks
    FROM WorkTasks
    WHERE AssignedTo = @EmployeeID;
    
    SELECT 
        AttendanceDate,
        CheckInTime,
        CheckOutTime,
        Status,
        TotalHours
    FROM Attendance
    WHERE EmployeeID = @EmployeeID 
        AND AttendanceDate = CONVERT(DATE, GETDATE());
    
    SELECT 
        lt.TypeName,
        lb.TotalDays,
        lb.UsedDays,
        lb.RemainingDays
    FROM LeaveBalances lb
    INNER JOIN LeaveTypes lt ON lb.LeaveTypeID = lt.LeaveTypeID
    WHERE lb.EmployeeID = @EmployeeID 
        AND lb.Year = YEAR(GETDATE());
END
GO

-- Procedure to Mark Attendance
CREATE PROCEDURE sp_MarkAttendance
    @EmployeeID INT,
    @Action NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Today DATE = CONVERT(DATE, GETDATE());
    DECLARE @CurrentTime DATETIME = GETDATE();
    DECLARE @AttendanceID INT;
    
    SELECT @AttendanceID = AttendanceID 
    FROM Attendance 
    WHERE EmployeeID = @EmployeeID 
        AND AttendanceDate = @Today;
    
    IF @Action = 'CHECKIN'
    BEGIN
        IF @AttendanceID IS NULL
        BEGIN
            INSERT INTO Attendance (EmployeeID, AttendanceDate, CheckInTime, Status)
            VALUES (@EmployeeID, @Today, @CurrentTime, 1);
            
            DECLARE @LateThreshold TIME = '09:30:00';
            IF CONVERT(TIME, @CurrentTime) > @LateThreshold
            BEGIN
                DECLARE @LateMinutes INT = DATEDIFF(MINUTE, @LateThreshold, CONVERT(TIME, @CurrentTime));
                UPDATE Attendance 
                SET LateMinutes = @LateMinutes 
                WHERE AttendanceID = SCOPE_IDENTITY();
            END
        END
        ELSE
        BEGIN
            UPDATE Attendance 
            SET CheckInTime = @CurrentTime,
                Status = 1
            WHERE AttendanceID = @AttendanceID;
        END
    END
    ELSE IF @Action = 'CHECKOUT'
    BEGIN
        IF @AttendanceID IS NULL
        BEGIN
            INSERT INTO Attendance (EmployeeID, AttendanceDate, CheckOutTime, Status)
            VALUES (@EmployeeID, @Today, @CurrentTime, 1);
        END
        ELSE
        BEGIN
            DECLARE @CheckInTime DATETIME;
            SELECT @CheckInTime = CheckInTime 
            FROM Attendance 
            WHERE AttendanceID = @AttendanceID;
            
            IF @CheckInTime IS NOT NULL
            BEGIN
                DECLARE @TotalHours DECIMAL(5,2) = DATEDIFF(MINUTE, @CheckInTime, @CurrentTime) / 60.0;
                DECLARE @OvertimeHours DECIMAL(5,2) = CASE WHEN @TotalHours > 8 THEN @TotalHours - 8 ELSE 0 END;
                
                UPDATE Attendance 
                SET CheckOutTime = @CurrentTime,
                    TotalHours = @TotalHours,
                    OvertimeHours = @OvertimeHours
                WHERE AttendanceID = @AttendanceID;
            END
            ELSE
            BEGIN
                UPDATE Attendance 
                SET CheckOutTime = @CurrentTime
                WHERE AttendanceID = @AttendanceID;
            END
        END
    END
    
    SELECT 'Success' AS Result, @Today AS AttendanceDate;
END
GO

-- Procedure to Get Manager Dashboard Data
CREATE PROCEDURE sp_GetManagerDashboardData
    @ManagerID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) AS TeamSize
    FROM Employees
    WHERE ManagerID = @ManagerID AND IsActive = 1;
    
    SELECT 
        COUNT(t.TaskID) AS TotalTasks,
        COUNT(CASE WHEN t.Status = 1 THEN 1 END) AS PendingTasks,
        COUNT(CASE WHEN t.Status = 2 THEN 1 END) AS InProgressTasks,
        COUNT(CASE WHEN t.Status = 3 THEN 1 END) AS SubmittedTasks,
        COUNT(CASE WHEN t.DueDate < GETDATE() AND t.Status NOT IN (4,5) THEN 1 END) AS OverdueTasks
    FROM WorkTasks t
    INNER JOIN Employees e ON t.AssignedTo = e.EmployeeID
    WHERE e.ManagerID = @ManagerID;
    
    SELECT 
        lr.LeaveRequestID,
        e.FirstName + ' ' + e.LastName AS EmployeeName,
        lt.TypeName AS LeaveType,
        lr.StartDate,
        lr.EndDate,
        lr.TotalDays,
        lr.Reason
    FROM LeaveRequests lr
    INNER JOIN Employees e ON lr.EmployeeID = e.EmployeeID
    INNER JOIN LeaveTypes lt ON lr.LeaveTypeID = lt.LeaveTypeID
    WHERE e.ManagerID = @ManagerID 
        AND lr.Status = 1
    ORDER BY lr.AppliedDate DESC;
    
    SELECT 
        e.EmployeeID,
        e.FirstName + ' ' + e.LastName AS EmployeeName,
        COUNT(t.TaskID) AS TotalTasks,
        COUNT(CASE WHEN t.Status IN (4,5) THEN 1 END) AS CompletedTasks,
        AVG(CASE WHEN t.CompletedDate IS NOT NULL 
                THEN DATEDIFF(DAY, t.StartDate, t.CompletedDate) 
                END) AS AvgCompletionDays
    FROM Employees e
    LEFT JOIN WorkTasks t ON e.EmployeeID = t.AssignedTo
    WHERE e.ManagerID = @ManagerID
    GROUP BY e.EmployeeID, e.FirstName, e.LastName;
END
GO

-- Procedure to Get Admin Dashboard Data
CREATE PROCEDURE sp_GetAdminDashboardData
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        COUNT(*) AS TotalEmployees,
        COUNT(CASE WHEN e.IsActive = 1 THEN 1 END) AS ActiveEmployees,
        COUNT(CASE WHEN e.IsActive = 0 THEN 1 END) AS InactiveEmployees
    FROM Employees e;
    
    SELECT COUNT(*) AS TotalDepartments
    FROM Departments
    WHERE IsActive = 1;
    
    SELECT 
        COUNT(*) AS TotalTasks,
        COUNT(CASE WHEN Status = 1 THEN 1 END) AS PendingTasks,
        COUNT(CASE WHEN Status = 2 THEN 1 END) AS InProgressTasks,
        COUNT(CASE WHEN Status = 3 THEN 1 END) AS SubmittedTasks,
        COUNT(CASE WHEN Status IN (4,5) THEN 1 END) AS CompletedTasks
    FROM WorkTasks;
    
    SELECT 
        COUNT(CASE WHEN a.Status = 1 THEN 1 END) AS PresentToday,
        COUNT(CASE WHEN a.Status = 2 THEN 1 END) AS AbsentToday,
        COUNT(CASE WHEN a.Status = 3 THEN 1 END) AS HalfDayToday,
        COUNT(CASE WHEN a.Status = 4 THEN 1 END) AS OnLeaveToday
    FROM Attendance a
    WHERE a.AttendanceDate = CONVERT(DATE, GETDATE());
    
    SELECT COUNT(*) AS PendingLeaves
    FROM LeaveRequests
    WHERE Status = 1;
    
    SELECT 
        d.DepartmentName,
        COUNT(e.EmployeeID) AS EmployeeCount
    FROM Departments d
    LEFT JOIN Employees e ON d.DepartmentID = e.DepartmentID AND e.IsActive = 1
    WHERE d.IsActive = 1
    GROUP BY d.DepartmentName
    ORDER BY EmployeeCount DESC;
END
GO

-- ====================================================
-- 7. CREATE TRIGGERS (SEPARATE BATCHES)
-- ====================================================

-- Trigger for Activity Logging on WorkTasks
CREATE TRIGGER trg_WorkTasks_ActivityLog
ON WorkTasks
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UserID INT = NULL;
    DECLARE @ActivityType NVARCHAR(100);
    DECLARE @Description NVARCHAR(MAX);
    DECLARE @TaskID INT;
    
    SET @UserID = 1;
    
    IF EXISTS(SELECT * FROM INSERTED) AND EXISTS(SELECT * FROM DELETED)
    BEGIN
        SET @ActivityType = 'TASK_UPDATED';
        SELECT @TaskID = TaskID FROM INSERTED;
        SET @Description = 'Task ' + CAST(@TaskID AS NVARCHAR(10)) + ' was updated';
    END
    ELSE IF EXISTS(SELECT * FROM INSERTED)
    BEGIN
        SET @ActivityType = 'TASK_CREATED';
        SELECT @TaskID = TaskID FROM INSERTED;
        SET @Description = 'New task ' + CAST(@TaskID AS NVARCHAR(10)) + ' was created';
    END
    ELSE IF EXISTS(SELECT * FROM DELETED)
    BEGIN
        SET @ActivityType = 'TASK_DELETED';
        SELECT @TaskID = TaskID FROM DELETED;
        SET @Description = 'Task ' + CAST(@TaskID AS NVARCHAR(10)) + ' was deleted';
    END
    
    INSERT INTO ActivityLogs (UserID, ActivityType, Description, AffectedRecordID, AffectedTable)
    VALUES (@UserID, @ActivityType, @Description, @TaskID, 'WorkTasks');
END
GO

-- Trigger to Update Task Completed Date
CREATE TRIGGER trg_WorkTasks_UpdateCompletedDate
ON WorkTasks
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    IF UPDATE(Status)
    BEGIN
        UPDATE t
        SET t.CompletedDate = CASE 
                                WHEN i.Status = 5 THEN GETDATE()
                                ELSE NULL 
                              END,
            t.LastUpdatedDate = GETDATE()
        FROM WorkTasks t
        INNER JOIN INSERTED i ON t.TaskID = i.TaskID
        WHERE t.Status <> i.Status;
    END
END
GO

-- Trigger to Create Leave Balance on Employee Creation
CREATE TRIGGER trg_Employees_CreateLeaveBalance
ON Employees
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @EmployeeID INT;
    DECLARE @CurrentYear INT = YEAR(GETDATE());
    
    SELECT @EmployeeID = EmployeeID FROM INSERTED;
    
    INSERT INTO LeaveBalances (EmployeeID, LeaveTypeID, Year, TotalDays)
    SELECT 
        @EmployeeID,
        lt.LeaveTypeID,
        @CurrentYear,
        lt.MaxDaysPerYear
    FROM LeaveTypes lt;
END
GO

-- Trigger to Update Leave Balance on Leave Approval
CREATE TRIGGER trg_LeaveRequests_UpdateLeaveBalance
ON LeaveRequests
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    IF UPDATE(Status)
    BEGIN
        UPDATE lb
        SET lb.UsedDays = lb.UsedDays + i.TotalDays
        FROM LeaveBalances lb
        INNER JOIN INSERTED i ON lb.EmployeeID = i.EmployeeID AND lb.LeaveTypeID = i.LeaveTypeID
        INNER JOIN DELETED d ON i.LeaveRequestID = d.LeaveRequestID
        WHERE i.Status = 2
            AND d.Status = 1
            AND lb.Year = YEAR(i.StartDate);
    END
END
GO

-- ====================================================
-- 8. CREATE VIEWS
-- ====================================================

-- View for Employee Details with Department
CREATE VIEW vw_EmployeeDetails AS
SELECT 
    e.EmployeeID,
    e.FirstName + ' ' + e.LastName AS FullName,
    u.Email AS LoginEmail,
    e.Phone,
    d.DepartmentName,
    e.Position,
    e.JoiningDate,
    e.IsActive,
    m.FirstName + ' ' + m.LastName AS ManagerName,
    r.RoleName
FROM Employees e
INNER JOIN Users u ON e.UserID = u.UserID
INNER JOIN Departments d ON e.DepartmentID = d.DepartmentID
INNER JOIN Roles r ON u.RoleID = r.RoleID
LEFT JOIN Employees m ON e.ManagerID = m.EmployeeID;
GO

-- View for Task Details with Assignee Information
CREATE VIEW vw_TaskDetails AS
SELECT 
    t.TaskID,
    t.TaskCode,
    t.Title,
    t.Description,
    CASE t.Priority
        WHEN 1 THEN 'Low'
        WHEN 2 THEN 'Medium'
        WHEN 3 THEN 'High'
        WHEN 4 THEN 'Critical'
    END AS Priority,
    CASE t.Status
        WHEN 1 THEN 'Pending'
        WHEN 2 THEN 'In Progress'
        WHEN 3 THEN 'Submitted'
        WHEN 4 THEN 'Approved'
        WHEN 5 THEN 'Completed'
    END AS Status,
    ae.FirstName + ' ' + ae.LastName AS AssignedTo,
    ab.FirstName + ' ' + ab.LastName AS AssignedBy,
    d.DepartmentName,
    t.StartDate,
    t.DueDate,
    t.CompletedDate,
    t.EstimatedHours,
    t.ActualHours,
    DATEDIFF(DAY, GETDATE(), t.DueDate) AS DaysRemaining,
    CASE 
        WHEN t.DueDate < GETDATE() AND t.Status NOT IN (4,5) THEN 'Overdue'
        WHEN DATEDIFF(DAY, GETDATE(), t.DueDate) <= 2 AND t.Status NOT IN (4,5) THEN 'Due Soon'
        ELSE 'On Track'
    END AS DeadlineStatus
FROM WorkTasks t
INNER JOIN Employees ae ON t.AssignedTo = ae.EmployeeID
INNER JOIN Employees ab ON t.AssignedBy = ab.EmployeeID
INNER JOIN Departments d ON t.DepartmentID = d.DepartmentID;
GO

-- View for Monthly Attendance Summary
CREATE VIEW vw_MonthlyAttendance AS
SELECT 
    e.EmployeeID,
    e.FirstName + ' ' + e.LastName AS EmployeeName,
    d.DepartmentName,
    YEAR(a.AttendanceDate) AS Year,
    MONTH(a.AttendanceDate) AS Month,
    COUNT(CASE WHEN a.Status = 1 THEN 1 END) AS PresentDays,
    COUNT(CASE WHEN a.Status = 2 THEN 1 END) AS AbsentDays,
    COUNT(CASE WHEN a.Status = 3 THEN 1 END) AS HalfDays,
    COUNT(CASE WHEN a.Status = 4 THEN 1 END) AS LeaveDays,
    SUM(a.TotalHours) AS TotalHours,
    SUM(a.OvertimeHours) AS OvertimeHours
FROM Employees e
INNER JOIN Departments d ON e.DepartmentID = d.DepartmentID
LEFT JOIN Attendance a ON e.EmployeeID = a.EmployeeID
GROUP BY e.EmployeeID, e.FirstName, e.LastName, d.DepartmentName, 
         YEAR(a.AttendanceDate), MONTH(a.AttendanceDate);
GO

-- View for Leave Balance Summary
CREATE VIEW vw_LeaveBalanceSummary AS
SELECT 
    e.EmployeeID,
    e.FirstName + ' ' + e.LastName AS EmployeeName,
    d.DepartmentName,
    lt.TypeName AS LeaveType,
    lb.Year,
    lb.TotalDays,
    lb.UsedDays,
    lb.RemainingDays,
    CAST(ROUND((lb.UsedDays * 100.0 / NULLIF(lb.TotalDays, 0)), 2) AS DECIMAL(5,2)) AS UsagePercentage
FROM Employees e
INNER JOIN Departments d ON e.DepartmentID = d.DepartmentID
INNER JOIN LeaveBalances lb ON e.EmployeeID = lb.EmployeeID
INNER JOIN LeaveTypes lt ON lb.LeaveTypeID = lt.LeaveTypeID
WHERE lb.Year = YEAR(GETDATE());
GO

-- ====================================================
-- 9. INSERT SEED DATA
-- ====================================================

-- Insert Roles
INSERT INTO Roles (RoleName, Description) VALUES
('Admin', 'System Administrator with full access'),
('Manager', 'Department Manager with team management access'),
('Employee', 'Regular employee with limited access');
GO

-- Insert Leave Types
INSERT INTO LeaveTypes (TypeName, Description, MaxDaysPerYear, IsPaid, RequiresDocument) VALUES
('Casual Leave', 'Personal or casual leave', 12, 1, 0),
('Sick Leave', 'Medical or health related leave', 10, 1, 1),
('Paid Leave', 'Earned or privilege leave', 15, 1, 0),
('Maternity Leave', 'Maternity and childcare leave', 180, 1, 1),
('Paternity Leave', 'Paternity leave for fathers', 15, 1, 0),
('Unpaid Leave', 'Leave without pay', 30, 0, 0);
GO

-- Insert Departments
INSERT INTO Departments (DepartmentName, DepartmentCode, Description) VALUES
('Human Resources', 'HR', 'Human Resources Department'),
('Information Technology', 'IT', 'IT and Technical Support'),
('Sales & Marketing', 'SALES', 'Sales and Marketing Department'),
('Finance', 'FIN', 'Finance and Accounting'),
('Operations', 'OPS', 'Operations Management'),
('Customer Support', 'SUPPORT', 'Customer Service and Support');
GO

-- Insert Users
INSERT INTO Users (Email, PasswordHash, RoleID) VALUES
('admin@workops.com', 'Admin@123', 1),
('manager@workops.com', 'Manager@123', 2),
('employee1@workops.com', 'Employee@123', 3),
('employee2@workops.com', 'Employee@123', 3),
('employee3@workops.com', 'Employee@123', 3),
('employee4@workops.com', 'Employee@123', 3);
GO

-- Insert Employees
INSERT INTO Employees (UserID, FirstName, LastName, Phone, DepartmentID, Position, JoiningDate) VALUES
(1, 'System', 'Administrator', '1234567890', 2, 'System Admin', '2023-01-01'),
(2, 'John', 'Manager', '1234567891', 2, 'IT Manager', '2023-01-15'),
(3, 'Alice', 'Johnson', '1234567892', 2, 'Software Developer', '2023-02-01'),
(4, 'Bob', 'Smith', '1234567893', 3, 'Sales Executive', '2023-02-15'),
(5, 'Carol', 'Davis', '1234567894', 4, 'Accountant', '2023-03-01'),
(6, 'David', 'Wilson', '1234567895', 2, 'System Analyst', '2023-03-15');
GO

-- Update ManagerID for employees
UPDATE Employees SET ManagerID = 2 WHERE EmployeeID IN (3, 6);
GO

-- Update Department Heads
UPDATE Departments SET DepartmentHeadID = 2 WHERE DepartmentID = 2;
UPDATE Departments SET DepartmentHeadID = 4 WHERE DepartmentID = 3;
GO

-- Insert Sample WorkTasks
INSERT INTO WorkTasks (TaskCode, Title, Description, Priority, Status, AssignedTo, AssignedBy, DepartmentID, StartDate, DueDate) VALUES
('TASK-001', 'Design Database Schema', 'Design the complete database schema for WorkOps system', 3, 2, 3, 2, 2, '2024-01-01', '2024-01-10'),
('TASK-002', 'Implement User Authentication', 'Create login and registration system with role-based access', 3, 2, 3, 2, 2, '2024-01-05', '2024-01-15'),
('TASK-003', 'Sales Report Generation', 'Generate monthly sales report for Q4 2023', 2, 1, 4, 2, 3, '2024-01-03', '2024-01-12'),
('TASK-004', 'Financial Audit Preparation', 'Prepare documents for annual financial audit', 4, 1, 5, 2, 4, '2024-01-02', '2024-01-20'),
('TASK-005', 'System Performance Optimization', 'Optimize database queries and application performance', 3, 3, 6, 2, 2, '2024-01-01', '2024-01-08'),
('TASK-006', 'Employee Onboarding Portal', 'Develop portal for new employee onboarding', 2, 5, 3, 2, 2, '2023-12-20', '2023-12-31');
GO

-- Insert Sample Attendance
INSERT INTO Attendance (EmployeeID, AttendanceDate, CheckInTime, CheckOutTime, Status, TotalHours) VALUES
(3, '2024-01-01', '2024-01-01 09:00:00', '2024-01-01 17:00:00', 1, 8.0),
(4, '2024-01-01', '2024-01-01 09:15:00', '2024-01-01 17:30:00', 1, 8.25),
(5, '2024-01-01', '2024-01-01 09:05:00', '2024-01-01 16:45:00', 1, 7.67),
(3, '2024-01-02', '2024-01-02 09:10:00', '2024-01-02 17:15:00', 1, 8.08),
(4, '2024-01-02', '2024-01-02 09:00:00', '2024-01-02 17:00:00', 1, 8.0);
GO

-- Insert Sample Leave Requests
INSERT INTO LeaveRequests (EmployeeID, LeaveTypeID, StartDate, EndDate, TotalDays, Reason, Status) VALUES
(3, 1, '2024-01-15', '2024-01-16', 2, 'Family function', 1),
(4, 2, '2024-01-10', '2024-01-12', 3, 'Medical appointment', 2),
(5, 3, '2024-01-20', '2024-01-25', 6, 'Vacation', 1);
GO

-- Insert Sample Notifications
INSERT INTO Notifications (UserID, Title, Message, NotificationType, Link) VALUES
(3, 'New Task Assigned', 'You have been assigned a new task: Design Database Schema', 1, '/Employee/Tasks/Details/1'),
(4, 'Leave Approved', 'Your sick leave request has been approved', 2, '/Employee/Leaves'),
(2, 'Task Submitted', 'Alice Johnson has submitted task for approval', 1, '/Manager/Tasks/Details/5');
GO

-- Insert Sample Holidays
INSERT INTO Holidays (HolidayName, HolidayDate, Description, IsRecurring) VALUES
('New Year''s Day', '2024-01-01', 'New Year Celebration', 1),
('Republic Day', '2024-01-26', 'Republic Day of India', 1),
('Holi', '2024-03-25', 'Festival of Colors', 0),
('Independence Day', '2024-08-15', 'Independence Day of India', 1),
('Diwali', '2024-11-12', 'Festival of Lights', 0);
GO

-- Insert System Settings
INSERT INTO Settings (SettingKey, SettingValue, SettingType, Category, Description) VALUES
('CompanyName', 'WorkOps Inc.', 'String', 'General', 'Company Name'),
('CompanyEmail', 'info@workops.com', 'String', 'General', 'Company Email'),
('WorkStartTime', '09:00', 'Time', 'Attendance', 'Default work start time'),
('WorkEndTime', '18:00', 'Time', 'Attendance', 'Default work end time'),
('LateThreshold', '09:30', 'Time', 'Attendance', 'Time after which employee is marked late'),
('MaxFileSize', '10485760', 'Int', 'File', 'Maximum file upload size in bytes (10MB)'),
('AllowedFileTypes', 'pdf,doc,docx,xls,xlsx,jpg,jpeg,png', 'String', 'File', 'Allowed file extensions'),
('SmtpServer', 'smtp.gmail.com', 'String', 'Email', 'SMTP Server for emails'),
('LeaveApprovalRequired', 'true', 'Bool', 'Leave', 'Whether leave requires manager approval');
GO

-- ====================================================
-- 10. VERIFICATION AND FINAL SETUP
-- ====================================================

-- Update Statistics
EXEC sp_updatestats;
GO

-- Display verification
PRINT '===========================================';
PRINT 'WORKOPS DATABASE CREATED SUCCESSFULLY';
PRINT '===========================================';
PRINT 'Database: WorkOpsDB';
PRINT '===========================================';
PRINT 'DATABASE READY FOR ENTITY FRAMEWORK SETUP';
PRINT '===========================================';
GO

-- Test the authentication procedure
EXEC sp_AuthenticateUser @Email = 'admin@workops.com', @Password = 'Admin@123';
GO