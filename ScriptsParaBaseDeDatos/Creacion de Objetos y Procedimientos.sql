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
    IsDeleted BIT NOT NULL
        CONSTRAINT DF_Tasks_IsDeleted DEFAULT(0),
    DeletedDate DATETIME NULL,

    CONSTRAINT PK_Tasks
        PRIMARY KEY CLUSTERED (TaskId),
    CONSTRAINT FK_Tasks_Users
        FOREIGN KEY (UserId)
        REFERENCES dbo.Users(UserId),
    CONSTRAINT CK_Tasks_Priority
        CHECK ([Priority] IN (1,2,3)),
    CONSTRAINT CK_Tasks_Status
        CHECK ([Status] IN (1,2,3)),
    CONSTRAINT CK_Tasks_Title
        CHECK (LEN(LTRIM(RTRIM(Title))) > 0),
    CONSTRAINT CK_Tasks_Description
        CHECK ([Description] IS NULL OR LEN([Description]) <= 500)
);
GO

-- Indice unique para evitar asignar duplicidad en las tareas por usuario
CREATE UNIQUE INDEX UX_Tasks_User_Title
ON dbo.Tasks(UserId, Title)
WHERE IsDeleted = 0;
GO

-- Indices para optimizacion de busquedas
CREATE INDEX IX_Tasks_IsDeleted ON dbo.Tasks(IsDeleted);
GO

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
    LEFT JOIN dbo.Tasks T (nolock) ON U.UserId = T.UserId AND T.IsDeleted = 0
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

-- Registros de tareas para pruebas
INSERT INTO dbo.Tasks
(Title,[Description],[Priority],CreatedDate,StartDate,EndDate,DueDate,[Status],UserId)
VALUES
-- Pendientes vencidas
('Tarea prueba 01', 'Tarea de pruebas 1', 1, '2026-07-01T09:00:00', NULL, NULL, '2026-07-10T18:00:00', 1, 1),
('Tarea prueba 02', 'Prueba rápida A', 2, '2026-07-02T10:30:00', NULL, NULL, '2026-07-12T18:00:00', 1, 2),
('Tarea prueba 03', 'Nueva prueba X', 3, '2026-07-03T08:15:00', NULL, NULL, '2026-07-14T18:00:00', 1, 3),
('Tarea prueba 04', 'Pendiente por probar', 1, '2026-07-04T11:00:00', NULL, NULL, '2026-07-16T18:00:00', 1, 1),
('Tarea prueba 05', 'Tarea A', 2, '2026-07-05T12:20:00', NULL, NULL, '2026-07-18T18:00:00', 1, 2),
('Tarea prueba 06', 'Otra tarea rápida', 3, '2026-07-06T09:45:00', NULL, NULL, '2026-07-19T18:00:00', 1, 3),
('Tarea prueba 07', 'Prueba improvisada', 1, '2026-07-07T14:10:00', NULL, NULL, '2026-07-20T18:00:00', 1, 1),
('Tarea prueba 08', 'Pendiente B', 2, '2026-07-08T15:30:00', NULL, NULL, '2026-07-21T18:00:00', 1, 2),
('Tarea prueba 09', 'Nueva prueba 9', 3, '2026-07-09T16:00:00', NULL, NULL, '2026-07-22T18:00:00', 1, 3),
('Tarea prueba 10', 'Tarea rápida X', 2, '2026-07-10T10:00:00', NULL, NULL, '2026-07-24T18:00:00', 1, 1),

-- Pendientes vigentes
('Tarea prueba 11', 'Pendiente futura A', 1, '2026-07-15T09:00:00', NULL, NULL, '2026-07-28T18:00:00', 1, 2),
('Tarea prueba 12', 'Nueva tarea B', 2, '2026-07-16T10:00:00', NULL, NULL, '2026-07-30T18:00:00', 1, 3),
('Tarea prueba 13', 'Prueba futura 1', 3, '2026-07-17T11:00:00', NULL, NULL, '2026-08-01T18:00:00', 1, 1),
('Tarea prueba 14', 'Tarea pendiente C', 1, '2026-07-18T12:00:00', NULL, NULL, '2026-08-03T18:00:00', 1, 2),
('Tarea prueba 15', 'Registro de prueba', 2, '2026-07-19T13:00:00', NULL, NULL, '2026-08-05T18:00:00', 1, 3),
('Tarea prueba 16', 'Prueba pendiente D', 3, '2026-07-20T14:00:00', NULL, NULL, '2026-08-07T18:00:00', 1, 1),
('Tarea prueba 17', 'Nueva prueba futura', 2, '2026-07-21T15:00:00', NULL, NULL, '2026-08-10T18:00:00', 1, 2),

-- En progreso vencidas
('Tarea prueba 18', 'En proceso A', 1, '2026-07-01T08:00:00', '2026-07-02T09:00:00', NULL, '2026-07-11T18:00:00', 2, 3),
('Tarea prueba 19', 'Prueba trabajando', 2, '2026-07-02T09:00:00', '2026-07-03T10:00:00', NULL, '2026-07-13T18:00:00', 2, 1),
('Tarea prueba 20', 'Nueva prueba activa', 3, '2026-07-03T10:00:00', '2026-07-04T11:00:00', NULL, '2026-07-15T18:00:00', 2, 2),
('Tarea prueba 21', 'Tarea en curso', 1, '2026-07-04T11:00:00', '2026-07-05T12:00:00', NULL, '2026-07-17T18:00:00', 2, 3),
('Tarea prueba 22', 'Prueba en proceso X', 2, '2026-07-05T12:00:00', '2026-07-06T13:00:00', NULL, '2026-07-19T18:00:00', 2, 1),
('Tarea prueba 23', 'Registro activo A', 3, '2026-07-06T13:00:00', '2026-07-07T14:00:00', NULL, '2026-07-21T18:00:00', 2, 2),
('Tarea prueba 24', 'Tarea empezada', 1, '2026-07-07T14:00:00', '2026-07-08T15:00:00', NULL, '2026-07-23T18:00:00', 2, 3),
('Tarea prueba 25', 'Prueba aún abierta', 2, '2026-07-08T15:00:00', '2026-07-09T16:00:00', NULL, '2026-07-25T18:00:00', 2, 1),

-- En progreso vigentes
('Tarea prueba 26', 'En proceso futuro A', 3, '2026-07-15T09:00:00', '2026-07-16T10:00:00', NULL, '2026-07-29T18:00:00', 2, 2),
('Tarea prueba 27', 'Tarea activa B', 1, '2026-07-16T10:00:00', '2026-07-17T11:00:00', NULL, '2026-07-31T18:00:00', 2, 3),
('Tarea prueba 28', 'Nueva prueba activa C', 2, '2026-07-17T11:00:00', '2026-07-18T12:00:00', NULL, '2026-08-02T18:00:00', 2, 1),
('Tarea prueba 29', 'Prueba en curso D', 3, '2026-07-18T12:00:00', '2026-07-19T13:00:00', NULL, '2026-08-04T18:00:00', 2, 2),
('Tarea prueba 30', 'Tarea trabajando X', 1, '2026-07-19T13:00:00', '2026-07-20T14:00:00', NULL, '2026-08-06T18:00:00', 2, 3),
('Tarea prueba 31', 'Registro todavía abierto', 2, '2026-07-20T14:00:00', '2026-07-21T15:00:00', NULL, '2026-08-08T18:00:00', 2, 1),
('Tarea prueba 32', 'Nueva tarea activa', 3, '2026-07-21T15:00:00', '2026-07-22T16:00:00', NULL, '2026-08-11T18:00:00', 2, 2),
('Tarea prueba 33', 'Prueba todavía en curso', 2, '2026-07-22T16:00:00', '2026-07-23T17:00:00', NULL, '2026-08-13T18:00:00', 2, 3),

-- Completadas
('Tarea prueba 34', 'Tarea terminada A', 1, '2026-06-25T09:00:00', '2026-06-26T10:00:00', '2026-06-28T17:00:00', '2026-06-30T18:00:00', 3, 1),
('Tarea prueba 35', 'Prueba completada B', 2, '2026-06-27T10:00:00', '2026-06-28T11:00:00', '2026-07-01T16:00:00', '2026-07-02T18:00:00', 3, 2),
('Tarea prueba 36', 'Nueva prueba terminada', 3, '2026-06-29T11:00:00', '2026-06-30T12:00:00', '2026-07-03T15:00:00', '2026-07-04T18:00:00', 3, 3),
('Tarea prueba 37', 'Tarea lista', 1, '2026-07-01T12:00:00', '2026-07-02T13:00:00', '2026-07-05T14:00:00', '2026-07-06T18:00:00', 3, 1),
('Tarea prueba 38', 'Prueba finalizada X', 2, '2026-07-03T13:00:00', '2026-07-04T14:00:00', '2026-07-07T13:00:00', '2026-07-08T18:00:00', 3, 2),
('Tarea prueba 39', 'Registro ya terminado', 3, '2026-07-05T14:00:00', '2026-07-06T15:00:00', '2026-07-09T12:00:00', '2026-07-10T18:00:00', 3, 3),
('Tarea prueba 40', 'Tarea completada C', 1, '2026-07-07T15:00:00', '2026-07-08T16:00:00', '2026-07-11T11:00:00', '2026-07-12T18:00:00', 3, 1),
('Tarea prueba 41', 'Nueva tarea lista', 2, '2026-07-09T16:00:00', '2026-07-10T17:00:00', '2026-07-13T10:00:00', '2026-07-14T18:00:00', 3, 2),
('Tarea prueba 42', 'Prueba completada D', 3, '2026-07-11T09:00:00', '2026-07-12T10:00:00', '2026-07-15T17:00:00', '2026-07-16T18:00:00', 3, 3),
('Tarea prueba 43', 'Tarea final A', 1, '2026-07-13T10:00:00', '2026-07-14T11:00:00', '2026-07-17T16:00:00', '2026-07-18T18:00:00', 3, 1),
('Tarea prueba 44', 'Registro completado', 2, '2026-07-15T11:00:00', '2026-07-16T12:00:00', '2026-07-19T15:00:00', '2026-07-20T18:00:00', 3, 2),
('Tarea prueba 45', 'Prueba lista X', 3, '2026-07-17T12:00:00', '2026-07-18T13:00:00', '2026-07-21T14:00:00', '2026-07-22T18:00:00', 3, 3),
('Tarea prueba 46', 'Tarea ya hecha', 1, '2026-07-18T13:00:00', '2026-07-19T14:00:00', '2026-07-22T13:00:00', '2026-07-23T18:00:00', 3, 1),
('Tarea prueba 47', 'Nueva prueba final', 2, '2026-07-19T14:00:00', '2026-07-20T15:00:00', '2026-07-23T12:00:00', '2026-07-24T18:00:00', 3, 2),
('Tarea prueba 48', 'Prueba completada rápido', 3, '2026-07-20T15:00:00', '2026-07-21T16:00:00', '2026-07-24T11:00:00', '2026-07-25T18:00:00', 3, 3),
('Tarea prueba 49', 'Tarea terminada X', 1, '2026-07-21T09:00:00', '2026-07-22T10:00:00', '2026-07-24T16:00:00', '2026-07-26T18:00:00', 3, 1),
('Tarea prueba 50', 'Última prueba rápida', 2, '2026-07-22T10:00:00', '2026-07-23T11:00:00', '2026-07-25T15:00:00', '2026-07-28T18:00:00', 3, 3);
GO
