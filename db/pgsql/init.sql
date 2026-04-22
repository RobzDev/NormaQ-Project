-- =============================================================================
-- QualityDoc-Polyglot | PostgreSQL DDL
-- Módulo de Auditoría — Portal de Consulta PHP
-- Bitácora de interacciones usuario-archivo (ISO 9001 Cláusula 7.5)
-- =============================================================================

-- La base se crea desde docker-compose con POSTGRES_DB.
-- Este script se ejecuta conectado a esa base (-d $PG_DB).

-- =============================================================================
-- Tabla principal de logs
-- =============================================================================

CREATE TABLE IF NOT EXISTS logs_auditoria (
    id                  BIGSERIAL       PRIMARY KEY,
    fecha_hora          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    usuario_id          INT             NOT NULL,
    usuario_nombre      VARCHAR(150)    NOT NULL,
    departamento_nombre VARCHAR(100)    NOT NULL,
    documento_codigo    VARCHAR(30)     NOT NULL,
    documento_nombre    VARCHAR(255)    NOT NULL,
    version_documento   VARCHAR(10)     NOT NULL,   -- ej. '1.3'
    accion              VARCHAR(20)     NOT NULL,
    ip_origen           VARCHAR(45)     NULL,        -- soporta IPv4 e IPv6
    modulo_origen       VARCHAR(30)     NOT NULL DEFAULT 'portal_php',
    CONSTRAINT chk_accion CHECK (accion IN ('Lectura', 'Descarga'))
);

-- =============================================================================
-- Índices orientados a los reportes más frecuentes en auditorías ISO
-- =============================================================================

-- "¿Quién accedió a qué documentos?" — filtro por usuario
CREATE INDEX ix_logs_usuario
    ON logs_auditoria (usuario_id, fecha_hora DESC);

-- "¿Quién vio este documento y cuándo?" — filtro por documento
CREATE INDEX ix_logs_documento
    ON logs_auditoria (documento_codigo, fecha_hora DESC);

-- "Actividad del período X" — filtro por rango de fechas
CREATE INDEX ix_logs_fecha
    ON logs_auditoria (fecha_hora DESC);

-- "Actividad por departamento" — reportes de cumplimiento por área
CREATE INDEX ix_logs_departamento
    ON logs_auditoria (departamento_nombre, fecha_hora DESC);