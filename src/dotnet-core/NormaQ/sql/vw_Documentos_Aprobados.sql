CREATE VIEW vw_Documentos_Aprobados AS
SELECT
    vd.id                                            AS version_id,
    d.id                                             AS documento_id,
    d.codigo                                         AS codigo_documento,
    d.nombre                                         AS nombre_documento,
    nd.nombre                                        AS nivel,
    n.codigo                                         AS norma_codigo,
    n.nombre                                         AS norma_nombre,
    dep.nombre                                       AS departamento,
    CONCAT(vd.version_mayor, '.', vd.version_menor)  AS version,
    u_owner.nombre                                   AS owner,
    u_aprobador.nombre                               AS approved_by,
    vd.fecha_aprobacion                              AS approved_at,
    vd.fecha_creacion                                AS created_at,
    vd.minio_identifier                              AS storage_path
FROM Versiones_Documento vd
INNER JOIN Documentos        d            ON d.id           = vd.documento_id
INNER JOIN Niveles_Documento nd           ON nd.id          = d.nivel_id
INNER JOIN Normas            n            ON n.id           = d.norma_id
INNER JOIN Departamentos     dep          ON dep.id         = d.departamento_id
INNER JOIN Usuarios          u_owner      ON u_owner.id     = vd.creado_por
LEFT  JOIN Flujos_Aprobacion fa           ON fa.version_id  = vd.id
                                         AND fa.tipo_firma  = 'Aprobó'
                                         AND fa.estado_firma = 'Aprobado'
LEFT  JOIN Usuarios          u_aprobador  ON u_aprobador.id = fa.usuario_id
WHERE vd.estado = 'Aprobado';
GO