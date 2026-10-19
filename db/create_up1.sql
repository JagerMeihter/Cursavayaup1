-- ============================================================
-- База данных: up1
-- Платформа сбора пожертвований
-- Курсовой / учебная практика УП.01
-- ============================================================

USE master;
GO

IF DB_ID(N'up1') IS NULL
BEGIN
    CREATE DATABASE up1;
END
GO

USE up1;
GO

-- ============================================================
-- 1. Таблица Users
-- ============================================================
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        UserId       INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Username     NVARCHAR(50)  NOT NULL,
        PasswordHash NVARCHAR(256) NOT NULL,
        Email        NVARCHAR(100) NOT NULL,
        FullName     NVARCHAR(150) NOT NULL,
        Role         NVARCHAR(50)  NOT NULL DEFAULT N'User',
        CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE(),
        IsActive     BIT           NOT NULL DEFAULT 1,
        Phone        NVARCHAR(20)  NULL,
        LastLogin    DATETIME      NULL
    );
END
GO

-- Уникальность
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_Users_Email' AND object_id = OBJECT_ID(N'dbo.Users'))
    ALTER TABLE dbo.Users ADD CONSTRAINT UQ_Users_Email UNIQUE (Email);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_Users_Username' AND object_id = OBJECT_ID(N'dbo.Users'))
    ALTER TABLE dbo.Users ADD CONSTRAINT UQ_Users_Username UNIQUE (Username);
GO

-- Дополнительные столбцы (если таблица уже существовала)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = N'Phone')
    ALTER TABLE dbo.Users ADD Phone NVARCHAR(20) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = N'LastLogin')
    ALTER TABLE dbo.Users ADD LastLogin DATETIME NULL;
GO

-- ============================================================
-- 2. Таблица Roles
-- ============================================================
IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles (
        RoleId      INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        RoleName    NVARCHAR(50)  NOT NULL UNIQUE,
        Description NVARCHAR(200) NULL
    );

    INSERT INTO dbo.Roles (RoleName, Description) VALUES
    (N'User',      N'Обычный пользователь'),
    (N'Admin',     N'Администратор системы'),
    (N'Moderator', N'Модератор');
END
GO

-- ============================================================
-- 3. Таблица Categories
-- ============================================================
IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories (
        CategoryId   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CategoryName NVARCHAR(100) NOT NULL UNIQUE,
        Description  NVARCHAR(300) NULL,
        Icon         NVARCHAR(100) NULL
    );

    INSERT INTO dbo.Categories (CategoryName, Description) VALUES
    (N'Медицина',         N'Лечение, операции, реабилитация'),
    (N'Животные',         N'Помощь приютам и животным'),
    (N'Образование',      N'Обучение, стипендии, школы'),
    (N'Экология',         N'Охрана природы'),
    (N'Социальная помощь',N'Помощь нуждающимся'),
    (N'Спорт',            N'Спортивные проекты'),
    (N'Другое',           N'Прочие кампании');
END
GO

-- ============================================================
-- 4. Таблица Campaigns
-- ============================================================
IF OBJECT_ID(N'dbo.Campaigns', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Campaigns (
        CampaignId       INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Title            NVARCHAR(200)  NOT NULL,
        Description      NVARCHAR(MAX)  NOT NULL,
        ShortDescription NVARCHAR(300)  NULL,
        GoalAmount       DECIMAL(18,2)  NOT NULL,
        CurrentAmount    DECIMAL(18,2)  NOT NULL DEFAULT 0,
        CreatorId        INT            NOT NULL,
        Status           NVARCHAR(50)   NOT NULL DEFAULT N'Active',
        CreatedAt        DATETIME       NOT NULL DEFAULT GETDATE(),
        EndDate          DATETIME       NULL,
        ImagePath        NVARCHAR(500)  NULL,
        CategoryId       INT            NULL,
        CONSTRAINT FK_Campaigns_Creator  FOREIGN KEY (CreatorId)  REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_Campaigns_Category FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(CategoryId),
        CONSTRAINT CK_Campaigns_Status   CHECK (Status IN (N'Active', N'Completed', N'Cancelled', N'Draft')),
        CONSTRAINT CK_Campaigns_Goal     CHECK (GoalAmount > 0)
    );
END
GO

-- Дополнительные столбцы (если таблица уже существовала)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Campaigns') AND name = N'ImagePath')
    ALTER TABLE dbo.Campaigns ADD ImagePath NVARCHAR(500) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Campaigns') AND name = N'ShortDescription')
    ALTER TABLE dbo.Campaigns ADD ShortDescription NVARCHAR(300) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Campaigns') AND name = N'CategoryId')
    ALTER TABLE dbo.Campaigns ADD CategoryId INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Campaigns_Category')
    ALTER TABLE dbo.Campaigns ADD CONSTRAINT FK_Campaigns_Category
        FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(CategoryId);
GO

-- ============================================================
-- 5. Таблица Donations
-- ============================================================
IF OBJECT_ID(N'dbo.Donations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Donations (
        DonationId   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CampaignId   INT            NOT NULL,
        UserId       INT            NOT NULL,
        Amount       DECIMAL(18,2)  NOT NULL,
        Message      NVARCHAR(500)  NULL,
        DonationDate DATETIME       NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Donations_Campaign FOREIGN KEY (CampaignId) REFERENCES dbo.Campaigns(CampaignId),
        CONSTRAINT FK_Donations_User     FOREIGN KEY (UserId)     REFERENCES dbo.Users(UserId),
        CONSTRAINT CK_Donations_Amount   CHECK (Amount > 0)
    );
END
GO

-- ============================================================
-- 6. Таблица Comments
-- ============================================================
IF OBJECT_ID(N'dbo.Comments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Comments (
        CommentId   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CampaignId  INT            NOT NULL,
        UserId      INT            NOT NULL,
        CommentText NVARCHAR(1000) NOT NULL,
        CreatedAt   DATETIME       NOT NULL DEFAULT GETDATE(),
        IsDeleted   BIT            NOT NULL DEFAULT 0,
        CONSTRAINT FK_Comments_Campaign FOREIGN KEY (CampaignId) REFERENCES dbo.Campaigns(CampaignId),
        CONSTRAINT FK_Comments_User     FOREIGN KEY (UserId)     REFERENCES dbo.Users(UserId)
    );
END
GO

-- ============================================================
-- 7. Таблица Favorites
-- ============================================================
IF OBJECT_ID(N'dbo.Favorites', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Favorites (
        FavoriteId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId     INT      NOT NULL,
        CampaignId INT      NOT NULL,
        AddedAt    DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Favorites_User     FOREIGN KEY (UserId)     REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_Favorites_Campaign FOREIGN KEY (CampaignId) REFERENCES dbo.Campaigns(CampaignId),
        CONSTRAINT UQ_Favorites_User_Campaign UNIQUE (UserId, CampaignId)
    );
END
GO

-- ============================================================
-- 8. Таблица CampaignUpdates
-- ============================================================
IF OBJECT_ID(N'dbo.CampaignUpdates', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CampaignUpdates (
        UpdateId   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CampaignId INT            NOT NULL,
        Title      NVARCHAR(200)  NOT NULL,
        Content    NVARCHAR(MAX)  NOT NULL,
        CreatedAt  DATETIME       NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_CampaignUpdates_Campaign FOREIGN KEY (CampaignId) REFERENCES dbo.Campaigns(CampaignId)
    );
END
GO

-- ============================================================
-- 9. Таблица Notifications
-- ============================================================
IF OBJECT_ID(N'dbo.Notifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notifications (
        NotificationId    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId            INT            NOT NULL,
        Title             NVARCHAR(200)  NOT NULL,
        Message           NVARCHAR(500)  NOT NULL,
        IsRead            BIT            NOT NULL DEFAULT 0,
        CreatedAt         DATETIME       NOT NULL DEFAULT GETDATE(),
        RelatedCampaignId INT            NULL,
        CONSTRAINT FK_Notifications_User     FOREIGN KEY (UserId)            REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_Notifications_Campaign FOREIGN KEY (RelatedCampaignId) REFERENCES dbo.Campaigns(CampaignId)
    );
END
GO

-- ============================================================
-- 10. Таблица Reports
-- ============================================================
IF OBJECT_ID(N'dbo.Reports', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reports (
        ReportId    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CampaignId  INT            NOT NULL,
        ReporterId  INT            NOT NULL,
        Reason      NVARCHAR(300)  NOT NULL,
        Description NVARCHAR(1000) NULL,
        Status      NVARCHAR(50)   NOT NULL DEFAULT N'Pending',
        CreatedAt   DATETIME       NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Reports_Campaign FOREIGN KEY (CampaignId) REFERENCES dbo.Campaigns(CampaignId),
        CONSTRAINT FK_Reports_User     FOREIGN KEY (ReporterId) REFERENCES dbo.Users(UserId)
    );
END
GO

-- ============================================================
-- 11. Таблица LoginHistory
-- ============================================================
IF OBJECT_ID(N'dbo.LoginHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LoginHistory (
        LoginId   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId    INT         NOT NULL,
        LoginDate DATETIME    NOT NULL DEFAULT GETDATE(),
        IpAddress NVARCHAR(50) NULL,
        Success   BIT         NOT NULL DEFAULT 1,
        CONSTRAINT FK_LoginHistory_User FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
    );
END
GO

-- ============================================================
-- Индексы
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Donations_UserId' AND object_id = OBJECT_ID(N'dbo.Donations'))
    CREATE NONCLUSTERED INDEX IX_Donations_UserId ON dbo.Donations(UserId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Donations_CampaignId' AND object_id = OBJECT_ID(N'dbo.Donations'))
    CREATE NONCLUSTERED INDEX IX_Donations_CampaignId ON dbo.Donations(CampaignId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Campaigns_CreatorId' AND object_id = OBJECT_ID(N'dbo.Campaigns'))
    CREATE NONCLUSTERED INDEX IX_Campaigns_CreatorId ON dbo.Campaigns(CreatorId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Campaigns_Status' AND object_id = OBJECT_ID(N'dbo.Campaigns'))
    CREATE NONCLUSTERED INDEX IX_Campaigns_Status ON dbo.Campaigns(Status);
GO

-- ============================================================
-- Триггер: автоматическое обновление CurrentAmount
-- ============================================================
CREATE OR ALTER TRIGGER trg_UpdateCampaignAmount
ON dbo.Donations
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Добавление доната
    UPDATE c
    SET CurrentAmount = CurrentAmount + i.Amount
    FROM dbo.Campaigns c
    INNER JOIN inserted i ON c.CampaignId = i.CampaignId;

    -- Удаление доната
    UPDATE c
    SET CurrentAmount = CurrentAmount - d.Amount
    FROM dbo.Campaigns c
    INNER JOIN deleted d ON c.CampaignId = d.CampaignId;

    -- Автозавершение кампании при достижении цели
    UPDATE c
    SET Status = N'Completed'
    FROM dbo.Campaigns c
    INNER JOIN inserted i ON c.CampaignId = i.CampaignId
    WHERE c.CurrentAmount >= c.GoalAmount AND c.Status = N'Active';
END;
GO

-- ============================================================
-- Представление vw_CampaignsFull
-- ============================================================
CREATE OR ALTER VIEW dbo.vw_CampaignsFull
AS
SELECT
    c.CampaignId,
    c.Title,
    c.ShortDescription,
    c.Description,
    c.GoalAmount,
    c.CurrentAmount,
    CAST(ROUND(ISNULL(c.CurrentAmount, 0) * 100.0 / NULLIF(c.GoalAmount, 0), 1) AS DECIMAL(5,1)) AS ProgressPercent,
    c.Status,
    c.ImagePath,
    c.CreatedAt,
    c.EndDate,
    cat.CategoryName,
    u.UserId   AS CreatorId,
    u.FullName AS CreatorName,
    u.Username AS CreatorUsername,
    (SELECT COUNT(*) FROM dbo.Donations d WHERE d.CampaignId = c.CampaignId) AS DonationsCount
FROM dbo.Campaigns c
INNER JOIN dbo.Users u ON c.CreatorId = u.UserId
LEFT  JOIN dbo.Categories cat ON c.CategoryId = cat.CategoryId;
GO

-- ============================================================
-- Тестовые данные
-- ============================================================

-- Очистка (осторожно — удаляет все данные)
DELETE FROM dbo.Donations;
DELETE FROM dbo.Comments;
DELETE FROM dbo.Favorites;
DELETE FROM dbo.CampaignUpdates;
DELETE FROM dbo.Notifications;
DELETE FROM dbo.Reports;
DELETE FROM dbo.LoginHistory;
DELETE FROM dbo.Campaigns;
DELETE FROM dbo.Users;
GO

-- Пользователи
-- Пароли в открытом виде для тестовых данных (в приложении хэшируются)
SET IDENTITY_INSERT dbo.Users ON;
INSERT INTO dbo.Users (UserId, Username, PasswordHash, Email, FullName, Role, CreatedAt, IsActive, Phone)
VALUES
(1, N'admin',   N'admin123', N'admin@donate.local',  N'Администратор Системы', N'Admin', GETDATE(), 1, N'+375291111111'),
(2, N'ivanov',  N'12345',    N'ivanov@mail.ru',      N'Иванов Алексей',        N'User',  GETDATE(), 1, N'+375292222222'),
(3, N'petrova', N'12345',    N'petrova@mail.ru',     N'Петрова Мария',         N'User',  GETDATE(), 1, N'+375293333333'),
(4, N'sidorov', N'12345',    N'sidorov@mail.ru',     N'Сидоров Дмитрий',       N'User',  GETDATE(), 1, N'+375294444444');
SET IDENTITY_INSERT dbo.Users OFF;
GO

-- Кампании
SET IDENTITY_INSERT dbo.Campaigns ON;
INSERT INTO dbo.Campaigns (CampaignId, Title, Description, ShortDescription, GoalAmount, CurrentAmount, CreatorId, Status, CreatedAt, EndDate, CategoryId)
VALUES
(1, N'Помощь на операцию ребёнку',
    N'Маленькому Мише нужна срочная операция на сердце. Семья не может собрать нужную сумму самостоятельно.',
    N'Сбор средств на операцию сердца для ребёнка',
    15000, 0, 2, N'Active', GETDATE(), DATEADD(MONTH, 2, GETDATE()), 1),

(2, N'Корм для приюта животных',
    N'Приют «Добрые лапы» нуждается в корме и лекарствах для 80 собак и кошек.',
    N'Помощь приюту животных',
    5000, 0, 3, N'Active', GETDATE(), DATEADD(MONTH, 1, GETDATE()), 2),

(3, N'Стипендия для студента',
    N'Талантливый студент из многодетной семьи нуждается в поддержке для продолжения учёбы.',
    N'Образовательная поддержка',
    3000, 0, 4, N'Active', GETDATE(), DATEADD(MONTH, 3, GETDATE()), 3),

(4, N'Посадка деревьев',
    N'Экологическая акция по высадке 500 деревьев в городском парке.',
    N'Экология и природа',
    2500, 0, 2, N'Active', DATEADD(MONTH, -1, GETDATE()), GETDATE(), 4);
SET IDENTITY_INSERT dbo.Campaigns OFF;
GO

-- Пожертвования (триггер автоматически обновит CurrentAmount)
INSERT INTO dbo.Donations (CampaignId, UserId, Amount, Message, DonationDate)
VALUES
(1, 3, 500,  N'Выздоравливай, малыш!', GETDATE()),
(1, 4, 1000, N'Держитесь!',            DATEADD(DAY, -2, GETDATE())),
(1, 3, 2850, N'Надеюсь, этого хватит', DATEADD(DAY, -5, GETDATE())),
(2, 2, 700,  N'Любим животных',        GETDATE()),
(2, 4, 1400, NULL,                     DATEADD(DAY, -1, GETDATE())),
(3, 2, 300,  N'Учёба важна',           GETDATE()),
(3, 3, 500,  N'Удачи!',                DATEADD(DAY, -3, GETDATE())),
(4, 3, 2500, N'За зелёную планету!',   DATEADD(DAY, -10, GETDATE()));
GO

-- Пересчёт CurrentAmount на всякий случай
UPDATE c
SET c.CurrentAmount = ISNULL((
    SELECT SUM(d.Amount) FROM dbo.Donations d WHERE d.CampaignId = c.CampaignId
), 0)
FROM dbo.Campaigns c;
GO

-- Завершаем кампанию, если цель достигнута
UPDATE dbo.Campaigns
SET Status = N'Completed'
WHERE CurrentAmount >= GoalAmount AND Status = N'Active';
GO

PRINT N'================================================';
PRINT N'База данных up1 успешно создана и наполнена!';
PRINT N'';
PRINT N'Тестовые учётные записи:';
PRINT N'  admin   / admin123  (Администратор)';
PRINT N'  ivanov  / 12345     (Пользователь)';
PRINT N'  petrova / 12345     (Пользователь)';
PRINT N'  sidorov / 12345     (Пользователь)';
PRINT N'================================================';
GO
