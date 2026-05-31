-- =============================================================================
-- QualityDoc-Polyglot | Seeding Oficial Demo
-- Reproducible: usa MERGE en todos los bloques para ser idempotente.
-- Seguro para migraciones EF Core: no duplica datos al reiniciar el contenedor.
-- =============================================================================

-- =============================================================================
-- 1. CATÁLOGOS BASE
-- =============================================================================

-- Roles
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
    INSERT (nombre, descripcion) VALUES (source.nombre, source.descripcion);
GO

-- Normas
MERGE INTO Normas AS target
USING (VALUES
    ('ISO 9001',    'Sistemas de Gestión de Calidad',            '2015'),
    ('IATF 16949',  'Sistemas de Gestión de Calidad Automotriz', '2016'),
    ('ISO 14001',   'Sistemas de Gestión Ambiental',             '2015')
) AS source (codigo, nombre, version)
ON target.codigo = source.codigo
WHEN NOT MATCHED THEN
    INSERT (codigo, nombre, version) VALUES (source.codigo, source.nombre, source.version);
GO

-- Niveles de Documento
MERGE INTO Niveles_Documento AS target
USING (VALUES
    (1, 'Manual de Calidad'),
    (2, 'Procedimiento'),
    (3, 'Instrucción de Trabajo'),
    (4, 'Registro y Formato')
) AS source (numero, nombre)
ON target.numero = source.numero
WHEN NOT MATCHED THEN
    INSERT (numero, nombre) VALUES (source.numero, source.nombre);
GO


-- =============================================================================
-- 2. COMPAÑÍAS (4)
-- =============================================================================

MERGE INTO Companias AS target
USING (VALUES
    ('Empresa Demo S.A. de C.V.',       'DEM010101ABC', 'Av. Principal 123, Ciudad de México, CDMX',      1),
    ('Manufactura Norteña S.A. de C.V.','MAN850301XYZ', 'Blvd. Industria 456, Monterrey, NL',            1),
    ('Grupo Logístico del Bajío S.C.',  'GLB920615QRS', 'Calle Comercio 789, León, Guanajuato',          1),
    ('Soluciones Técnicas del Norte',   'STN010203DEF', 'Av. Tecnológico 321, Saltillo, Coahuila',       1)
) AS source (nombre, rfc, direccion, activo)
ON target.rfc = source.rfc
WHEN NOT MATCHED THEN
    INSERT (nombre, rfc, direccion, activo)
    VALUES (source.nombre, source.rfc, source.direccion, source.activo);
GO


-- =============================================================================
-- 3. DEPARTAMENTOS (4 por compañía = 16 total)
-- =============================================================================

MERGE INTO Departamentos AS target
USING (VALUES
    -- Compañía 1: Empresa Demo
    ('Calidad',           (SELECT id FROM Companias WHERE rfc = 'DEM010101ABC')),
    ('Producción',        (SELECT id FROM Companias WHERE rfc = 'DEM010101ABC')),
    ('Recursos Humanos',  (SELECT id FROM Companias WHERE rfc = 'DEM010101ABC')),
    ('Contaduría',        (SELECT id FROM Companias WHERE rfc = 'DEM010101ABC')),
    -- Compañía 2: Manufactura Norteña
    ('Calidad',           (SELECT id FROM Companias WHERE rfc = 'MAN850301XYZ')),
    ('Producción',        (SELECT id FROM Companias WHERE rfc = 'MAN850301XYZ')),
    ('Logística',         (SELECT id FROM Companias WHERE rfc = 'MAN850301XYZ')),
    ('Mantenimiento',     (SELECT id FROM Companias WHERE rfc = 'MAN850301XYZ')),
    -- Compañía 3: Grupo Logístico
    ('Operaciones',       (SELECT id FROM Companias WHERE rfc = 'GLB920615QRS')),
    ('Calidad',           (SELECT id FROM Companias WHERE rfc = 'GLB920615QRS')),
    ('Compras',           (SELECT id FROM Companias WHERE rfc = 'GLB920615QRS')),
    ('Sistemas',          (SELECT id FROM Companias WHERE rfc = 'GLB920615QRS')),
    -- Compañía 4: Soluciones Técnicas
    ('Ingeniería',        (SELECT id FROM Companias WHERE rfc = 'STN010203DEF')),
    ('Calidad',           (SELECT id FROM Companias WHERE rfc = 'STN010203DEF')),
    ('Proyectos',         (SELECT id FROM Companias WHERE rfc = 'STN010203DEF')),
    ('Soporte Técnico',   (SELECT id FROM Companias WHERE rfc = 'STN010203DEF'))
) AS source (nombre, compania_id)
ON target.nombre = source.nombre AND target.compania_id = source.compania_id
WHEN NOT MATCHED THEN
    INSERT (nombre, compania_id) VALUES (source.nombre, source.compania_id);
GO


-- =============================================================================
-- 4. USUARIOS
-- Hash uniforme: $2a$12$lR1j5EtJ8qHylHYwnwg5Du2IAIm9qGgKzm6Wfd9fGvFcCwx240YYG
-- Regla: departamento_id = departamento base (donde fue creado)
-- Compañía 1 Depto Calidad → correos reales de prueba
-- Resto → correos inventados
-- =============================================================================

DECLARE @hash VARCHAR(255) = '$2a$12$lR1j5EtJ8qHylHYwnwg5Du2IAIm9qGgKzm6Wfd9fGvFcCwx240YYG';

-- -------------------------------------------------------
-- COMPAÑÍA 1 — Empresa Demo
-- -------------------------------------------------------

-- Depto: Calidad (correos reales de prueba)
MERGE INTO Usuarios AS target
USING (VALUES
    ('Ramiro César Espinoza',   'rcespinoza04@gmail.com',            @hash, (SELECT id FROM Departamentos WHERE nombre='Calidad'    AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1),
    ('Roberto X. Martínez',     'Roberto_Espinoza.Dev@outlook.com',  @hash, (SELECT id FROM Departamentos WHERE nombre='Calidad'    AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1),
    ('Jorge Cárdenas',          'rcardiel4@gmail.com',               @hash, (SELECT id FROM Departamentos WHERE nombre='Calidad'    AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1),
    ('Ana Sofía Reyes',         'I23050336@monclova.tecnm.mx',       @hash, (SELECT id FROM Departamentos WHERE nombre='Calidad'    AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1),
    ('Operario Demo Calidad',   'operario.calidad.demo@qualitydoc.mx',@hash,(SELECT id FROM Departamentos WHERE nombre='Calidad'    AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1),
    -- Admin 2 Calidad
    ('Laura Medina Ortiz',      'lmedina.calidad@qualitydoc.mx',     @hash, (SELECT id FROM Departamentos WHERE nombre='Calidad'    AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1)
) AS source (nombre, email, password_hash, departamento_id, activo)
ON target.email = source.email
WHEN NOT MATCHED THEN
    INSERT (nombre, email, password_hash, departamento_id, activo)
    VALUES (source.nombre, source.email, source.password_hash, source.departamento_id, source.activo);

-- Depto: Producción
MERGE INTO Usuarios AS target
USING (VALUES
    ('Carlos Ibarra Núñez',     'carbarra.prod@qualitydoc.mx',       @hash, (SELECT id FROM Departamentos WHERE nombre='Producción' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1),
    ('Fernanda Lozano',         'flozano.prod@qualitydoc.mx',        @hash, (SELECT id FROM Departamentos WHERE nombre='Producción' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1),
    ('Miguel Ángel Torres',     'matorres.prod@qualitydoc.mx',       @hash, (SELECT id FROM Departamentos WHERE nombre='Producción' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1),
    ('Daniela Herrera',         'dherrera.prod@qualitydoc.mx',       @hash, (SELECT id FROM Departamentos WHERE nombre='Producción' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1),
    ('Operario Demo Producción','operario.prod.demo@qualitydoc.mx',  @hash, (SELECT id FROM Departamentos WHERE nombre='Producción' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1),
    -- Admin 2 Producción
    ('Sofía Guerrero Paz',      'sguerrero.prod@qualitydoc.mx',      @hash, (SELECT id FROM Departamentos WHERE nombre='Producción' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1)
) AS source (nombre, email, password_hash, departamento_id, activo)
ON target.email = source.email
WHEN NOT MATCHED THEN
    INSERT (nombre, email, password_hash, departamento_id, activo)
    VALUES (source.nombre, source.email, source.password_hash, source.departamento_id, source.activo);

-- Depto: Recursos Humanos
MERGE INTO Usuarios AS target
USING (VALUES
    ('Patricia Villanueva',     'pvillanueva.rh@qualitydoc.mx',      @hash, (SELECT id FROM Departamentos WHERE nombre='Recursos Humanos' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1),
    ('Eduardo Soto Lima',       'esoto.rh@qualitydoc.mx',            @hash, (SELECT id FROM Departamentos WHERE nombre='Recursos Humanos' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1),
    ('Valeria Cruz Mora',       'vcruz.rh@qualitydoc.mx',            @hash, (SELECT id FROM Departamentos WHERE nombre='Recursos Humanos' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1),
    ('Héctor Alvarado',         'halvarado.rh@qualitydoc.mx',        @hash, (SELECT id FROM Departamentos WHERE nombre='Recursos Humanos' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1),
    ('Operario Demo RH',        'operario.rh.demo@qualitydoc.mx',    @hash, (SELECT id FROM Departamentos WHERE nombre='Recursos Humanos' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1),
    -- Admin 2 RH
    ('Gabriela Ríos Castillo',  'grios.rh@qualitydoc.mx',            @hash, (SELECT id FROM Departamentos WHERE nombre='Recursos Humanos' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1)
) AS source (nombre, email, password_hash, departamento_id, activo)
ON target.email = source.email
WHEN NOT MATCHED THEN
    INSERT (nombre, email, password_hash, departamento_id, activo)
    VALUES (source.nombre, source.email, source.password_hash, source.departamento_id, source.activo);

-- Depto: Contaduría
MERGE INTO Usuarios AS target
USING (VALUES
    ('Beatriz Sandoval',        'bsandoval.cont@qualitydoc.mx',      @hash, (SELECT id FROM Departamentos WHERE nombre='Contaduría'  AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1),
    ('Rodrigo Fuentes',         'rfuentes.cont@qualitydoc.mx',       @hash, (SELECT id FROM Departamentos WHERE nombre='Contaduría'  AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1),
    ('Mariana Peña López',      'mpena.cont@qualitydoc.mx',          @hash, (SELECT id FROM Departamentos WHERE nombre='Contaduría'  AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1),
    ('Óscar Delgado',           'odelgado.cont@qualitydoc.mx',       @hash, (SELECT id FROM Departamentos WHERE nombre='Contaduría'  AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1),
    ('Operario Demo Contaduría','operario.cont.demo@qualitydoc.mx',  @hash, (SELECT id FROM Departamentos WHERE nombre='Contaduría'  AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1),
    -- Admin 2 Contaduría
    ('Isabel Moreno Vega',      'imoreno.cont@qualitydoc.mx',        @hash, (SELECT id FROM Departamentos WHERE nombre='Contaduría'  AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), 1)
) AS source (nombre, email, password_hash, departamento_id, activo)
ON target.email = source.email
WHEN NOT MATCHED THEN
    INSERT (nombre, email, password_hash, departamento_id, activo)
    VALUES (source.nombre, source.email, source.password_hash, source.departamento_id, source.activo);
GO


-- =============================================================================
-- 5. USUARIOS_ROLES — Compañía 1
-- Reglas:
--   - Todo usuario tiene rol en su departamento base
--   - 2 Admins por depto, solo admin (no roles cruzados)
--   - 1 Operario por depto, solo operario
--   - Aprobadores pueden tener rol Revisor en otro depto misma compañía
--   - Revisores pueden tener rol Aprobador en otro depto misma compañía
--   - 1 Elaborador con rol Revisor en otro depto (test de bugs)
-- =============================================================================

-- Helper: subqueries reutilizables por nombre para Cía 1
-- Calidad    = C1_CAL, Producción = C1_PRO, RH = C1_RH, Contaduría = C1_CON

-- -------------------------------------------------------
-- DEPTO: CALIDAD (Empresa Demo)
-- Ramiro   → Admin
-- Laura    → Admin
-- Jorge    → Aprobador  + Revisor en Producción (cross-depto)
-- Ana Sofía→ Revisor    + Aprobador en Contaduría (cross-depto)
-- Roberto  → Elaborador + Revisor en RH (test de bugs)
-- Operario → Operario
-- -------------------------------------------------------
MERGE INTO Usuarios_Roles AS target
USING (VALUES
    -- Ramiro: Admin en Calidad
    ((SELECT id FROM Usuarios WHERE email='rcespinoza04@gmail.com'),
     (SELECT id FROM Roles WHERE nombre='Administrador'),
     (SELECT id FROM Departamentos WHERE nombre='Calidad' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Laura: Admin en Calidad
    ((SELECT id FROM Usuarios WHERE email='lmedina.calidad@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Administrador'),
     (SELECT id FROM Departamentos WHERE nombre='Calidad' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Jorge: Aprobador en Calidad (base)
    ((SELECT id FROM Usuarios WHERE email='rcardiel4@gmail.com'),
     (SELECT id FROM Roles WHERE nombre='Aprobador'),
     (SELECT id FROM Departamentos WHERE nombre='Calidad' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Jorge: Revisor en Producción (cross-depto, misma compañía)
    ((SELECT id FROM Usuarios WHERE email='rcardiel4@gmail.com'),
     (SELECT id FROM Roles WHERE nombre='Revisor'),
     (SELECT id FROM Departamentos WHERE nombre='Producción' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Ana Sofía: Revisor en Calidad (base)
    ((SELECT id FROM Usuarios WHERE email='I23050336@monclova.tecnm.mx'),
     (SELECT id FROM Roles WHERE nombre='Revisor'),
     (SELECT id FROM Departamentos WHERE nombre='Calidad' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Ana Sofía: Aprobador en Contaduría (cross-depto, misma compañía)
    ((SELECT id FROM Usuarios WHERE email='I23050336@monclova.tecnm.mx'),
     (SELECT id FROM Roles WHERE nombre='Aprobador'),
     (SELECT id FROM Departamentos WHERE nombre='Contaduría' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Roberto: Elaborador en Calidad (base)
    ((SELECT id FROM Usuarios WHERE email='Roberto_Espinoza.Dev@outlook.com'),
     (SELECT id FROM Roles WHERE nombre='Elaborador'),
     (SELECT id FROM Departamentos WHERE nombre='Calidad' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Roberto: Revisor en RH (cross-depto — test de bugs con elaborador)
    ((SELECT id FROM Usuarios WHERE email='Roberto_Espinoza.Dev@outlook.com'),
     (SELECT id FROM Roles WHERE nombre='Revisor'),
     (SELECT id FROM Departamentos WHERE nombre='Recursos Humanos' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Operario Calidad
    ((SELECT id FROM Usuarios WHERE email='operario.calidad.demo@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Operario'),
     (SELECT id FROM Departamentos WHERE nombre='Calidad' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')))
) AS source (usuario_id, rol_id, departamento_id)
ON target.usuario_id = source.usuario_id AND target.rol_id = source.rol_id AND target.departamento_id = source.departamento_id
WHEN NOT MATCHED THEN
    INSERT (usuario_id, rol_id, departamento_id) VALUES (source.usuario_id, source.rol_id, source.departamento_id);
GO

-- -------------------------------------------------------
-- DEPTO: PRODUCCIÓN (Empresa Demo)
-- Carlos    → Admin
-- Sofía G.  → Admin
-- Fernanda  → Aprobador + Revisor en RH (cross-depto)
-- Miguel    → Revisor   + Aprobador en Calidad (cross-depto)
-- Daniela   → Elaborador
-- Operario  → Operario
-- -------------------------------------------------------
MERGE INTO Usuarios_Roles AS target
USING (VALUES
    ((SELECT id FROM Usuarios WHERE email='carbarra.prod@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Administrador'),
     (SELECT id FROM Departamentos WHERE nombre='Producción' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    ((SELECT id FROM Usuarios WHERE email='sguerrero.prod@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Administrador'),
     (SELECT id FROM Departamentos WHERE nombre='Producción' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Fernanda: Aprobador base
    ((SELECT id FROM Usuarios WHERE email='flozano.prod@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Aprobador'),
     (SELECT id FROM Departamentos WHERE nombre='Producción' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Fernanda: Revisor en RH (cross-depto)
    ((SELECT id FROM Usuarios WHERE email='flozano.prod@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Revisor'),
     (SELECT id FROM Departamentos WHERE nombre='Recursos Humanos' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Miguel: Revisor base
    ((SELECT id FROM Usuarios WHERE email='matorres.prod@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Revisor'),
     (SELECT id FROM Departamentos WHERE nombre='Producción' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Miguel: Aprobador en Calidad (cross-depto)
    ((SELECT id FROM Usuarios WHERE email='matorres.prod@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Aprobador'),
     (SELECT id FROM Departamentos WHERE nombre='Calidad' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Daniela: Elaborador base
    ((SELECT id FROM Usuarios WHERE email='dherrera.prod@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Elaborador'),
     (SELECT id FROM Departamentos WHERE nombre='Producción' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Operario Producción
    ((SELECT id FROM Usuarios WHERE email='operario.prod.demo@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Operario'),
     (SELECT id FROM Departamentos WHERE nombre='Producción' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')))
) AS source (usuario_id, rol_id, departamento_id)
ON target.usuario_id = source.usuario_id AND target.rol_id = source.rol_id AND target.departamento_id = source.departamento_id
WHEN NOT MATCHED THEN
    INSERT (usuario_id, rol_id, departamento_id) VALUES (source.usuario_id, source.rol_id, source.departamento_id);
GO

-- -------------------------------------------------------
-- DEPTO: RECURSOS HUMANOS (Empresa Demo)
-- Patricia  → Admin
-- Gabriela  → Admin
-- Eduardo   → Aprobador + Revisor en Contaduría (cross-depto)
-- Valeria   → Revisor   + Aprobador en Producción (cross-depto)
-- Héctor    → Elaborador
-- Operario  → Operario
-- -------------------------------------------------------
MERGE INTO Usuarios_Roles AS target
USING (VALUES
    ((SELECT id FROM Usuarios WHERE email='pvillanueva.rh@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Administrador'),
     (SELECT id FROM Departamentos WHERE nombre='Recursos Humanos' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    ((SELECT id FROM Usuarios WHERE email='grios.rh@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Administrador'),
     (SELECT id FROM Departamentos WHERE nombre='Recursos Humanos' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Eduardo: Aprobador base
    ((SELECT id FROM Usuarios WHERE email='esoto.rh@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Aprobador'),
     (SELECT id FROM Departamentos WHERE nombre='Recursos Humanos' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Eduardo: Revisor en Contaduría (cross-depto)
    ((SELECT id FROM Usuarios WHERE email='esoto.rh@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Revisor'),
     (SELECT id FROM Departamentos WHERE nombre='Contaduría' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Valeria: Revisor base
    ((SELECT id FROM Usuarios WHERE email='vcruz.rh@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Revisor'),
     (SELECT id FROM Departamentos WHERE nombre='Recursos Humanos' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Valeria: Aprobador en Producción (cross-depto)
    ((SELECT id FROM Usuarios WHERE email='vcruz.rh@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Aprobador'),
     (SELECT id FROM Departamentos WHERE nombre='Producción' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Héctor: Elaborador base
    ((SELECT id FROM Usuarios WHERE email='halvarado.rh@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Elaborador'),
     (SELECT id FROM Departamentos WHERE nombre='Recursos Humanos' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Operario RH
    ((SELECT id FROM Usuarios WHERE email='operario.rh.demo@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Operario'),
     (SELECT id FROM Departamentos WHERE nombre='Recursos Humanos' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')))
) AS source (usuario_id, rol_id, departamento_id)
ON target.usuario_id = source.usuario_id AND target.rol_id = source.rol_id AND target.departamento_id = source.departamento_id
WHEN NOT MATCHED THEN
    INSERT (usuario_id, rol_id, departamento_id) VALUES (source.usuario_id, source.rol_id, source.departamento_id);
GO

-- -------------------------------------------------------
-- DEPTO: CONTADURÍA (Empresa Demo)
-- Beatriz   → Admin
-- Isabel    → Admin
-- Rodrigo   → Aprobador + Revisor en Calidad (cross-depto)
-- Mariana   → Revisor   + Aprobador en RH (cross-depto)
-- Óscar     → Elaborador
-- Operario  → Operario
-- -------------------------------------------------------
MERGE INTO Usuarios_Roles AS target
USING (VALUES
    ((SELECT id FROM Usuarios WHERE email='bsandoval.cont@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Administrador'),
     (SELECT id FROM Departamentos WHERE nombre='Contaduría' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    ((SELECT id FROM Usuarios WHERE email='imoreno.cont@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Administrador'),
     (SELECT id FROM Departamentos WHERE nombre='Contaduría' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Rodrigo: Aprobador base
    ((SELECT id FROM Usuarios WHERE email='rfuentes.cont@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Aprobador'),
     (SELECT id FROM Departamentos WHERE nombre='Contaduría' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Rodrigo: Revisor en Calidad (cross-depto)
    ((SELECT id FROM Usuarios WHERE email='rfuentes.cont@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Revisor'),
     (SELECT id FROM Departamentos WHERE nombre='Calidad' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Mariana: Revisor base
    ((SELECT id FROM Usuarios WHERE email='mpena.cont@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Revisor'),
     (SELECT id FROM Departamentos WHERE nombre='Contaduría' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Mariana: Aprobador en RH (cross-depto)
    ((SELECT id FROM Usuarios WHERE email='mpena.cont@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Aprobador'),
     (SELECT id FROM Departamentos WHERE nombre='Recursos Humanos' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Óscar: Elaborador base
    ((SELECT id FROM Usuarios WHERE email='odelgado.cont@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Elaborador'),
     (SELECT id FROM Departamentos WHERE nombre='Contaduría' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC'))),
    -- Operario Contaduría
    ((SELECT id FROM Usuarios WHERE email='operario.cont.demo@qualitydoc.mx'),
     (SELECT id FROM Roles WHERE nombre='Operario'),
     (SELECT id FROM Departamentos WHERE nombre='Contaduría' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')))
) AS source (usuario_id, rol_id, departamento_id)
ON target.usuario_id = source.usuario_id AND target.rol_id = source.rol_id AND target.departamento_id = source.departamento_id
WHEN NOT MATCHED THEN
    INSERT (usuario_id, rol_id, departamento_id) VALUES (source.usuario_id, source.rol_id, source.departamento_id);
GO


-- =============================================================================
-- 6. DOCUMENTOS — 4 por departamento (uno por nivel), creados por admins
-- =============================================================================

MERGE INTO Documentos AS target
USING (VALUES
    -- CALIDAD (4 niveles)
    ('CAL-01', 'Manual del Sistema de Gestión de Calidad',           1, 1, (SELECT id FROM Departamentos WHERE nombre='Calidad'         AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), (SELECT id FROM Usuarios WHERE email='rcespinoza04@gmail.com')),
    ('CAL-02', 'Procedimiento de Auditorías Internas',               2, 1, (SELECT id FROM Departamentos WHERE nombre='Calidad'         AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), (SELECT id FROM Usuarios WHERE email='lmedina.calidad@qualitydoc.mx')),
    ('CAL-03', 'Instrucción de Uso del Software de Gestión',         3, 1, (SELECT id FROM Departamentos WHERE nombre='Calidad'         AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), (SELECT id FROM Usuarios WHERE email='rcespinoza04@gmail.com')),
    ('CAL-04', 'Registro de No Conformidades',                       4, 1, (SELECT id FROM Departamentos WHERE nombre='Calidad'         AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), (SELECT id FROM Usuarios WHERE email='lmedina.calidad@qualitydoc.mx')),
    -- PRODUCCIÓN (4 niveles)
    ('PRO-01', 'Manual de Gestión de Producción',                    1, 2, (SELECT id FROM Departamentos WHERE nombre='Producción'      AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), (SELECT id FROM Usuarios WHERE email='carbarra.prod@qualitydoc.mx')),
    ('PRO-02', 'Procedimiento de Control de Calidad en Línea',       2, 2, (SELECT id FROM Departamentos WHERE nombre='Producción'      AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), (SELECT id FROM Usuarios WHERE email='sguerrero.prod@qualitydoc.mx')),
    ('PRO-03', 'Instrucción de Operación de Maquinaria CNC',         3, 2, (SELECT id FROM Departamentos WHERE nombre='Producción'      AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), (SELECT id FROM Usuarios WHERE email='carbarra.prod@qualitydoc.mx')),
    ('PRO-04', 'Registro de Inspección de Producto Terminado',       4, 1, (SELECT id FROM Departamentos WHERE nombre='Producción'      AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), (SELECT id FROM Usuarios WHERE email='sguerrero.prod@qualitydoc.mx')),
    -- RECURSOS HUMANOS (4 niveles)
    ('RH-01',  'Manual de Políticas de Recursos Humanos',            1, 1, (SELECT id FROM Departamentos WHERE nombre='Recursos Humanos' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), (SELECT id FROM Usuarios WHERE email='pvillanueva.rh@qualitydoc.mx')),
    ('RH-02',  'Procedimiento de Reclutamiento y Selección',         2, 1, (SELECT id FROM Departamentos WHERE nombre='Recursos Humanos' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), (SELECT id FROM Usuarios WHERE email='grios.rh@qualitydoc.mx')),
    ('RH-03',  'Instrucción de Evaluación de Desempeño',             3, 1, (SELECT id FROM Departamentos WHERE nombre='Recursos Humanos' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), (SELECT id FROM Usuarios WHERE email='pvillanueva.rh@qualitydoc.mx')),
    ('RH-04',  'Registro de Incidencias del Personal',               4, 1, (SELECT id FROM Departamentos WHERE nombre='Recursos Humanos' AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), (SELECT id FROM Usuarios WHERE email='grios.rh@qualitydoc.mx')),
    -- CONTADURÍA (4 niveles)
    ('CON-01', 'Manual del Sistema de Gestión Contable',             1, 1, (SELECT id FROM Departamentos WHERE nombre='Contaduría'      AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), (SELECT id FROM Usuarios WHERE email='bsandoval.cont@qualitydoc.mx')),
    ('CON-02', 'Procedimiento de Control de Gastos',                 2, 1, (SELECT id FROM Departamentos WHERE nombre='Contaduría'      AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), (SELECT id FROM Usuarios WHERE email='imoreno.cont@qualitydoc.mx')),
    ('CON-03', 'Instrucción de Conciliación Bancaria',               3, 1, (SELECT id FROM Departamentos WHERE nombre='Contaduría'      AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), (SELECT id FROM Usuarios WHERE email='bsandoval.cont@qualitydoc.mx')),
    ('CON-04', 'Registro de Auditoría Interna Contable',             4, 1, (SELECT id FROM Departamentos WHERE nombre='Contaduría'      AND compania_id=(SELECT id FROM Companias WHERE rfc='DEM010101ABC')), (SELECT id FROM Usuarios WHERE email='imoreno.cont@qualitydoc.mx'))
) AS source (codigo, nombre, nivel_id, norma_id, departamento_id, creado_por)
ON target.codigo = source.codigo
WHEN NOT MATCHED THEN
    INSERT (codigo, nombre, nivel_id, norma_id, departamento_id, creado_por)
    VALUES (source.codigo, source.nombre, source.nivel_id, source.norma_id, source.departamento_id, source.creado_por);
GO


-- =============================================================================
-- 7. SECUENCIA_FIRMA — variada por documento
-- Calidad:    CAL-01 orden 3 estándar | CAL-02 orden 4 | CAL-03 orden 3 | CAL-04 orden 6
-- Producción: PRO-01 orden 3          | PRO-02 orden 4 | PRO-03 orden 3 | PRO-04 orden 4
-- RH:         RH-01  orden 3          | RH-02  orden 4 | RH-03  orden 3 | RH-04  orden 4
-- Contaduría: CON-01 orden 3          | CON-02 orden 4 | CON-03 orden 3 | CON-04 orden 4
-- =============================================================================

-- -------------------------------------------------------
-- CAL-01 | Orden estándar 3 (Elaboró→Revisó→Aprobó)
-- Elaborador: Roberto (Elaborador en Calidad)
-- Revisor:    Ana Sofía (Revisor en Calidad)
-- Aprobador:  Jorge (Aprobador en Calidad)
-- -------------------------------------------------------
MERGE INTO Secuencia_Firma AS target
USING (VALUES
    ((SELECT id FROM Documentos WHERE codigo='CAL-01'), (SELECT id FROM Roles WHERE nombre='Elaborador'), 'Elaboró', 1),
    ((SELECT id FROM Documentos WHERE codigo='CAL-01'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  2),
    ((SELECT id FROM Documentos WHERE codigo='CAL-01'), (SELECT id FROM Roles WHERE nombre='Aprobador'),  'Aprobó',  3)
) AS source (documento_id, rol_id, tipo_firma, orden)
ON target.documento_id = source.documento_id AND target.orden = source.orden
WHEN NOT MATCHED THEN
    INSERT (documento_id, rol_id, tipo_firma, orden)
    VALUES (source.documento_id, source.rol_id, source.tipo_firma, source.orden);
GO

-- -------------------------------------------------------
-- CAL-02 | Orden 4 (Elaboró→Elaboró→Revisó→Aprobó)
-- Dos elaboradores disponibles en calidad: Roberto
-- Nota: el SP resuelve al creador en orden 1 y busca por rol en orden 2
-- -------------------------------------------------------
MERGE INTO Secuencia_Firma AS target
USING (VALUES
    ((SELECT id FROM Documentos WHERE codigo='CAL-02'), (SELECT id FROM Roles WHERE nombre='Elaborador'), 'Elaboró', 1),
    ((SELECT id FROM Documentos WHERE codigo='CAL-02'), (SELECT id FROM Roles WHERE nombre='Elaborador'), 'Elaboró', 2),
    ((SELECT id FROM Documentos WHERE codigo='CAL-02'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  3),
    ((SELECT id FROM Documentos WHERE codigo='CAL-02'), (SELECT id FROM Roles WHERE nombre='Aprobador'),  'Aprobó',  4)
) AS source (documento_id, rol_id, tipo_firma, orden)
ON target.documento_id = source.documento_id AND target.orden = source.orden
WHEN NOT MATCHED THEN
    INSERT (documento_id, rol_id, tipo_firma, orden)
    VALUES (source.documento_id, source.rol_id, source.tipo_firma, source.orden);
GO

-- -------------------------------------------------------
-- CAL-03 | Orden estándar 3
-- -------------------------------------------------------
MERGE INTO Secuencia_Firma AS target
USING (VALUES
    ((SELECT id FROM Documentos WHERE codigo='CAL-03'), (SELECT id FROM Roles WHERE nombre='Elaborador'), 'Elaboró', 1),
    ((SELECT id FROM Documentos WHERE codigo='CAL-03'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  2),
    ((SELECT id FROM Documentos WHERE codigo='CAL-03'), (SELECT id FROM Roles WHERE nombre='Aprobador'),  'Aprobó',  3)
) AS source (documento_id, rol_id, tipo_firma, orden)
ON target.documento_id = source.documento_id AND target.orden = source.orden
WHEN NOT MATCHED THEN
    INSERT (documento_id, rol_id, tipo_firma, orden)
    VALUES (source.documento_id, source.rol_id, source.tipo_firma, source.orden);
GO

-- -------------------------------------------------------
-- CAL-04 | Orden largo 6 (Elaboró→Elaboró→Revisó→Revisó→Revisor cross→Aprobó)
-- Calidad tiene: Roberto(Elaborador), Ana Sofía(Revisor base),
--               Rodrigo de Contaduría (Revisor cross en Calidad),
--               Miguel de Producción (Aprobador cross en Calidad),
--               Jorge (Aprobador base)
-- -------------------------------------------------------
MERGE INTO Secuencia_Firma AS target
USING (VALUES
    ((SELECT id FROM Documentos WHERE codigo='CAL-04'), (SELECT id FROM Roles WHERE nombre='Elaborador'), 'Elaboró', 1),
    ((SELECT id FROM Documentos WHERE codigo='CAL-04'), (SELECT id FROM Roles WHERE nombre='Elaborador'), 'Elaboró', 2),
    ((SELECT id FROM Documentos WHERE codigo='CAL-04'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  3),
    ((SELECT id FROM Documentos WHERE codigo='CAL-04'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  4),
    ((SELECT id FROM Documentos WHERE codigo='CAL-04'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  5),
    ((SELECT id FROM Documentos WHERE codigo='CAL-04'), (SELECT id FROM Roles WHERE nombre='Aprobador'),  'Aprobó',  6)
) AS source (documento_id, rol_id, tipo_firma, orden)
ON target.documento_id = source.documento_id AND target.orden = source.orden
WHEN NOT MATCHED THEN
    INSERT (documento_id, rol_id, tipo_firma, orden)
    VALUES (source.documento_id, source.rol_id, source.tipo_firma, source.orden);
GO

-- -------------------------------------------------------
-- PRO-01 | Orden estándar 3
-- -------------------------------------------------------
MERGE INTO Secuencia_Firma AS target
USING (VALUES
    ((SELECT id FROM Documentos WHERE codigo='PRO-01'), (SELECT id FROM Roles WHERE nombre='Elaborador'), 'Elaboró', 1),
    ((SELECT id FROM Documentos WHERE codigo='PRO-01'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  2),
    ((SELECT id FROM Documentos WHERE codigo='PRO-01'), (SELECT id FROM Roles WHERE nombre='Aprobador'),  'Aprobó',  3)
) AS source (documento_id, rol_id, tipo_firma, orden)
ON target.documento_id = source.documento_id AND target.orden = source.orden
WHEN NOT MATCHED THEN
    INSERT (documento_id, rol_id, tipo_firma, orden)
    VALUES (source.documento_id, source.rol_id, source.tipo_firma, source.orden);
GO

-- -------------------------------------------------------
-- PRO-02 | Orden 4 (Elaboró→Revisó→Revisó→Aprobó)
-- Producción tiene: Daniela(Elaborador), Miguel(Revisor base),
--                  Jorge de Calidad (Revisor cross en Producción),
--                  Fernanda(Aprobador base)
-- -------------------------------------------------------
MERGE INTO Secuencia_Firma AS target
USING (VALUES
    ((SELECT id FROM Documentos WHERE codigo='PRO-02'), (SELECT id FROM Roles WHERE nombre='Elaborador'), 'Elaboró', 1),
    ((SELECT id FROM Documentos WHERE codigo='PRO-02'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  2),
    ((SELECT id FROM Documentos WHERE codigo='PRO-02'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  3),
    ((SELECT id FROM Documentos WHERE codigo='PRO-02'), (SELECT id FROM Roles WHERE nombre='Aprobador'),  'Aprobó',  4)
) AS source (documento_id, rol_id, tipo_firma, orden)
ON target.documento_id = source.documento_id AND target.orden = source.orden
WHEN NOT MATCHED THEN
    INSERT (documento_id, rol_id, tipo_firma, orden)
    VALUES (source.documento_id, source.rol_id, source.tipo_firma, source.orden);
GO

-- -------------------------------------------------------
-- PRO-03 | Orden estándar 3
-- -------------------------------------------------------
MERGE INTO Secuencia_Firma AS target
USING (VALUES
    ((SELECT id FROM Documentos WHERE codigo='PRO-03'), (SELECT id FROM Roles WHERE nombre='Elaborador'), 'Elaboró', 1),
    ((SELECT id FROM Documentos WHERE codigo='PRO-03'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  2),
    ((SELECT id FROM Documentos WHERE codigo='PRO-03'), (SELECT id FROM Roles WHERE nombre='Aprobador'),  'Aprobó',  3)
) AS source (documento_id, rol_id, tipo_firma, orden)
ON target.documento_id = source.documento_id AND target.orden = source.orden
WHEN NOT MATCHED THEN
    INSERT (documento_id, rol_id, tipo_firma, orden)
    VALUES (source.documento_id, source.rol_id, source.tipo_firma, source.orden);
GO

-- -------------------------------------------------------
-- PRO-04 | Orden 4
-- -------------------------------------------------------
MERGE INTO Secuencia_Firma AS target
USING (VALUES
    ((SELECT id FROM Documentos WHERE codigo='PRO-04'), (SELECT id FROM Roles WHERE nombre='Elaborador'), 'Elaboró', 1),
    ((SELECT id FROM Documentos WHERE codigo='PRO-04'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  2),
    ((SELECT id FROM Documentos WHERE codigo='PRO-04'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  3),
    ((SELECT id FROM Documentos WHERE codigo='PRO-04'), (SELECT id FROM Roles WHERE nombre='Aprobador'),  'Aprobó',  4)
) AS source (documento_id, rol_id, tipo_firma, orden)
ON target.documento_id = source.documento_id AND target.orden = source.orden
WHEN NOT MATCHED THEN
    INSERT (documento_id, rol_id, tipo_firma, orden)
    VALUES (source.documento_id, source.rol_id, source.tipo_firma, source.orden);
GO

-- -------------------------------------------------------
-- RH-01 | Orden estándar 3
-- RH tiene: Héctor(Elaborador), Valeria(Revisor base),
--           Fernanda de Producción (Revisor cross en RH),
--           Roberto de Calidad (Revisor cross en RH),
--           Eduardo(Aprobador base)
-- -------------------------------------------------------
MERGE INTO Secuencia_Firma AS target
USING (VALUES
    ((SELECT id FROM Documentos WHERE codigo='RH-01'), (SELECT id FROM Roles WHERE nombre='Elaborador'), 'Elaboró', 1),
    ((SELECT id FROM Documentos WHERE codigo='RH-01'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  2),
    ((SELECT id FROM Documentos WHERE codigo='RH-01'), (SELECT id FROM Roles WHERE nombre='Aprobador'),  'Aprobó',  3)
) AS source (documento_id, rol_id, tipo_firma, orden)
ON target.documento_id = source.documento_id AND target.orden = source.orden
WHEN NOT MATCHED THEN
    INSERT (documento_id, rol_id, tipo_firma, orden)
    VALUES (source.documento_id, source.rol_id, source.tipo_firma, source.orden);
GO

-- -------------------------------------------------------
-- RH-02 | Orden 4 (Elaboró→Revisó→Revisó→Aprobó)
-- Activa revisores cross: Fernanda(Revisor cross en RH) y Roberto(Revisor cross en RH)
-- -------------------------------------------------------
MERGE INTO Secuencia_Firma AS target
USING (VALUES
    ((SELECT id FROM Documentos WHERE codigo='RH-02'), (SELECT id FROM Roles WHERE nombre='Elaborador'), 'Elaboró', 1),
    ((SELECT id FROM Documentos WHERE codigo='RH-02'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  2),
    ((SELECT id FROM Documentos WHERE codigo='RH-02'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  3),
    ((SELECT id FROM Documentos WHERE codigo='RH-02'), (SELECT id FROM Roles WHERE nombre='Aprobador'),  'Aprobó',  4)
) AS source (documento_id, rol_id, tipo_firma, orden)
ON target.documento_id = source.documento_id AND target.orden = source.orden
WHEN NOT MATCHED THEN
    INSERT (documento_id, rol_id, tipo_firma, orden)
    VALUES (source.documento_id, source.rol_id, source.tipo_firma, source.orden);
GO

-- -------------------------------------------------------
-- RH-03 | Orden estándar 3
-- -------------------------------------------------------
MERGE INTO Secuencia_Firma AS target
USING (VALUES
    ((SELECT id FROM Documentos WHERE codigo='RH-03'), (SELECT id FROM Roles WHERE nombre='Elaborador'), 'Elaboró', 1),
    ((SELECT id FROM Documentos WHERE codigo='RH-03'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  2),
    ((SELECT id FROM Documentos WHERE codigo='RH-03'), (SELECT id FROM Roles WHERE nombre='Aprobador'),  'Aprobó',  3)
) AS source (documento_id, rol_id, tipo_firma, orden)
ON target.documento_id = source.documento_id AND target.orden = source.orden
WHEN NOT MATCHED THEN
    INSERT (documento_id, rol_id, tipo_firma, orden)
    VALUES (source.documento_id, source.rol_id, source.tipo_firma, source.orden);
GO

-- -------------------------------------------------------
-- RH-04 | Orden 4 — activa Mariana(Aprobador cross en RH)
-- -------------------------------------------------------
MERGE INTO Secuencia_Firma AS target
USING (VALUES
    ((SELECT id FROM Documentos WHERE codigo='RH-04'), (SELECT id FROM Roles WHERE nombre='Elaborador'), 'Elaboró', 1),
    ((SELECT id FROM Documentos WHERE codigo='RH-04'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  2),
    ((SELECT id FROM Documentos WHERE codigo='RH-04'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  3),
    ((SELECT id FROM Documentos WHERE codigo='RH-04'), (SELECT id FROM Roles WHERE nombre='Aprobador'),  'Aprobó',  4)
) AS source (documento_id, rol_id, tipo_firma, orden)
ON target.documento_id = source.documento_id AND target.orden = source.orden
WHEN NOT MATCHED THEN
    INSERT (documento_id, rol_id, tipo_firma, orden)
    VALUES (source.documento_id, source.rol_id, source.tipo_firma, source.orden);
GO

-- -------------------------------------------------------
-- CON-01 | Orden estándar 3
-- Contaduría tiene: Óscar(Elaborador), Mariana(Revisor base),
--                  Eduardo de RH (Revisor cross en Contaduría),
--                  Ana Sofía de Calidad (Aprobador cross en Contaduría),
--                  Rodrigo(Aprobador base)
-- -------------------------------------------------------
MERGE INTO Secuencia_Firma AS target
USING (VALUES
    ((SELECT id FROM Documentos WHERE codigo='CON-01'), (SELECT id FROM Roles WHERE nombre='Elaborador'), 'Elaboró', 1),
    ((SELECT id FROM Documentos WHERE codigo='CON-01'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  2),
    ((SELECT id FROM Documentos WHERE codigo='CON-01'), (SELECT id FROM Roles WHERE nombre='Aprobador'),  'Aprobó',  3)
) AS source (documento_id, rol_id, tipo_firma, orden)
ON target.documento_id = source.documento_id AND target.orden = source.orden
WHEN NOT MATCHED THEN
    INSERT (documento_id, rol_id, tipo_firma, orden)
    VALUES (source.documento_id, source.rol_id, source.tipo_firma, source.orden);
GO

-- -------------------------------------------------------
-- CON-02 | Orden 4 (Elaboró→Revisó→Revisó→Aprobó)
-- Activa Eduardo(Revisor cross en Contaduría) y Mariana(Revisor base)
-- -------------------------------------------------------
MERGE INTO Secuencia_Firma AS target
USING (VALUES
    ((SELECT id FROM Documentos WHERE codigo='CON-02'), (SELECT id FROM Roles WHERE nombre='Elaborador'), 'Elaboró', 1),
    ((SELECT id FROM Documentos WHERE codigo='CON-02'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  2),
    ((SELECT id FROM Documentos WHERE codigo='CON-02'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  3),
    ((SELECT id FROM Documentos WHERE codigo='CON-02'), (SELECT id FROM Roles WHERE nombre='Aprobador'),  'Aprobó',  4)
) AS source (documento_id, rol_id, tipo_firma, orden)
ON target.documento_id = source.documento_id AND target.orden = source.orden
WHEN NOT MATCHED THEN
    INSERT (documento_id, rol_id, tipo_firma, orden)
    VALUES (source.documento_id, source.rol_id, source.tipo_firma, source.orden);
GO

-- -------------------------------------------------------
-- CON-03 | Orden estándar 3
-- -------------------------------------------------------
MERGE INTO Secuencia_Firma AS target
USING (VALUES
    ((SELECT id FROM Documentos WHERE codigo='CON-03'), (SELECT id FROM Roles WHERE nombre='Elaborador'), 'Elaboró', 1),
    ((SELECT id FROM Documentos WHERE codigo='CON-03'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  2),
    ((SELECT id FROM Documentos WHERE codigo='CON-03'), (SELECT id FROM Roles WHERE nombre='Aprobador'),  'Aprobó',  3)
) AS source (documento_id, rol_id, tipo_firma, orden)
ON target.documento_id = source.documento_id AND target.orden = source.orden
WHEN NOT MATCHED THEN
    INSERT (documento_id, rol_id, tipo_firma, orden)
    VALUES (source.documento_id, source.rol_id, source.tipo_firma, source.orden);
GO

-- -------------------------------------------------------
-- CON-04 | Orden 4 — activa Ana Sofía (Aprobador cross en Contaduría)
-- -------------------------------------------------------
MERGE INTO Secuencia_Firma AS target
USING (VALUES
    ((SELECT id FROM Documentos WHERE codigo='CON-04'), (SELECT id FROM Roles WHERE nombre='Elaborador'), 'Elaboró', 1),
    ((SELECT id FROM Documentos WHERE codigo='CON-04'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  2),
    ((SELECT id FROM Documentos WHERE codigo='CON-04'), (SELECT id FROM Roles WHERE nombre='Revisor'),    'Revisó',  3),
    ((SELECT id FROM Documentos WHERE codigo='CON-04'), (SELECT id FROM Roles WHERE nombre='Aprobador'),  'Aprobó',  4)
) AS source (documento_id, rol_id, tipo_firma, orden)
ON target.documento_id = source.documento_id AND target.orden = source.orden
WHEN NOT MATCHED THEN
    INSERT (documento_id, rol_id, tipo_firma, orden)
    VALUES (source.documento_id, source.rol_id, source.tipo_firma, source.orden);
GO


-- =============================================================================
-- VERIFICACIÓN FINAL
-- =============================================================================
SELECT
    d.codigo,
    dp.nombre                   AS departamento,
    nd.nombre                   AS nivel,
    sf.orden,
    sf.tipo_firma,
    r.nombre                    AS rol_requerido
FROM Documentos d
INNER JOIN Departamentos     dp ON dp.id = d.departamento_id
INNER JOIN Niveles_Documento nd ON nd.id = d.nivel_id
INNER JOIN Secuencia_Firma   sf ON sf.documento_id = d.id
INNER JOIN Roles              r ON r.id = sf.rol_id
WHERE dp.compania_id = (SELECT id FROM Companias WHERE rfc = 'DEM010101ABC')
ORDER BY d.codigo, sf.orden;
GO