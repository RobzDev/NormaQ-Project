<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;
use Illuminate\Support\Facades\DB;

class AuditoriaController extends Controller
{
    public function index(Request $request)
    {
        $departamento = $request->query('departamento');

        if (!$departamento) {
            return response()->json(['error' => 'Departamento requerido'], 400);
        }

        $logs = DB::select("
            SELECT 
                id,
                fecha_hora,
                usuario_id,
                usuario_nombre,
                documento_codigo,
                documento_nombre,
                version_documento,
                accion,
                ip_origen
            FROM logs_auditoria
            WHERE departamento_nombre = ?
            ORDER BY fecha_hora DESC
        ", [$departamento]);

        return response()->json(['results' => $logs]);
    }
}