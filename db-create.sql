-- ============================================================
-- Quizora - SQL Server Database Generation Script
-- Generato dalle configurazioni EF Core (5 moduli, 5 schemi)
-- Eseguire su un'istanza SQL Server pulita
-- ============================================================

USE master;
GO

-- Cambia il nome del database se necessario
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'QuizoraDb')
    CREATE DATABASE QuizoraDb;
GO

USE QuizoraDb;
GO

-- ============================================================
-- SCHEMI
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'identity')
    EXEC('CREATE SCHEMA [identity]');
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'quiz')
    EXEC('CREATE SCHEMA [quiz]');
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'exam')
    EXEC('CREATE SCHEMA [exam]');
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'analytics')
    EXEC('CREATE SCHEMA [analytics]');
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ai')
    EXEC('CREATE SCHEMA [ai]');
GO

-- ============================================================
-- MODULO: IDENTITY
-- ============================================================

CREATE TABLE [identity].[Users] (
    [Id]               UNIQUEIDENTIFIER NOT NULL,
    [Email]            NVARCHAR(256)    NOT NULL,
    [UserName]         NVARCHAR(50)     NOT NULL,
    [PasswordHash]     NVARCHAR(MAX)    NOT NULL,
    [IsEmailConfirmed] BIT              NOT NULL DEFAULT 0,
    [CreatedAt]        DATETIME2        NOT NULL,
    [LastLoginAt]      DATETIME2        NULL,
    [IsAdmin]          BIT              NOT NULL DEFAULT 0,
    [IsCreator]        BIT              NOT NULL DEFAULT 0,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_Users_Email]
    ON [identity].[Users] ([Email]);
GO

CREATE TABLE [identity].[RefreshTokens] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [UserId]      UNIQUEIDENTIFIER NOT NULL,
    [Token]       NVARCHAR(512)    NOT NULL,
    [ExpiresAt]   DATETIME2        NOT NULL,
    [CreatedAt]   DATETIME2        NOT NULL,
    [CreatedByIp] NVARCHAR(45)     NULL,
    [RevokedAt]   DATETIME2        NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefreshTokens_Users]
        FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id])
        ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX [IX_RefreshTokens_Token]
    ON [identity].[RefreshTokens] ([Token]);
CREATE INDEX [IX_RefreshTokens_UserId]
    ON [identity].[RefreshTokens] ([UserId]);
GO

-- ============================================================
-- MODULO: QUIZ MANAGEMENT
-- ============================================================

CREATE TABLE [quiz].[Categories] (
    [Id]         UNIQUEIDENTIFIER NOT NULL,
    [Name]       NVARCHAR(100)    NOT NULL,
    [Slug]       NVARCHAR(100)    NOT NULL,
    [ParentId]   UNIQUEIDENTIFIER NULL,
    [OrderIndex] INT              NOT NULL DEFAULT 0,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_Categories_Slug]
    ON [quiz].[Categories] ([Slug]);
GO

CREATE TABLE [quiz].[Tags] (
    [Id]   UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR(100)    NOT NULL,
    [Slug] NVARCHAR(100)    NOT NULL,
    CONSTRAINT [PK_Tags] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_Tags_Slug]
    ON [quiz].[Tags] ([Slug]);
GO

CREATE TABLE [quiz].[Questions] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [Text]        NVARCHAR(2000)   NOT NULL,
    [Explanation] NVARCHAR(2000)   NULL,
    [Difficulty]  NVARCHAR(20)     NOT NULL,
    [CreatorId]   UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_Questions] PRIMARY KEY ([Id])
);
GO

CREATE INDEX [IX_Questions_CreatorId]
    ON [quiz].[Questions] ([CreatorId]);
GO

CREATE TABLE [quiz].[QuestionOptions] (
    [Id]         UNIQUEIDENTIFIER NOT NULL,
    [Text]       NVARCHAR(500)    NOT NULL,
    [IsCorrect]  BIT              NOT NULL DEFAULT 0,
    [OrderIndex] INT              NOT NULL DEFAULT 0,
    [QuestionId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_QuestionOptions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_QuestionOptions_Questions]
        FOREIGN KEY ([QuestionId]) REFERENCES [quiz].[Questions] ([Id])
        ON DELETE CASCADE
);
GO

CREATE INDEX [IX_QuestionOptions_QuestionId]
    ON [quiz].[QuestionOptions] ([QuestionId]);
GO

-- ExamConfig e' un owned entity: le sue colonne sono inline in Quizzes
CREATE TABLE [quiz].[Quizzes] (
    [Id]               UNIQUEIDENTIFIER NOT NULL,
    [Title]            NVARCHAR(200)    NOT NULL,
    [Slug]             NVARCHAR(200)    NOT NULL,
    [Description]      NVARCHAR(MAX)    NULL,
    [Status]           NVARCHAR(20)     NOT NULL,
    [CategoryId]       UNIQUEIDENTIFIER NOT NULL,
    [CreatorId]        UNIQUEIDENTIFIER NOT NULL,
    -- ExamConfig (owned entity, colonne inline)
    [TimeLimitSeconds] INT              NOT NULL,
    [PointsCorrect]    FLOAT            NOT NULL,
    [PointsWrong]      FLOAT            NOT NULL,
    [PointsSkipped]    FLOAT            NOT NULL,
    [PassingScore]     FLOAT            NULL,
    [ShuffleQuestions] BIT              NOT NULL,
    [ShuffleOptions]   BIT              NOT NULL,
    [CreatedAt]        DATETIME2        NOT NULL,
    [UpdatedAt]        DATETIME2        NOT NULL,
    [IsDeleted]        BIT              NOT NULL DEFAULT 0,
    CONSTRAINT [PK_Quizzes] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_Quizzes_Slug]
    ON [quiz].[Quizzes] ([Slug]);
CREATE INDEX [IX_Quizzes_CategoryId]
    ON [quiz].[Quizzes] ([CategoryId]);
CREATE INDEX [IX_Quizzes_CreatorId]
    ON [quiz].[Quizzes] ([CreatorId]);
CREATE INDEX [IX_Quizzes_Status_IsDeleted]
    ON [quiz].[Quizzes] ([Status], [IsDeleted]);
GO

-- Tabella di join owned: QuizQuestions e' un OwnsMany da Quiz
CREATE TABLE [quiz].[QuizQuestions] (
    [QuizId]      UNIQUEIDENTIFIER NOT NULL,
    [QuestionId]  UNIQUEIDENTIFIER NOT NULL,
    [OrderIndex]  INT              NOT NULL,
    CONSTRAINT [PK_QuizQuestions] PRIMARY KEY ([QuizId], [QuestionId]),
    CONSTRAINT [FK_QuizQuestions_Quizzes]
        FOREIGN KEY ([QuizId]) REFERENCES [quiz].[Quizzes] ([Id])
        ON DELETE CASCADE
);
GO

-- ============================================================
-- MODULO: EXAM ENGINE
-- ============================================================

-- ConfigSnapshot e' un owned entity: le sue colonne sono inline in ExamSessions
CREATE TABLE [exam].[ExamSessions] (
    [Id]               UNIQUEIDENTIFIER NOT NULL,
    [QuizId]           UNIQUEIDENTIFIER NOT NULL,
    [UserId]           UNIQUEIDENTIFIER NOT NULL,
    [Type]             NVARCHAR(20)     NOT NULL,
    [Status]           NVARCHAR(20)     NOT NULL,
    [StartedAt]        DATETIME2        NOT NULL,
    [CompletedAt]      DATETIME2        NULL,
    [ExpiresAt]        DATETIME2        NULL,
    [Score]            FLOAT            NULL,
    [NormalizedScore]  FLOAT            NULL,
    [TotalQuestions]   INT              NOT NULL,
    [CorrectCount]     INT              NOT NULL DEFAULT 0,
    [WrongCount]       INT              NOT NULL DEFAULT 0,
    [SkippedCount]     INT              NOT NULL DEFAULT 0,
    -- ConfigSnapshot (owned entity, colonne inline)
    [TimeLimitSeconds] INT              NOT NULL,
    [PointsCorrect]    FLOAT            NOT NULL,
    [PointsWrong]      FLOAT            NOT NULL,
    [PointsSkipped]    FLOAT            NOT NULL,
    [PassingScore]     FLOAT            NULL,
    [ShuffleQuestions] BIT              NOT NULL,
    [ShuffleOptions]   BIT              NOT NULL,
    [CategoryId]       UNIQUEIDENTIFIER NULL,
    [QuizTitle]        NVARCHAR(200)    NULL,
    CONSTRAINT [PK_ExamSessions] PRIMARY KEY ([Id])
);
GO

CREATE INDEX [IX_ExamSessions_UserId]
    ON [exam].[ExamSessions] ([UserId]);
CREATE INDEX [IX_ExamSessions_QuizId]
    ON [exam].[ExamSessions] ([QuizId]);
CREATE INDEX [IX_ExamSessions_UserId_Status]
    ON [exam].[ExamSessions] ([UserId], [Status]);
CREATE INDEX [IX_ExamSessions_Status_ExpiresAt]
    ON [exam].[ExamSessions] ([Status], [ExpiresAt])
    WHERE [ExpiresAt] IS NOT NULL;
GO

CREATE TABLE [exam].[SessionAnswers] (
    [Id]               UNIQUEIDENTIFIER NOT NULL,
    [SessionId]        UNIQUEIDENTIFIER NOT NULL,
    [QuestionId]       UNIQUEIDENTIFIER NOT NULL,
    [SelectedOptionId] UNIQUEIDENTIFIER NULL,
    [IsCorrect]        BIT              NOT NULL DEFAULT 0,
    [PointsAwarded]    FLOAT            NOT NULL,
    [AnsweredAt]       DATETIME2        NOT NULL,
    [TimeSpentSeconds] INT              NOT NULL,
    CONSTRAINT [PK_SessionAnswers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SessionAnswers_ExamSessions]
        FOREIGN KEY ([SessionId]) REFERENCES [exam].[ExamSessions] ([Id])
        ON DELETE CASCADE
);
GO

CREATE INDEX [IX_SessionAnswers_SessionId]
    ON [exam].[SessionAnswers] ([SessionId]);
GO

-- ============================================================
-- MODULO: ANALYTICS
-- ============================================================

CREATE TABLE [analytics].[UserStats] (
    [Id]                    UNIQUEIDENTIFIER NOT NULL,
    [UserId]                UNIQUEIDENTIFIER NOT NULL,
    [TotalExams]            INT              NOT NULL,
    [TotalCorrect]          INT              NOT NULL,
    [TotalAnswered]         INT              NOT NULL,
    [AverageScore]          FLOAT            NOT NULL,
    [BestScore]             FLOAT            NOT NULL,
    [TotalTimeSpentSeconds] INT              NOT NULL,
    [UpdatedAt]             DATETIME2        NOT NULL,
    CONSTRAINT [PK_UserStats] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_UserStats_UserId]
    ON [analytics].[UserStats] ([UserId]);
GO

CREATE TABLE [analytics].[CategoryStats] (
    [Id]            UNIQUEIDENTIFIER NOT NULL,
    [UserId]        UNIQUEIDENTIFIER NOT NULL,
    [CategoryId]    UNIQUEIDENTIFIER NOT NULL,
    [TotalExams]    INT              NOT NULL,
    [AverageScore]  FLOAT            NOT NULL,
    [WeakAreaScore] FLOAT            NOT NULL,
    [UpdatedAt]     DATETIME2        NOT NULL,
    CONSTRAINT [PK_CategoryStats] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_CategoryStats_UserId_CategoryId]
    ON [analytics].[CategoryStats] ([UserId], [CategoryId]);
GO

CREATE TABLE [analytics].[DailyActivity] (
    [Id]               UNIQUEIDENTIFIER NOT NULL,
    [UserId]           UNIQUEIDENTIFIER NOT NULL,
    [Date]             DATE             NOT NULL,
    [ExamsCount]       INT              NOT NULL,
    [CorrectAnswers]   INT              NOT NULL,
    [TimeSpentSeconds] INT              NOT NULL,
    CONSTRAINT [PK_DailyActivity] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_DailyActivity_UserId_Date]
    ON [analytics].[DailyActivity] ([UserId], [Date]);
GO

-- ============================================================
-- MODULO: AI TUTOR
-- ============================================================

CREATE TABLE [ai].[ChatSessions] (
    [Id]            UNIQUEIDENTIFIER NOT NULL,
    [UserId]        UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt]     DATETIME2        NOT NULL,
    [LastMessageAt] DATETIME2        NOT NULL,
    CONSTRAINT [PK_ChatSessions] PRIMARY KEY ([Id])
);
GO

CREATE INDEX [IX_ChatSessions_UserId]
    ON [ai].[ChatSessions] ([UserId]);
GO

CREATE TABLE [ai].[ChatMessages] (
    [Id]        UNIQUEIDENTIFIER NOT NULL,
    [SessionId] UNIQUEIDENTIFIER NOT NULL,
    [Role]      NVARCHAR(20)     NOT NULL,
    [Content]   NVARCHAR(MAX)    NOT NULL,
    [CreatedAt] DATETIME2        NOT NULL,
    CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChatMessages_ChatSessions]
        FOREIGN KEY ([SessionId]) REFERENCES [ai].[ChatSessions] ([Id])
        ON DELETE CASCADE
);
GO

CREATE INDEX [IX_ChatMessages_SessionId]
    ON [ai].[ChatMessages] ([SessionId]);
GO

CREATE TABLE [ai].[StudyPlans] (
    [Id]                   UNIQUEIDENTIFIER NOT NULL,
    [UserId]               UNIQUEIDENTIFIER NOT NULL,
    [ContentJson]          NVARCHAR(MAX)    NOT NULL,
    [GeneratedAt]          DATETIME2        NOT NULL,
    [UpdatedAutomatically] BIT              NOT NULL,
    CONSTRAINT [PK_StudyPlans] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_StudyPlans_UserId]
    ON [ai].[StudyPlans] ([UserId]);
GO

-- ============================================================
-- FINE SCRIPT
-- ============================================================
