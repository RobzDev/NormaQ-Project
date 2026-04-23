<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
   public function up(): void
{
    Schema::create('logs_auditoria', function (Blueprint $table) {
        // id BIGSERIAL PRIMARY KEY
        $table->id(); 

        // fecha_hora TIMESTAMPTZ NOT NULL DEFAULT NOW()
        $table->timestampTz('fecha_hora')->useCurrent();

        // Columnas de Usuario
        $table->integer('usuario_id');
        $table->string('usuario_nombre', 150);
        $table->string('departamento_nombre', 100);

        // Columnas de Documento
        $table->string('documento_codigo', 30);
        $table->string('documento_nombre', 255);
        $table->string('version_documento', 10);

        // Acción e Infraestructura
        $table->string('accion', 20);
        $table->string('ip_origen', 45)->nullable(); // Soporta IPv6
        $table->string('modulo_origen', 30)->default('portal_php');

        // =============================================================================
        // Índices (Optimización para ISO 9001)
        // =============================================================================
        
        $table->index(['usuario_id', 'fecha_hora'], 'ix_logs_usuario');
        $table->index(['documento_codigo', 'fecha_hora'], 'ix_logs_documento');
        $table->index(['fecha_hora'], 'ix_logs_fecha');
        $table->index(['departamento_nombre', 'fecha_hora'], 'ix_logs_departamento');
    });

    // Para el CONSTRAINT CHECK, usamos una consulta RAW ya que Laravel no tiene un método nativo 'check'
    DB::statement("ALTER TABLE logs_auditoria ADD CONSTRAINT chk_accion CHECK (accion IN ('Lectura', 'Descarga'))");
}

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('logs_auditoria');
    }
};
