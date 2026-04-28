-- =========================
-- Roles
-- =========================
MERGE INTO Roles AS target
USING (VALUES
    ('Administrador', 'Gestión total del sistema'),
    ('Aprobador',     'Firma de aprobación final'),
    ('Revisor',       'Firma de revisión técnica'),
    ('Elaborador',    'Crea y edita borradores'),
    ('Operario',      'Solo lectura de documentos aprobados')
) AS source (nombre, descripcion)
ON target.nombre = source.nombre
WHEN NOT MATCHED THEN
    INSERT (nombre, descripcion)
    VALUES (source.nombre, source.descripcion);

-- =========================
-- Normas
-- =========================
MERGE INTO Normas AS target
USING (VALUES
    ('ISO 9001',   'Sistemas de Gestión de Calidad',            '2015'),
    ('IATF 16949', 'Sistemas de Gestión de Calidad Automotriz', '2016'),
    ('ISO 14001',  'Sistemas de Gestión Ambiental',             '2015')
) AS source (codigo, nombre, version)
ON target.codigo = source.codigo
WHEN NOT MATCHED THEN
    INSERT (codigo, nombre, version)
    VALUES (source.codigo, source.nombre, source.version);

-- =========================
-- Niveles de Documento
-- =========================
MERGE INTO Niveles_Documento AS target
USING (VALUES
    (1, 'Manual de Calidad'),
    (2, 'Procedimiento'),
    (3, 'Instrucción de Trabajo'),
    (4, 'Registro y Formato')
) AS source (numero, nombre)
ON target.numero = source.numero
WHEN NOT MATCHED THEN
    INSERT (numero, nombre)
    VALUES (source.numero, source.nombre);





MERGE INTO Companias AS target
USING (VALUES
    ('Empresa Demo S.A. de C.V.', 'DEM010101ABC', 'Av. Principal 123, Ciudad de México, CDMX', 1)
) AS source (nombre, rfc, direccion, activo)
ON target.rfc = source.rfc
WHEN NOT MATCHED THEN
    INSERT (nombre, rfc, direccion, activo)
    VALUES (source.nombre, source.rfc, source.direccion, source.activo);

-- =========================
-- Departamentos
-- =========================
MERGE INTO Departamentos AS target
USING (VALUES
    ('Calidad', 1)
) AS source (nombre, compania_id)
ON target.nombre = source.nombre AND target.compania_id = source.compania_id
WHEN NOT MATCHED THEN
    INSERT (nombre, compania_id)
    VALUES (source.nombre, source.compania_id);
