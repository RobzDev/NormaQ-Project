-- =============================================================================
-- QualityDoc-Polyglot | SQL Server
-- trg_MutarEstadoVersion
-- AFTER UPDATE sobre Flujos_Aprobacion
-- Muta el estado de Versiones_Documento según el tipo_firma aprobado
-- =============================================================================

CREATE OR ALTER TRIGGER trg_MutarEstadoVersion
ON Flujos_Aprobacion
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Salir inmediatamente si la actualización no involucra estado_firma
    IF NOT UPDATE(estado_firma) RETURN;

    -- -------------------------------------------------------------------------
    -- Solo nos interesa la fila que acaba de cambiar a 'Aprobado'
    -- Las filas canceladas por C# no entran aquí
    -- -------------------------------------------------------------------------
    DECLARE
        @version_id     INT,
        @tipo_firma     VARCHAR(20),
        @orden          TINYINT,
        @documento_id   INT;

    SELECT
        @version_id  = i.version_id,
        @tipo_firma  = i.tipo_firma,
        @orden       = i.orden
    FROM INSERTED i
    WHERE i.estado_firma = 'Aprobado';

    -- Si ninguna fila del batch cambió a 'Aprobado', no hay nada que hacer
    IF @version_id IS NULL RETURN;

    -- Resolver documento_id desde la versión
    SELECT @documento_id = documento_id
    FROM Versiones_Documento
    WHERE id = @version_id;

    -- -------------------------------------------------------------------------
    -- REGLA 1: tipo_firma = 'Elaboró'
    -- Borrador → Revision
    -- -------------------------------------------------------------------------
    IF @tipo_firma = 'Elaboró'
    BEGIN
        UPDATE Versiones_Documento
        SET estado = 'Revision'
        WHERE id     = @version_id
          AND estado = 'Borrador';

        RETURN;
    END

    -- -------------------------------------------------------------------------
    -- REGLA 2: tipo_firma = 'Revisó'
    -- No muta el estado de la versión, solo avanza el flujo.
    -- El estado 'Revision' ya fue asignado cuando el Elaborador firmó.
    -- -------------------------------------------------------------------------
    IF @tipo_firma = 'Revisó'
    BEGIN
        RETURN;
    END

    -- -------------------------------------------------------------------------
    -- REGLA 3: tipo_firma = 'Aprobó'
    -- Verificar si es el último paso del flujo
    -- -------------------------------------------------------------------------
    IF @tipo_firma = 'Aprobó'
    BEGIN
        -- ¿Existe algún orden mayor aún pendiente o aprobado?
        IF EXISTS (
            SELECT 1
            FROM Flujos_Aprobacion
            WHERE version_id    = @version_id
              AND orden         > @orden
              AND estado_firma  IN ('Pendiente', 'Aprobado')
        )
        BEGIN
            -- No es el último paso, no hacer nada
            RETURN;
        END

        -- Es el último paso: primero marcar la versión anterior como Obsoleto
        -- para no colisionar con el índice UX_UnAprobadoPorDocumento
        UPDATE Versiones_Documento
        SET
            estado               = 'Obsoleto',
            fecha_obsolescencia  = GETDATE()
        WHERE documento_id  = @documento_id
          AND estado         = 'Aprobado';

        -- Ahora sí promover la versión actual a Aprobado
        UPDATE Versiones_Documento
        SET
            estado            = 'Aprobado',
            fecha_aprobacion  = GETDATE()
        WHERE id     = @version_id
          AND estado = 'Revision';

        RETURN;
    END

END
GO