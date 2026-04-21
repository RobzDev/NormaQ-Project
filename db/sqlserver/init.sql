-- =============================================================================
-- QualityDoc-Polyglot | SQL Server DDL
-- Sistema Integral de Gestión Documental para Normativas de Calidad
-- Normalizado a 3NF
-- =============================================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'QualityDocDB')
    CREATE DATABASE QualityDocDB;
GO

USE QualityDocDB;
GO

-- =============================================================================
-- PILAR A — Estructura organizacional y seguridad
-- =============================================================================

CREATE TABLE Companias (
    id          INT             IDENTITY(1,1)   PRIMARY KEY,
    nombre      NVARCHAR(150)   NOT NULL,
    rfc         VARCHAR(20)     NULL,
    direccion   NVARCHAR(255)   NULL,
    activo      BIT             NOT NULL DEFAULT 1,
    creado_en   DATETIME        NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE Departamentos (
    id              INT             IDENTITY(1,1)   PRIMARY KEY,
    nombre          NVARCHAR(100)   NOT NULL,
    compania_id     INT             NOT NULL,
    activo          BIT             NOT NULL DEFAULT 1,
    CONSTRAINT FK_Deptos_Companias FOREIGN KEY (compania_id)
        REFERENCES Companias(id)
);
GO

CREATE TABLE Roles (
    id              INT             IDENTITY(1,1)   PRIMARY KEY,
    nombre          NVARCHAR(80)    NOT NULL,
    descripcion     NVARCHAR(255)   NULL
);
GO



CREATE TABLE Usuarios (
    id                  INT             IDENTITY(1,1)   PRIMARY KEY,
    nombre              NVARCHAR(150)   NOT NULL,
    email               VARCHAR(150)    NOT NULL,
    password_hash       VARCHAR(255)    NOT NULL,
    departamento_id     INT             NOT NULL,
    activo              BIT             NOT NULL DEFAULT 1,
    creado_en           DATETIME        NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_Usuarios_Email    UNIQUE (email),
    CONSTRAINT FK_Usuarios_Deptos   FOREIGN KEY (departamento_id)
        REFERENCES Departamentos(id)
);
GO

-- Asignación flexible: un usuario puede tener distintos roles según el contexto de departamento
CREATE TABLE Usuarios_Roles (
    id                  INT     IDENTITY(1,1)   PRIMARY KEY,
    usuario_id          INT     NOT NULL,
    rol_id              INT     NOT NULL,
    departamento_id     INT     NOT NULL,
    CONSTRAINT UQ_UsuariosRoles         UNIQUE (usuario_id, rol_id, departamento_id),
    CONSTRAINT FK_UsuRoles_Usuario      FOREIGN KEY (usuario_id)    REFERENCES Usuarios(id),
    CONSTRAINT FK_UsuRoles_Rol          FOREIGN KEY (rol_id)        REFERENCES Roles(id),
    CONSTRAINT FK_UsuRoles_Depto        FOREIGN KEY (departamento_id) REFERENCES Departamentos(id)
);
GO


-- =============================================================================
-- PILAR B — Gestión documental (Maestro - Detalle)
-- =============================================================================

-- Catálogo de normas (ISO 9001, IATF 16949, etc.)
CREATE TABLE Normas (
    id          INT             IDENTITY(1,1)   PRIMARY KEY,
    codigo      VARCHAR(30)     NOT NULL,           -- ej. 'ISO 9001'
    nombre      NVARCHAR(150)   NOT NULL,           -- ej. 'Sistemas de Gestión de Calidad'
    version     VARCHAR(20)     NULL,               -- ej. '2015'
    CONSTRAINT UQ_Normas_Codigo UNIQUE (codigo)
);
GO



-- Catálogo de niveles ISO (elimina el entero mágico 1-4)
CREATE TABLE Niveles_Documento (
    id      INT             IDENTITY(1,1)   PRIMARY KEY,
    numero  TINYINT         NOT NULL,
    nombre  NVARCHAR(80)    NOT NULL,
    CONSTRAINT UQ_Niveles_Numero UNIQUE (numero)
);
GO



-- Tabla MAESTRA: identidad permanente del documento (nunca se sobreescribe)
CREATE TABLE Documentos (
    id                  INT             IDENTITY(1,1)   PRIMARY KEY,
    codigo              VARCHAR(30)     NOT NULL,           -- ej. 'PR-01'
    nombre              NVARCHAR(255)   NOT NULL,
    nivel_id            INT             NOT NULL,
    norma_id            INT             NOT NULL,
    departamento_id     INT             NOT NULL,
    creado_por          INT             NOT NULL,
    creado_en           DATETIME        NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_Documentos_Codigo     UNIQUE (codigo),
    CONSTRAINT FK_Docs_Nivel            FOREIGN KEY (nivel_id)          REFERENCES Niveles_Documento(id),
    CONSTRAINT FK_Docs_Norma            FOREIGN KEY (norma_id)           REFERENCES Normas(id),
    CONSTRAINT FK_Docs_Depto            FOREIGN KEY (departamento_id)    REFERENCES Departamentos(id),
    CONSTRAINT FK_Docs_Creador          FOREIGN KEY (creado_por)         REFERENCES Usuarios(id)
);
GO

-- Tabla DETALLE: ciclo de vida por versión
-- version_mayor.version_menor  ej. 1.3 → version_mayor=1, version_menor=3
CREATE TABLE Versiones_Documento (
    id                      INT             IDENTITY(1,1)   PRIMARY KEY,
    documento_id            INT             NOT NULL,
    version_mayor           TINYINT         NOT NULL DEFAULT 1,
    version_menor           TINYINT         NOT NULL DEFAULT 0,
    estado                  VARCHAR(20)     NOT NULL DEFAULT 'Borrador',
    minio_identifier        VARCHAR(500)    NOT NULL,   -- ruta/URL del archivo físico en MinIO
    creado_por              INT             NOT NULL,
    fecha_creacion          DATETIME        NOT NULL DEFAULT GETDATE(),
    fecha_aprobacion        DATETIME        NULL,
    fecha_obsolescencia     DATETIME        NULL,
    CONSTRAINT CHK_Versiones_Estado CHECK (estado IN ('Borrador', 'Revision', 'Aprobado', 'Obsoleto')),
    CONSTRAINT UQ_Versiones_Numero  UNIQUE (documento_id, version_mayor, version_menor),
    CONSTRAINT FK_Ver_Documento     FOREIGN KEY (documento_id)  REFERENCES Documentos(id),
    CONSTRAINT FK_Ver_Creador       FOREIGN KEY (creado_por)    REFERENCES Usuarios(id)
);
GO

-- Índice filtrado: garantiza que solo exista UNA versión 'Aprobado' por documento
-- Si el backend falla al marcar la anterior como Obsoleto, la BD lo bloquea
CREATE UNIQUE INDEX UX_UnAprobadoPorDocumento
    ON Versiones_Documento (documento_id)
    WHERE estado = 'Aprobado';
GO


-- =============================================================================
-- PILAR C — Flujos de autorización (Workflows)
-- =============================================================================

-- Plantilla de firmas por documento: define quién firma y en qué orden
-- Se crea una sola vez al registrar el documento. Nunca cambia por sí sola.
-- El backend la lee como referencia al generar Flujos_Aprobacion.
CREATE TABLE Secuencia_Firma (
    id              INT             IDENTITY(1,1)   PRIMARY KEY,
    documento_id    INT             NOT NULL,
    rol_id          INT             NOT NULL,
    tipo_firma      VARCHAR(20)     NOT NULL,
    orden           TINYINT         NOT NULL,
    CONSTRAINT CHK_SecFirma_Tipo    CHECK (tipo_firma IN ('Elaboró', 'Revisó', 'Aprobó')),
    CONSTRAINT UQ_SecFirma_Orden    UNIQUE (documento_id, orden),   -- no puede haber dos pasos con el mismo orden
    CONSTRAINT FK_SecFirma_Doc      FOREIGN KEY (documento_id)  REFERENCES Documentos(id),
    CONSTRAINT FK_SecFirma_Rol      FOREIGN KEY (rol_id)        REFERENCES Roles(id)
);
GO

-- Instancia viva del workflow: una fila por firmante por versión
-- Generada automáticamente por el backend al crear una Versiones_Documento,
-- leyendo la plantilla de Secuencia_Firma y resolviendo usuario concreto por rol+depto.
CREATE TABLE Flujos_Aprobacion (
    id              INT             IDENTITY(1,1)   PRIMARY KEY,
    version_id      INT             NOT NULL,
    usuario_id      INT             NOT NULL,
    tipo_firma      VARCHAR(20)     NOT NULL,
    orden           TINYINT         NOT NULL,   -- copiado de Secuencia_Firma al generar el flujo
    estado_firma    VARCHAR(20)     NOT NULL DEFAULT 'Pendiente',
    comentarios     NVARCHAR(500)   NULL,
    fecha_firma     DATETIME        NULL,
    CONSTRAINT CHK_Flujos_Tipo      CHECK (tipo_firma   IN ('Elaboró', 'Revisó', 'Aprobó')),
    CONSTRAINT CHK_Flujos_Estado    CHECK (estado_firma IN ('Pendiente', 'Aprobado', 'Rechazado')),
    CONSTRAINT FK_Flujos_Version    FOREIGN KEY (version_id)    REFERENCES Versiones_Documento(id),
    CONSTRAINT FK_Flujos_Usuario    FOREIGN KEY (usuario_id)    REFERENCES Usuarios(id)
);
GO


-- =============================================================================
-- ÍNDICES adicionales de rendimiento
-- =============================================================================

-- Búsquedas frecuentes por estado de versión (ej. traer todos los 'Aprobado')
CREATE INDEX IX_Versiones_Estado
    ON Versiones_Documento (estado)
    INCLUDE (documento_id, version_mayor, version_menor, minio_identifier);
GO

-- Búsquedas de flujos pendientes por usuario (ej. "mis firmas pendientes")
CREATE INDEX IX_Flujos_UsuarioEstado
    ON Flujos_Aprobacion (usuario_id, estado_firma)
    INCLUDE (version_id, tipo_firma, orden);
GO

-- Búsqueda de documentos por departamento
CREATE INDEX IX_Documentos_Depto
    ON Documentos (departamento_id)
    INCLUDE (codigo, nombre, nivel_id);
GO