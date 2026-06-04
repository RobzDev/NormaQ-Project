-- =============================================================================
-- QualityDoc-Polyglot | Vistas Dashboard — aliases en PascalCase
-- EF Core mapea las propiedades C# directamente al nombre de columna.
-- Todos los aliases coinciden exactamente con las propiedades del modelo C#.
-- =============================================================================


-- =============================================================================
-- VISTA 1: vw_Snapshot_Depto
-- =============================================================================
CREATE OR ALTER VIEW vw_Snapshot_Depto AS
SELECT
    d.departamento_id                                           AS DepartamentoId,
    dep.nombre                                                  AS Departamento,
    COUNT(CASE WHEN v.estado = 'Borrador'  THEN 1 END)         AS Borradores,
    COUNT(CASE WHEN v.estado = 'Revision'  THEN 1 END)         AS EnRevision,
    COUNT(CASE WHEN v.estado = 'Aprobado'  THEN 1 END)         AS Aprobados,
    COUNT(CASE WHEN v.estado = 'Obsoleto'  THEN 1 END)         AS Obsoletos,
    COUNT(*)                                                    AS TotalVersiones
FROM Versiones_Documento v
INNER JOIN Documentos    d   ON d.id   = v.documento_id
INNER JOIN Departamentos dep ON dep.id = d.departamento_id
GROUP BY d.departamento_id, dep.nombre;
GO


-- =============================================================================
-- VISTA 2: vw_Actividad_Semanal
-- =============================================================================
CREATE OR ALTER VIEW vw_Actividad_Semanal AS
SELECT
    d.departamento_id                                           AS DepartamentoId,
    ca.anio                                                     AS Anio,
    ca.semana                                                   AS Semana,
    ca.inicio_semana                                            AS InicioSemana,
    COUNT(*)                                                    AS VersionesCreadas,
    COUNT(v.fecha_aprobacion)                                   AS VersionesAprobadas,
    COUNT(CASE WHEN v.estado = 'Borrador'
               AND fa.version_id IS NOT NULL THEN 1 END)        AS VersionesRechazadas
FROM Versiones_Documento v
INNER JOIN Documentos d ON d.id = v.documento_id
LEFT JOIN (
    SELECT DISTINCT version_id
    FROM Flujos_Aprobacion
    WHERE estado_firma = 'Rechazado'
) fa ON fa.version_id = v.id
CROSS APPLY (
    SELECT
        DATEPART(YEAR,  v.fecha_creacion)                                               AS anio,
        DATEPART(WEEK,  v.fecha_creacion)                                               AS semana,
        DATEADD(DAY, 1 - DATEPART(WEEKDAY, v.fecha_creacion), CAST(v.fecha_creacion AS DATE)) AS inicio_semana
) ca
WHERE v.fecha_creacion >= DATEADD(DAY, -30, GETDATE())
GROUP BY
    d.departamento_id,
    ca.anio,
    ca.semana,
    ca.inicio_semana;
GO


-- =============================================================================
-- VISTA 3: vw_Documentos_Mas_Versiones
-- =============================================================================
CREATE OR ALTER VIEW vw_Documentos_Mas_Versiones AS
SELECT
    d.departamento_id                                               AS DepartamentoId,
    d.id                                                            AS DocumentoId,
    d.codigo                                                        AS Codigo,
    d.nombre                                                        AS Documento,
    nd.nombre                                                       AS Nivel,
    COUNT(DISTINCT v.id)                                            AS TotalVersiones,
    COUNT(DISTINCT CASE WHEN v.estado = 'Aprobado' THEN v.id END)  AS VersionesAprobadas,
    COUNT(DISTINCT CASE WHEN v.estado = 'Obsoleto' THEN v.id END)  AS VersionesObsoletas,
    COUNT(DISTINCT CASE WHEN fa.version_id IS NOT NULL
                        THEN v.id END)                              AS VersionesConRechazo
FROM Documentos d
INNER JOIN Versiones_Documento v  ON v.documento_id = d.id
INNER JOIN Niveles_Documento   nd ON nd.id          = d.nivel_id
LEFT JOIN (
    SELECT DISTINCT version_id
    FROM Flujos_Aprobacion
    WHERE estado_firma = 'Rechazado'
) fa ON fa.version_id = v.id
GROUP BY d.departamento_id, d.id, d.codigo, d.nombre, nd.nombre;
GO


-- =============================================================================
-- VISTA 4: vw_Firmas_Pendientes
-- =============================================================================
CREATE OR ALTER VIEW vw_Firmas_Pendientes AS
SELECT
    doc.departamento_id                                         AS DepartamentoId,
    u.id                                                        AS UsuarioId,
    u.nombre                                                    AS Usuario,
    r.nombre                                                    AS Rol,
    fa.tipo_firma                                               AS TipoFirma,
    fa.orden                                                    AS Orden,
    doc.codigo                                                  AS DocumentoCodigo,
    doc.nombre                                                  AS Documento,
    CONCAT(v.version_mayor, '.', v.version_menor)               AS Version,
    v.fecha_creacion                                            AS VersionCreadaEn
FROM Flujos_Aprobacion fa
INNER JOIN Versiones_Documento v   ON v.id    = fa.version_id
INNER JOIN Documentos          doc ON doc.id  = v.documento_id
INNER JOIN Departamentos       d   ON d.id    = doc.departamento_id
INNER JOIN Usuarios            u   ON u.id    = fa.usuario_id
INNER JOIN Usuarios_Roles      ur  ON ur.usuario_id      = fa.usuario_id
                                  AND ur.departamento_id  = doc.departamento_id
INNER JOIN Roles               r   ON r.id    = ur.rol_id
WHERE
    fa.estado_firma = 'Pendiente'
    AND u.activo    = 1
    AND NOT EXISTS (
        SELECT 1 FROM Flujos_Aprobacion fa_prev
        WHERE fa_prev.version_id   = fa.version_id
          AND fa_prev.orden        < fa.orden
          AND fa_prev.estado_firma != 'Aprobado'
    );
GO


-- =============================================================================
-- VISTA 5: vw_Usuarios_Activos
-- =============================================================================
CREATE OR ALTER VIEW vw_Usuarios_Activos AS
SELECT
    doc.departamento_id                                         AS DepartamentoId,
    u.id                                                        AS UsuarioId,
    u.nombre                                                    AS Usuario,
    r.nombre                                                    AS Rol,
    COUNT(fa.id)                                                AS TotalFirmas,
    COUNT(CASE WHEN fa.estado_firma = 'Aprobado'  THEN 1 END)  AS FirmasAprobadas,
    COUNT(CASE WHEN fa.estado_firma = 'Rechazado' THEN 1 END)  AS FirmasRechazadas,
    MAX(fa.fecha_firma)                                         AS UltimaFirma
FROM Flujos_Aprobacion fa
INNER JOIN Versiones_Documento v   ON v.id    = fa.version_id
INNER JOIN Documentos          doc ON doc.id  = v.documento_id
INNER JOIN Departamentos       d   ON d.id    = doc.departamento_id
INNER JOIN Usuarios            u   ON u.id    = fa.usuario_id
INNER JOIN Usuarios_Roles      ur  ON ur.usuario_id      = fa.usuario_id
                                  AND ur.departamento_id  = doc.departamento_id
INNER JOIN Roles               r   ON r.id    = ur.rol_id
WHERE
    fa.fecha_firma    >= DATEADD(DAY, -30, GETDATE())
    AND fa.estado_firma IN ('Aprobado', 'Rechazado')
GROUP BY doc.departamento_id, u.id, u.nombre, r.nombre;
GO