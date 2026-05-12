
-- =============================================================================
-- Stored Procedure
-- =============================================================================
 
CREATE OR ALTER PROCEDURE SP_Generar_Flujos_Aprobacion
    @version_id INT
AS
BEGIN
    SET NOCOUNT ON;
 
    -- -------------------------------------------------------------------------
    -- Variables de trabajo
    -- -------------------------------------------------------------------------
    DECLARE
        @documento_id       INT,
        @creado_por         INT,
        @departamento_id    INT,
        @error_msg          NVARCHAR(500),
 
        -- Variables del cursor
        @cur_orden          TINYINT,
        @cur_rol_id         INT,
        @cur_tipo_firma     VARCHAR(20),
 
        -- Para resolver usuarios por rol+depto
        @usuario_resuelto   INT,
        @usuarios_encontrados INT;
 
    -- -------------------------------------------------------------------------
    -- BLOQUE PRINCIPAL
    -- -------------------------------------------------------------------------
    BEGIN TRY
        BEGIN TRANSACTION;
 
        -- ---------------------------------------------------------------------
        -- VALIDACIÓN 1: ¿Existe la versión?
        -- ---------------------------------------------------------------------
        IF NOT EXISTS (
            SELECT 1 FROM Versiones_Documento WHERE id = @version_id
        )
        BEGIN
            SET @error_msg = 'La versión con ID ' + CAST(@version_id AS VARCHAR) + ' no existe.';
            RAISERROR(@error_msg, 16, 1);
            RETURN;
        END
 
        -- ---------------------------------------------------------------------
        -- VALIDACIÓN 2: ¿Estado = 'Borrador'?
        -- ---------------------------------------------------------------------
        IF NOT EXISTS (
            SELECT 1 FROM Versiones_Documento
            WHERE id = @version_id AND estado = 'Borrador'
        )
        BEGIN
            SET @error_msg = 'La versión ' + CAST(@version_id AS VARCHAR) + ' no está en estado Borrador. No se pueden generar flujos.';
            RAISERROR(@error_msg, 16, 1);
            RETURN;
        END
 
        -- ---------------------------------------------------------------------
        -- VALIDACIÓN 3: ¿Ya tiene flujos generados?
        -- ---------------------------------------------------------------------
        IF EXISTS (
            SELECT 1 FROM Flujos_Aprobacion WHERE version_id = @version_id
        )
        BEGIN
            SET @error_msg = 'La versión ' + CAST(@version_id AS VARCHAR) + ' ya tiene flujos de aprobación generados.';
            RAISERROR(@error_msg, 16, 1);
            RETURN;
        END
 
        -- ---------------------------------------------------------------------
        -- RESOLVER datos base desde Versiones_Documento → Documentos
        -- ---------------------------------------------------------------------
        SELECT
            @documento_id    = vd.documento_id,
            @creado_por      = vd.creado_por,
            @departamento_id = d.departamento_id
        FROM Versiones_Documento vd
        INNER JOIN Documentos d ON d.id = vd.documento_id
        WHERE vd.id = @version_id;
 
        -- ---------------------------------------------------------------------
        -- VALIDACIÓN 4: ¿Existe plantilla en Secuencia_Firma?
        -- ---------------------------------------------------------------------
        IF NOT EXISTS (
            SELECT 1 FROM Secuencia_Firma WHERE documento_id = @documento_id
        )
        BEGIN
            SET @error_msg = 'El documento ID ' + CAST(@documento_id AS VARCHAR) + ' no tiene una plantilla de firmas definida en Secuencia_Firma.';
            RAISERROR(@error_msg, 16, 1);
            RETURN;
        END
 
        -- ---------------------------------------------------------------------
        -- CURSOR: recorre la plantilla ordenada por orden ASC
        -- ---------------------------------------------------------------------
        DECLARE cur_secuencia CURSOR LOCAL FAST_FORWARD FOR
            SELECT orden, rol_id, tipo_firma
            FROM Secuencia_Firma
            WHERE documento_id = @documento_id
            ORDER BY orden ASC;
 
        OPEN cur_secuencia;
        FETCH NEXT FROM cur_secuencia INTO @cur_orden, @cur_rol_id, @cur_tipo_firma;
 
        WHILE @@FETCH_STATUS = 0
        BEGIN
 
            -- -----------------------------------------------------------------
            -- ORDEN 1: siempre es el creador de la versión (Elaborador)
            -- -----------------------------------------------------------------
            IF @cur_orden = 1
            BEGIN
                INSERT INTO Flujos_Aprobacion
                    (version_id, usuario_id, tipo_firma, orden, estado_firma)
                VALUES
                    (@version_id, @creado_por, @cur_tipo_firma, @cur_orden, 'Pendiente');
            END
 
            -- -----------------------------------------------------------------
            -- ORDEN > 1: resolver todos los usuarios con ese rol en ese depto
            -- -----------------------------------------------------------------
            ELSE
            BEGIN
                -- Contar cuántos usuarios cumplen el rol en el departamento
                SELECT @usuarios_encontrados = COUNT(*)
                FROM Usuarios_Roles ur
                INNER JOIN Usuarios u ON u.id = ur.usuario_id
                WHERE ur.rol_id          = @cur_rol_id
                  AND ur.departamento_id = @departamento_id
                  AND u.activo           = 1;
 
                -- Si ningún usuario cumple el rol, el SP falla con rollback
                IF @usuarios_encontrados = 0
                BEGIN
                    SET @error_msg =
                        'No se encontró ningún usuario activo con el rol ID ' +
                        CAST(@cur_rol_id AS VARCHAR) +
                        ' en el departamento ID ' +
                        CAST(@departamento_id AS VARCHAR) +
                        ' para el orden ' + CAST(@cur_orden AS VARCHAR) + '.';
                    RAISERROR(@error_msg, 16, 1);
                END
 
                -- Insertar una fila por cada usuario que cumpla rol+depto
                -- Patrón "el primero que atiende se lo queda":
                -- C# aprobará una fila y cancelará las demás del mismo orden
                INSERT INTO Flujos_Aprobacion
                    (version_id, usuario_id, tipo_firma, orden, estado_firma)
                SELECT
                    @version_id,
                    ur.usuario_id,
                    @cur_tipo_firma,
                    @cur_orden,
                    'Pendiente'
                FROM Usuarios_Roles ur
                INNER JOIN Usuarios u ON u.id = ur.usuario_id
                WHERE ur.rol_id          = @cur_rol_id
                  AND ur.departamento_id = @departamento_id
                  AND u.activo           = 1
                ORDER BY ur.usuario_id ASC;   -- orden determinista, no afecta la lógica
            END
 
            FETCH NEXT FROM cur_secuencia INTO @cur_orden, @cur_rol_id, @cur_tipo_firma;
        END
 
        CLOSE cur_secuencia;
        DEALLOCATE cur_secuencia;
 
        -- Todo bien, confirmamos
        COMMIT TRANSACTION;
 
    END TRY
    BEGIN CATCH
 
        -- Si el cursor quedó abierto por un error, lo cerramos limpiamente
        IF CURSOR_STATUS('local', 'cur_secuencia') >= 0
        BEGIN
            CLOSE cur_secuencia;
            DEALLOCATE cur_secuencia;
        END
 
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
 
        -- Re-lanzamos el error hacia el backend (.NET)
        DECLARE @catch_msg   NVARCHAR(500) = ERROR_MESSAGE();
        DECLARE @catch_sev   INT           = ERROR_SEVERITY();
        DECLARE @catch_state INT           = ERROR_STATE();
        RAISERROR(@catch_msg, @catch_sev, @catch_state);
 
    END CATCH
END
GO