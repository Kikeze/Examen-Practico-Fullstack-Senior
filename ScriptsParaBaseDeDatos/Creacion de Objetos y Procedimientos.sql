-- Verificamos y recreamos la base de datos
IF DB_ID('TaskManagerTest') IS NOT NULL
BEGIN
    ALTER DATABASE TaskManagerTest SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE TaskManagerTest;
END
GO

CREATE DATABASE TaskManagerTest;
GO

USE TaskManagerTest;
GO

-- Tabla de usuarios
CREATE TABLE dbo.Users
(
    UserId INT IDENTITY(1,1) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    IsActive BIT NOT NULL
        CONSTRAINT DF_Users_IsActive DEFAULT(1),
    CreatedDate DATETIME NOT NULL
        CONSTRAINT DF_Users_CreatedDate DEFAULT(GETDATE()),

    CONSTRAINT PK_Users
        PRIMARY KEY CLUSTERED (UserId),
);
GO

-- Tabla de tareas
CREATE TABLE dbo.Tasks
(
    TaskId INT IDENTITY(1,1) NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [Priority] INT NOT NULL,
    CreatedDate DATETIME NOT NULL
        CONSTRAINT DF_Tasks_CreatedDate DEFAULT(GETDATE()),
    StartDate DATETIME NULL,
    EndDate DATETIME NULL,
    DueDate DATETIME NOT NULL,
    [Status] INT NOT NULL
        CONSTRAINT DF_Tasks_Status DEFAULT(1),
    UserId INT NOT NULL,

    CONSTRAINT PK_Tasks
        PRIMARY KEY CLUSTERED (TaskId),
    CONSTRAINT FK_Tasks_Users
        FOREIGN KEY (UserId)
        REFERENCES dbo.Users(UserId),
    CONSTRAINT CK_Tasks_Priority
        CHECK (Priority IN (1,2,3)),
    CONSTRAINT CK_Tasks_Status
        CHECK (Status IN (1,2,3)),
    CONSTRAINT CK_Tasks_Title
        CHECK (LEN(LTRIM(RTRIM(Title))) > 0),
    CONSTRAINT CK_Tasks_Description
        CHECK ([Description] IS NULL OR LEN(Description) <= 500)
);
GO

-- Indice unique para evitar asignar duplicidad en las tareas por usuario
CREATE UNIQUE INDEX UX_Tasks_User_Title
ON dbo.Tasks(UserId, Title);
GO

-- Indices para optimizacion de busquedas
CREATE INDEX IX_Tasks_UserId ON dbo.Tasks(UserId);
GO

CREATE INDEX IX_Tasks_Status ON dbo.Tasks([Status]);
GO

CREATE INDEX IX_Tasks_Priority ON dbo.Tasks([Priority]);
GO

CREATE INDEX IX_Tasks_DueDate ON dbo.Tasks(DueDate);
GO

CREATE INDEX IX_Tasks_User_Status ON dbo.Tasks(UserId, [Status]);
GO

CREATE INDEX IX_Tasks_User_Priority ON dbo.Tasks(UserId, [Priority]);
GO

-- Registros de usuarios para pruebas
INSERT INTO dbo.Users (FullName, Email)
VALUES
    ('Juan Perez', 'juan.perez@taskmanagertest.com'),
    ('Maria Lopez', 'maria.lopez@taskmanagertest.com'),
    ('Carlos Hernandez', 'carlos.hernandez@taskmanagertest.com')
GO

-- Tabla de Seguimiento para tareas
CREATE TABLE dbo.TaskAudit
(
    AuditId INT IDENTITY(1,1) NOT NULL,
    TaskId INT NOT NULL,
    OldStatus INT NOT NULL,
    NewStatus INT NOT NULL,
    ChangeDate DATETIME NOT NULL
        CONSTRAINT DF_TaskAudit_ChangeDate DEFAULT(GETDATE()),

    CONSTRAINT PK_TaskAudit
        PRIMARY KEY (AuditId),

    CONSTRAINT FK_TaskAudit_Task
        FOREIGN KEY (TaskId)
        REFERENCES dbo.Tasks(TaskId)
);
GO

-- Reporte de tareas por usuario
CREATE OR ALTER PROCEDURE dbo.sp_GetPendingTasks
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        U.UserId,
        U.FullName AS [Usuario],
        SUM(CASE
                WHEN T.[Status] = 1
                THEN 1
                ELSE 0
            END) AS TotalPendientes,
        SUM(CASE
                WHEN T.[Status] <> 3
                 AND T.DueDate < GETDATE()
                THEN 1
                ELSE 0
            END) AS TotalVencidas
    FROM dbo.Users U (nolock)
    LEFT JOIN dbo.Tasks T (nolock) ON U.UserId = T.UserId
    GROUP BY U.UserId, U.FullName
    ORDER BY U.FullName;
END
GO

-- Trigger para registrar modificaciones a la tabla de tareas
CREATE OR ALTER TRIGGER TR_Task_StatusAudit
ON dbo.Tasks
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.TaskAudit (TaskId, OldStatus, NewStatus)
    SELECT
        D.TaskId,
        D.[Status],
        I.[Status]
    FROM deleted D
    INNER JOIN inserted I ON D.TaskId = I.TaskId
    WHERE D.[Status] <> I.[Status];
END
GO
