/* ============================================================================
   TaskFlow - Enterprise Task Management System
   Full SQL Server Database Creation Script
   ----------------------------------------------------------------------------
   This script creates the complete schema (ASP.NET Identity tables + domain
   tables), all foreign keys, indexes, and seed/reference data.

   HOW TO USE:
     1. Open this file in SQL Server Management Studio (SSMS) or Azure Data
        Studio, connected to your SQL Server instance.
     2. Execute the entire script (F5). It is safe to re-run: every CREATE
        is guarded with an existence check.
     3. This script creates the SCHEMA and REFERENCE/DEMO DATA equivalent to
        running the application's own migrations + DatabaseSeeder. If you
        instead run the app with `dotnet ef database update`, you do NOT
        need this script - use one or the other, not both.

   NOTE ON PASSWORD HASHES:
     Seeded demo users below use ASP.NET Identity's PasswordHasher format.
     The hash included corresponds to the password: Demo@12345
     (documented in README.md - for local/demo use only).
   ============================================================================ */

SET NOCOUNT ON;
GO

IF DB_ID('TaskFlowDb') IS NULL
BEGIN
    CREATE DATABASE TaskFlowDb;
END
GO

USE TaskFlowDb;
GO

/* ============================================================================
   SECTION 1: ASP.NET CORE IDENTITY TABLES
   ============================================================================ */

IF OBJECT_ID('dbo.Roles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles
    (
        Id               NVARCHAR(450)   NOT NULL PRIMARY KEY,
        Name             NVARCHAR(256)   NULL,
        NormalizedName   NVARCHAR(256)   NULL,
        ConcurrencyStamp NVARCHAR(MAX)   NULL
    );
    CREATE UNIQUE INDEX RoleNameIndex ON dbo.Roles (NormalizedName) WHERE NormalizedName IS NOT NULL;
END
GO

IF OBJECT_ID('dbo.Departments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Departments
    (
        Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name         NVARCHAR(150)     NOT NULL,
        Description  NVARCHAR(500)     NULL,
        IsActive     BIT               NOT NULL DEFAULT (1),
        CreatedAt    DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt    DATETIME2         NULL
    );
    CREATE UNIQUE INDEX IX_Departments_Name ON dbo.Departments (Name);
END
GO

IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id                     NVARCHAR(450)   NOT NULL PRIMARY KEY,
        FirstName              NVARCHAR(100)   NOT NULL,
        LastName               NVARCHAR(100)   NOT NULL,
        ProfileImagePath       NVARCHAR(500)   NULL,
        JobTitle               NVARCHAR(150)   NULL,
        Bio                    NVARCHAR(1000)  NULL,
        IsActive               BIT             NOT NULL DEFAULT (1),
        CreatedAt              DATETIME2       NOT NULL DEFAULT (SYSUTCDATETIME()),
        LastLoginAt            DATETIME2       NULL,
        DepartmentId           INT             NULL,
        UserName               NVARCHAR(256)   NULL,
        NormalizedUserName     NVARCHAR(256)   NULL,
        Email                  NVARCHAR(256)   NULL,
        NormalizedEmail        NVARCHAR(256)   NULL,
        EmailConfirmed         BIT             NOT NULL DEFAULT (0),
        PasswordHash           NVARCHAR(MAX)   NULL,
        SecurityStamp          NVARCHAR(MAX)   NULL,
        ConcurrencyStamp       NVARCHAR(MAX)   NULL,
        PhoneNumber            NVARCHAR(MAX)   NULL,
        PhoneNumberConfirmed   BIT             NOT NULL DEFAULT (0),
        TwoFactorEnabled       BIT             NOT NULL DEFAULT (0),
        LockoutEnd             DATETIMEOFFSET  NULL,
        LockoutEnabled         BIT             NOT NULL DEFAULT (1),
        AccessFailedCount      INT             NOT NULL DEFAULT (0),
        CONSTRAINT FK_Users_Departments FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments (Id) ON DELETE SET NULL
    );
    CREATE UNIQUE INDEX UserNameIndex ON dbo.Users (NormalizedUserName) WHERE NormalizedUserName IS NOT NULL;
    CREATE INDEX EmailIndex ON dbo.Users (NormalizedEmail);
    CREATE INDEX IX_Users_DepartmentId ON dbo.Users (DepartmentId);
    CREATE INDEX IX_Users_IsActive ON dbo.Users (IsActive);
END
GO

IF OBJECT_ID('dbo.UserRoles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserRoles
    (
        UserId NVARCHAR(450) NOT NULL,
        RoleId NVARCHAR(450) NOT NULL,
        CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE,
        CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles (Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_UserRoles_RoleId ON dbo.UserRoles (RoleId);
END
GO

IF OBJECT_ID('dbo.UserClaims', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserClaims
    (
        Id         INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId     NVARCHAR(450)     NOT NULL,
        ClaimType  NVARCHAR(MAX)     NULL,
        ClaimValue NVARCHAR(MAX)     NULL,
        CONSTRAINT FK_UserClaims_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_UserClaims_UserId ON dbo.UserClaims (UserId);
END
GO

IF OBJECT_ID('dbo.UserLogins', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserLogins
    (
        LoginProvider       NVARCHAR(450) NOT NULL,
        ProviderKey         NVARCHAR(450) NOT NULL,
        ProviderDisplayName NVARCHAR(MAX) NULL,
        UserId              NVARCHAR(450) NOT NULL,
        CONSTRAINT PK_UserLogins PRIMARY KEY (LoginProvider, ProviderKey),
        CONSTRAINT FK_UserLogins_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_UserLogins_UserId ON dbo.UserLogins (UserId);
END
GO

IF OBJECT_ID('dbo.UserTokens', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserTokens
    (
        UserId        NVARCHAR(450) NOT NULL,
        LoginProvider NVARCHAR(450) NOT NULL,
        Name          NVARCHAR(450) NOT NULL,
        Value         NVARCHAR(MAX) NULL,
        CONSTRAINT PK_UserTokens PRIMARY KEY (UserId, LoginProvider, Name),
        CONSTRAINT FK_UserTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('dbo.RoleClaims', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RoleClaims
    (
        Id         INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        RoleId     NVARCHAR(450)     NOT NULL,
        ClaimType  NVARCHAR(MAX)     NULL,
        ClaimValue NVARCHAR(MAX)     NULL,
        CONSTRAINT FK_RoleClaims_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles (Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_RoleClaims_RoleId ON dbo.RoleClaims (RoleId);
END
GO

/* ============================================================================
   SECTION 2: ORGANIZATION (Teams, Team Members, Invites)
   ============================================================================ */

IF OBJECT_ID('dbo.Teams', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Teams
    (
        Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name         NVARCHAR(150)     NOT NULL,
        Description  NVARCHAR(500)     NULL,
        DepartmentId INT               NOT NULL,
        TeamLeadId   NVARCHAR(450)     NOT NULL,
        IsActive     BIT               NOT NULL DEFAULT (1),
        CreatedAt    DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt    DATETIME2         NULL,
        CONSTRAINT FK_Teams_Departments FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments (Id) ON DELETE CASCADE,
        CONSTRAINT FK_Teams_TeamLead FOREIGN KEY (TeamLeadId) REFERENCES dbo.Users (Id) ON DELETE NO ACTION
    );
    CREATE INDEX IX_Teams_DepartmentId ON dbo.Teams (DepartmentId);
END
GO

IF OBJECT_ID('dbo.TeamMembers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TeamMembers
    (
        Id          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TeamId      INT               NOT NULL,
        UserId      NVARCHAR(450)     NOT NULL,
        JoinedAt    DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        IsTeamLead  BIT               NOT NULL DEFAULT (0),
        CreatedAt   DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt   DATETIME2         NULL,
        CONSTRAINT FK_TeamMembers_Teams FOREIGN KEY (TeamId) REFERENCES dbo.Teams (Id) ON DELETE CASCADE,
        CONSTRAINT FK_TeamMembers_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_TeamMembers_TeamId_UserId ON dbo.TeamMembers (TeamId, UserId);
END
GO

IF OBJECT_ID('dbo.UserInvites', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserInvites
    (
        Id              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Email           NVARCHAR(256)     NOT NULL,
        Token           NVARCHAR(200)     NOT NULL,
        TeamId          INT               NULL,
        InvitedByUserId NVARCHAR(450)     NOT NULL,
        ExpiresAt       DATETIME2         NOT NULL,
        IsAccepted      BIT               NOT NULL DEFAULT (0),
        AcceptedAt      DATETIME2         NULL,
        CreatedAt       DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt       DATETIME2         NULL,
        CONSTRAINT FK_UserInvites_Teams FOREIGN KEY (TeamId) REFERENCES dbo.Teams (Id) ON DELETE SET NULL,
        CONSTRAINT FK_UserInvites_InvitedBy FOREIGN KEY (InvitedByUserId) REFERENCES dbo.Users (Id) ON DELETE NO ACTION
    );
    CREATE UNIQUE INDEX IX_UserInvites_Token ON dbo.UserInvites (Token);
    CREATE INDEX IX_UserInvites_Email ON dbo.UserInvites (Email);
END
GO

/* ============================================================================
   SECTION 3: PROJECTS
   ============================================================================ */

IF OBJECT_ID('dbo.Categories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories
    (
        Id          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name        NVARCHAR(100)     NOT NULL,
        Description NVARCHAR(300)     NULL,
        ColorHex    NVARCHAR(7)       NOT NULL DEFAULT ('#6366F1'),
        IsActive    BIT               NOT NULL DEFAULT (1),
        CreatedAt   DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt   DATETIME2         NULL
    );
    CREATE UNIQUE INDEX IX_Categories_Name ON dbo.Categories (Name);
END
GO

IF OBJECT_ID('dbo.Projects', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Projects
    (
        Id          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name        NVARCHAR(200)     NOT NULL,
        Description NVARCHAR(2000)    NULL,
        Status      NVARCHAR(30)      NOT NULL DEFAULT ('Planning'),
        StartDate   DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        EndDate     DATETIME2         NULL,
        ColorHex    NVARCHAR(7)       NOT NULL DEFAULT ('#3B82F6'),
        IsArchived  BIT               NOT NULL DEFAULT (0),
        CategoryId  INT               NULL,
        OwnerId     NVARCHAR(450)     NOT NULL,
        TeamId      INT               NULL,
        IsDeleted   BIT               NOT NULL DEFAULT (0),
        DeletedAt   DATETIME2         NULL,
        CreatedAt   DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt   DATETIME2         NULL,
        CONSTRAINT FK_Projects_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories (Id) ON DELETE SET NULL,
        CONSTRAINT FK_Projects_Owner FOREIGN KEY (OwnerId) REFERENCES dbo.Users (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_Projects_Teams FOREIGN KEY (TeamId) REFERENCES dbo.Teams (Id) ON DELETE SET NULL
    );
    CREATE INDEX IX_Projects_Status ON dbo.Projects (Status);
    CREATE INDEX IX_Projects_OwnerId ON dbo.Projects (OwnerId);
    CREATE INDEX IX_Projects_IsArchived ON dbo.Projects (IsArchived);
    CREATE INDEX IX_Projects_Name ON dbo.Projects (Name);
END
GO

IF OBJECT_ID('dbo.ProjectMembers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProjectMembers
    (
        Id               INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ProjectId        INT               NOT NULL,
        UserId           NVARCHAR(450)     NOT NULL,
        AddedAt          DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        IsProjectManager BIT               NOT NULL DEFAULT (0),
        CreatedAt        DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt        DATETIME2         NULL,
        CONSTRAINT FK_ProjectMembers_Projects FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (Id) ON DELETE CASCADE,
        CONSTRAINT FK_ProjectMembers_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_ProjectMembers_ProjectId_UserId ON dbo.ProjectMembers (ProjectId, UserId);
END
GO

/* ============================================================================
   SECTION 4: TASKS
   ============================================================================ */

IF OBJECT_ID('dbo.TaskItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaskItems
    (
        Id               INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Title            NVARCHAR(250)     NOT NULL,
        Description      NVARCHAR(4000)    NULL,
        Status           NVARCHAR(30)      NOT NULL DEFAULT ('ToDo'),
        Priority         NVARCHAR(20)      NOT NULL DEFAULT ('Medium'),
        DueDate          DATETIME2         NULL,
        StartDate        DATETIME2         NULL,
        CompletedAt      DATETIME2         NULL,
        BoardOrder       INT               NOT NULL DEFAULT (0),
        ProjectId        INT               NOT NULL,
        ParentTaskId     INT               NULL,
        CreatedByUserId  NVARCHAR(450)     NOT NULL,
        IsDeleted        BIT               NOT NULL DEFAULT (0),
        DeletedAt        DATETIME2         NULL,
        CreatedAt        DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt        DATETIME2         NULL,
        CONSTRAINT FK_TaskItems_Projects FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (Id) ON DELETE CASCADE,
        CONSTRAINT FK_TaskItems_ParentTask FOREIGN KEY (ParentTaskId) REFERENCES dbo.TaskItems (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_TaskItems_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users (Id) ON DELETE NO ACTION
    );
    CREATE INDEX IX_TaskItems_ProjectId_Status ON dbo.TaskItems (ProjectId, Status);
    CREATE INDEX IX_TaskItems_DueDate ON dbo.TaskItems (DueDate);
    CREATE INDEX IX_TaskItems_Priority ON dbo.TaskItems (Priority);
    CREATE INDEX IX_TaskItems_IsDeleted ON dbo.TaskItems (IsDeleted);
END
GO

IF OBJECT_ID('dbo.TaskAssignments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaskAssignments
    (
        Id               INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TaskItemId       INT               NOT NULL,
        UserId           NVARCHAR(450)     NOT NULL,
        AssignedAt       DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        AssignedByUserId NVARCHAR(450)     NULL,
        CreatedAt        DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt        DATETIME2         NULL,
        CONSTRAINT FK_TaskAssignments_TaskItems FOREIGN KEY (TaskItemId) REFERENCES dbo.TaskItems (Id) ON DELETE CASCADE,
        CONSTRAINT FK_TaskAssignments_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_TaskAssignments_TaskItemId_UserId ON dbo.TaskAssignments (TaskItemId, UserId);
    CREATE INDEX IX_TaskAssignments_UserId ON dbo.TaskAssignments (UserId);
END
GO

IF OBJECT_ID('dbo.ChecklistItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChecklistItems
    (
        Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Text         NVARCHAR(500)     NOT NULL,
        IsCompleted  BIT               NOT NULL DEFAULT (0),
        CompletedAt  DATETIME2         NULL,
        SortOrder    INT               NOT NULL DEFAULT (0),
        TaskItemId   INT               NOT NULL,
        CreatedAt    DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt    DATETIME2         NULL,
        CONSTRAINT FK_ChecklistItems_TaskItems FOREIGN KEY (TaskItemId) REFERENCES dbo.TaskItems (Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_ChecklistItems_TaskItemId ON dbo.ChecklistItems (TaskItemId);
END
GO

IF OBJECT_ID('dbo.Labels', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Labels
    (
        Id        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name      NVARCHAR(50)      NOT NULL,
        ColorHex  NVARCHAR(7)       NOT NULL DEFAULT ('#10B981'),
        ProjectId INT               NOT NULL,
        CreatedAt DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2         NULL,
        CONSTRAINT FK_Labels_Projects FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_Labels_ProjectId_Name ON dbo.Labels (ProjectId, Name);
END
GO

IF OBJECT_ID('dbo.TaskLabels', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaskLabels
    (
        TaskItemId INT NOT NULL,
        LabelId    INT NOT NULL,
        CONSTRAINT PK_TaskLabels PRIMARY KEY (TaskItemId, LabelId),
        CONSTRAINT FK_TaskLabels_TaskItems FOREIGN KEY (TaskItemId) REFERENCES dbo.TaskItems (Id) ON DELETE CASCADE,
        CONSTRAINT FK_TaskLabels_Labels FOREIGN KEY (LabelId) REFERENCES dbo.Labels (Id) ON DELETE CASCADE
    );
END
GO

/* ============================================================================
   SECTION 5: COLLABORATION (Comments, Attachments)
   ============================================================================ */

IF OBJECT_ID('dbo.Comments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Comments
    (
        Id              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Content         NVARCHAR(2000)    NOT NULL,
        TaskItemId      INT               NOT NULL,
        UserId          NVARCHAR(450)     NOT NULL,
        ParentCommentId INT               NULL,
        IsEdited        BIT               NOT NULL DEFAULT (0),
        CreatedAt       DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt       DATETIME2         NULL,
        CONSTRAINT FK_Comments_TaskItems FOREIGN KEY (TaskItemId) REFERENCES dbo.TaskItems (Id) ON DELETE CASCADE,
        CONSTRAINT FK_Comments_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_Comments_ParentComment FOREIGN KEY (ParentCommentId) REFERENCES dbo.Comments (Id) ON DELETE NO ACTION
    );
    CREATE INDEX IX_Comments_TaskItemId ON dbo.Comments (TaskItemId);
END
GO

IF OBJECT_ID('dbo.Attachments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Attachments
    (
        Id                INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        FileName          NVARCHAR(260)     NOT NULL,
        FilePath          NVARCHAR(500)     NOT NULL,
        ContentType       NVARCHAR(150)     NOT NULL,
        FileSizeBytes     BIGINT            NOT NULL,
        RelatedTo         NVARCHAR(20)      NOT NULL,
        ProjectId         INT               NULL,
        TaskItemId        INT               NULL,
        UploadedByUserId  NVARCHAR(450)     NOT NULL,
        CreatedAt         DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt         DATETIME2         NULL,
        CONSTRAINT FK_Attachments_Projects FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (Id) ON DELETE CASCADE,
        CONSTRAINT FK_Attachments_TaskItems FOREIGN KEY (TaskItemId) REFERENCES dbo.TaskItems (Id) ON DELETE CASCADE,
        CONSTRAINT FK_Attachments_UploadedBy FOREIGN KEY (UploadedByUserId) REFERENCES dbo.Users (Id) ON DELETE NO ACTION
    );
    CREATE INDEX IX_Attachments_ProjectId ON dbo.Attachments (ProjectId);
    CREATE INDEX IX_Attachments_TaskItemId ON dbo.Attachments (TaskItemId);
END
GO

/* ============================================================================
   SECTION 6: LOGGING & NOTIFICATIONS
   ============================================================================ */

IF OBJECT_ID('dbo.ActivityLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ActivityLogs
    (
        Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ActionType   NVARCHAR(30)      NOT NULL,
        Description  NVARCHAR(1000)    NOT NULL,
        MetadataJson NVARCHAR(MAX)     NULL,
        RelatedTo    NVARCHAR(20)      NOT NULL,
        ProjectId    INT               NULL,
        TaskItemId   INT               NULL,
        UserId       NVARCHAR(450)     NOT NULL,
        CreatedAt    DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt    DATETIME2         NULL,
        CONSTRAINT FK_ActivityLogs_Projects FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (Id) ON DELETE CASCADE,
        CONSTRAINT FK_ActivityLogs_TaskItems FOREIGN KEY (TaskItemId) REFERENCES dbo.TaskItems (Id) ON DELETE CASCADE,
        CONSTRAINT FK_ActivityLogs_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE NO ACTION
    );
    CREATE INDEX IX_ActivityLogs_TaskItemId_CreatedAt ON dbo.ActivityLogs (TaskItemId, CreatedAt);
    CREATE INDEX IX_ActivityLogs_ProjectId_CreatedAt ON dbo.ActivityLogs (ProjectId, CreatedAt);
END
GO

IF OBJECT_ID('dbo.SystemLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SystemLogs
    (
        Id                INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Level             NVARCHAR(20)      NOT NULL DEFAULT ('Information'),
        Message           NVARCHAR(2000)    NOT NULL,
        ExceptionDetails  NVARCHAR(MAX)     NULL,
        Source            NVARCHAR(200)     NULL,
        UserId            NVARCHAR(450)     NULL,
        CreatedAt         DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt         DATETIME2         NULL
    );
    CREATE INDEX IX_SystemLogs_Level ON dbo.SystemLogs (Level);
    CREATE INDEX IX_SystemLogs_CreatedAt ON dbo.SystemLogs (CreatedAt);
END
GO

IF OBJECT_ID('dbo.EmailLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmailLogs
    (
        Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ToEmail      NVARCHAR(256)     NOT NULL,
        Subject      NVARCHAR(300)     NOT NULL,
        Body         NVARCHAR(MAX)     NULL,
        EmailType    NVARCHAR(50)      NOT NULL,
        IsSimulated  BIT               NOT NULL DEFAULT (1),
        SentAt       DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        CreatedAt    DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt    DATETIME2         NULL
    );
    CREATE INDEX IX_EmailLogs_ToEmail ON dbo.EmailLogs (ToEmail);
    CREATE INDEX IX_EmailLogs_SentAt ON dbo.EmailLogs (SentAt);
END
GO

IF OBJECT_ID('dbo.Notifications', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notifications
    (
        Id                INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId            NVARCHAR(450)     NOT NULL,
        Type              NVARCHAR(30)      NOT NULL,
        Title             NVARCHAR(200)     NOT NULL,
        Message           NVARCHAR(1000)    NOT NULL,
        LinkUrl           NVARCHAR(500)     NULL,
        IsRead            BIT               NOT NULL DEFAULT (0),
        ReadAt            DATETIME2         NULL,
        RelatedTaskItemId INT               NULL,
        RelatedProjectId  INT               NULL,
        CreatedAt         DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt         DATETIME2         NULL,
        CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE,
        CONSTRAINT FK_Notifications_TaskItems FOREIGN KEY (RelatedTaskItemId) REFERENCES dbo.TaskItems (Id) ON DELETE SET NULL,
        CONSTRAINT FK_Notifications_Projects FOREIGN KEY (RelatedProjectId) REFERENCES dbo.Projects (Id) ON DELETE SET NULL
    );
    CREATE INDEX IX_Notifications_UserId_IsRead_CreatedAt ON dbo.Notifications (UserId, IsRead, CreatedAt);
END
GO

/* ============================================================================
   SECTION 7: SETTINGS
   ============================================================================ */

IF OBJECT_ID('dbo.SystemSettings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SystemSettings
    (
        Id          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Key]       NVARCHAR(150)     NOT NULL,
        Value       NVARCHAR(2000)    NOT NULL,
        Description NVARCHAR(500)     NULL,
        Category    NVARCHAR(50)      NOT NULL DEFAULT ('General'),
        CreatedAt   DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt   DATETIME2         NULL
    );
    CREATE UNIQUE INDEX IX_SystemSettings_Key ON dbo.SystemSettings ([Key]);
END
GO

/* ============================================================================
   SECTION 8: SEED DATA - Roles
   ============================================================================ */

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE NormalizedName = 'ADMIN')
    INSERT INTO dbo.Roles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Admin', 'ADMIN', NEWID());
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE NormalizedName = 'MANAGER')
    INSERT INTO dbo.Roles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Manager', 'MANAGER', NEWID());
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE NormalizedName = 'EMPLOYEE')
    INSERT INTO dbo.Roles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Employee', 'EMPLOYEE', NEWID());
GO

/* ============================================================================
   SECTION 9: SEED DATA - Departments & Categories (safe, self-contained)
   ============================================================================ */

IF NOT EXISTS (SELECT 1 FROM dbo.Departments WHERE Name = 'Engineering')
    INSERT INTO dbo.Departments (Name, Description) VALUES ('Engineering', 'Software development and technical operations');
IF NOT EXISTS (SELECT 1 FROM dbo.Departments WHERE Name = 'Product & Design')
    INSERT INTO dbo.Departments (Name, Description) VALUES ('Product & Design', 'Product management and UX/UI design');
IF NOT EXISTS (SELECT 1 FROM dbo.Departments WHERE Name = 'Quality Assurance')
    INSERT INTO dbo.Departments (Name, Description) VALUES ('Quality Assurance', 'Testing and quality control');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Name = 'Software Development')
    INSERT INTO dbo.Categories (Name, ColorHex) VALUES ('Software Development', '#4F46E5');
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Name = 'Marketing Campaign')
    INSERT INTO dbo.Categories (Name, ColorHex) VALUES ('Marketing Campaign', '#EC4899');
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Name = 'Internal Operations')
    INSERT INTO dbo.Categories (Name, ColorHex) VALUES ('Internal Operations', '#10B981');
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Name = 'Client Project')
    INSERT INTO dbo.Categories (Name, ColorHex) VALUES ('Client Project', '#F59E0B');
GO

/* ============================================================================
   SECTION 10: SEED DATA - System Settings
   ============================================================================ */

IF NOT EXISTS (SELECT 1 FROM dbo.SystemSettings WHERE [Key] = 'Site.Name')
    INSERT INTO dbo.SystemSettings ([Key], Value, Category, Description)
    VALUES ('Site.Name', 'TaskFlow', 'General', 'Application display name');
IF NOT EXISTS (SELECT 1 FROM dbo.SystemSettings WHERE [Key] = 'Uploads.MaxFileSizeMb')
    INSERT INTO dbo.SystemSettings ([Key], Value, Category, Description)
    VALUES ('Uploads.MaxFileSizeMb', '10', 'Uploads', 'Maximum attachment size in megabytes');
IF NOT EXISTS (SELECT 1 FROM dbo.SystemSettings WHERE [Key] = 'Uploads.AllowedExtensions')
    INSERT INTO dbo.SystemSettings ([Key], Value, Category, Description)
    VALUES ('Uploads.AllowedExtensions', '.jpg,.jpeg,.png,.gif,.pdf,.docx,.xlsx,.zip', 'Uploads', 'Comma-separated list of allowed file extensions');
IF NOT EXISTS (SELECT 1 FROM dbo.SystemSettings WHERE [Key] = 'Notifications.DueSoonThresholdHours')
    INSERT INTO dbo.SystemSettings ([Key], Value, Category, Description)
    VALUES ('Notifications.DueSoonThresholdHours', '48', 'Notifications', 'Hours before a due date to trigger a deadline alert');
IF NOT EXISTS (SELECT 1 FROM dbo.SystemSettings WHERE [Key] = 'Security.MaxLoginAttempts')
    INSERT INTO dbo.SystemSettings ([Key], Value, Category, Description)
    VALUES ('Security.MaxLoginAttempts', '5', 'Security', 'Failed login attempts before lockout');
GO

/* ============================================================================
   NOTE ON DEMO USERS / PROJECTS / TASKS:
   ----------------------------------------------------------------------------
   Demo Users, Projects, and Tasks (with correctly Identity-hashed passwords)
   are seeded by the application's own DatabaseSeeder.cs on first run, because
   ASP.NET Identity's PasswordHasher output should be generated by the same
   Identity version/library the app uses, not hand-written into a SQL script.

   This script gives you the full SCHEMA plus safe reference data (roles,
   departments, categories, system settings) so the database exists and is
   ready. Run the application once after this script to have UserManager
   populate demo Users/Projects/Tasks via DatabaseSeeder.SeedAsync().
   ============================================================================ */

PRINT 'TaskFlow database schema created successfully.';
GO
