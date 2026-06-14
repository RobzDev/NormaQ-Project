<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;
use Illuminate\Support\Facades\Http;
use Illuminate\Support\Facades\Session;
use Aws\S3\S3Client;

class OperativoController extends Controller
{
    private function resolveDepartamento(): string
    {
        $departamento = Session::get('user_dept');

        if (is_array($departamento)) {
            return (string) ($departamento['Nombre'] ?? $departamento['nombre'] ?? $departamento['departamento'] ?? reset($departamento) ?? '');
        }

        return (string) $departamento;
    }

    private function getFastApiUrl(): string
    {
        return config('services.fastapi.url', 'http://fastapi-service:8000');
    }

    private function getMinioClient(): S3Client
    {
        return new S3Client([
            'version'                 => 'latest',
            'region'                  => env('MINIO_REGION', 'us-east-1'),
            'endpoint'                => env('MINIO_ENDPOINT'),
            'use_path_style_endpoint' => true,
            'credentials'             => [
                'key'    => env('MINIO_ACCESS_KEY'),
                'secret' => env('MINIO_SECRET_KEY'),
            ],
        ]);
    }

 public function index(Request $request)
{
    $departamento = Session::get('user_dept');
    $fastapi      = $this->getFastApiUrl();
    $perPage      = 8;

    $response = Http::timeout(5)->get("{$fastapi}/search/documentos", [
        'departamento' => $departamento,
    ]);

    $documentos = $response->successful() ? $response->json('results') : [];

    // Agrupar por nivel
    $porNivelRaw = collect($documentos)->groupBy(fn($d) => $d['metadata']['nivel'] ?? 'Sin clasificar');

    // Paginar cada nivel independientemente
   // Cambiar esto en tu controlador:
$porNivel = $porNivelRaw->map(function ($docs, $nivel) use ($request, $perPage) {
    $slug     = str_replace(' ', '_', $nivel);
    // Usamos el slug directamente para capturar el parámetro de la URL (?page_procedimientos=2)
    $page     = (int) $request->query("page_{$slug}", 1);
    $total    = $docs->count();
    $items    = $docs->slice(($page - 1) * $perPage, $perPage)->values();
    $lastPage = (int) ceil($total / $perPage);

    return [
        'items'    => $items,
        'total'    => $total,
        'page'     => $page,
        'lastPage' => $lastPage,
        'slug'     => $slug,
    ];
});
    return view('operativo.dashboard', [
        'porNivel'     => $porNivel,
        'departamento' => $departamento,
        'userName'     => Session::get('user_name'),
    ]);
}

public function versiones(Request $request, string $doc_id)
{
    $fastapi  = $this->getFastApiUrl();
    $response = Http::timeout(5)->get("{$fastapi}/search/versiones/{$doc_id}");
    $versiones = $response->successful() ? $response->json('results') : [];

    $aprobada  = collect($versiones)->firstWhere('estado', 'aprobado');
    $obsoletas = collect($versiones)->where('estado', 'obsoleto')->values();

    // Paginación de obsoletas
    $perPage   = 5;
    $page      = (int) $request->query('page', 1);
    $total     = $obsoletas->count();
    $lastPage  = max(1, (int) ceil($total / $perPage));
    $obsoletasPaginadas = $obsoletas->slice(($page - 1) * $perPage, $perPage)->values();

    return view('operativo.versiones', [
        'doc_id'     => $doc_id,
        'aprobada'   => $aprobada,
        'obsoletas'  => $obsoletasPaginadas,
        'page'       => $page,
        'lastPage'   => $lastPage,
        'userName'   => Session::get('user_name'),
        'departamento' => Session::get('user_dept'),
    ]);
}

 public function preview(Request $request, string $storage_path)
{
    $minio    = $this->getMinioClient();
    $bucket   = env('MINIO_BUCKET', 'qualitydoc');
    $convert  = $request->query('convert') === 'pdf';
    $ext      = strtolower(pathinfo($storage_path, PATHINFO_EXTENSION));

    $object      = $minio->getObject(['Bucket' => $bucket, 'Key' => $storage_path]);
    $body        = $object['Body']->getContents();
    $contentType = $object['ContentType'] ?? 'application/octet-stream';
    $filename    = basename($storage_path);

    $this->logAuditoria($request, $storage_path, 'Lectura');

    // Conversión DOCX → PDF con LibreOffice
   if ($convert && $ext === 'docx') {
    
    $baseTmp = tempnam(sys_get_temp_dir(), 'doc_'); 
    @unlink($baseTmp); 
    
    $tmpDocx = $baseTmp . '.docx';
    $tmpDir  = sys_get_temp_dir() . '/lo_' . uniqid();
    $profileDir = sys_get_temp_dir() . '/lop_' . uniqid();

    if (!is_dir($tmpDir)) { mkdir($tmpDir, 0777, true); }
    if (!is_dir($profileDir)) { mkdir($profileDir, 0777, true); }

    file_put_contents($tmpDocx, $body);

    $escapedOutdir  = escapeshellarg($tmpDir);
    $escapedDocx    = escapeshellarg($tmpDocx);
    $envProfilePath = "file://" . $profileDir;

    // Ejecutamos con el parámetro de entorno que desbloqueó el Fatall Error
    $command = "FONTCONFIG_PATH=/tmp HOME=/tmp libreoffice --headless \"-env:UserInstallation={$envProfilePath}\" --convert-to pdf --outdir {$escapedOutdir} {$escapedDocx} 2>&1";    
    exec($command, $output, $code);

    $pdfPath = $tmpDir . '/' . pathinfo($tmpDocx, PATHINFO_FILENAME) . '.pdf';

    // Tu prueba de Docker demostró que $code es 0 y el archivo sí se crea físicamente
    if ($code === 0 && file_exists($pdfPath)) {
        $pdfContent = file_get_contents($pdfPath);

        // Limpieza de archivos temporales exitosos
        @unlink($tmpDocx);
        @unlink($pdfPath);
        @rmdir($tmpDir);
        @array_map('unlink', glob("{$profileDir}/*") ?: []);
        @rmdir($profileDir);

        return response($pdfContent, 200)
            ->header('Content-Type', 'application/pdf')
            ->header('Content-Disposition', 'inline; filename="' . pathinfo($filename, PATHINFO_FILENAME) . '.pdf"');
    }

    // Si entra aquí, algo diferente falló (ej. disco lleno), dejamos registro en el log
    \Log::error("Fallo crítico conversión. Código: {$code}. Salida completa: " . implode("\n", $output));

    @unlink($tmpDocx);
    if (is_dir($tmpDir)) { @array_map('unlink', glob("{$tmpDir}/*")); @rmdir($tmpDir); }
    if (is_dir($profileDir)) { @array_map('unlink', glob("{$profileDir}/*")); @rmdir($profileDir); }

    return response('No se pudo convertir el documento.', 500)
        ->header('Content-Type', 'text/plain');
}
    // Retorno por defecto para archivos que no requieren conversión (.pdf, .txt)
    return response($body, 200)
        ->header('Content-Type', $contentType)
        ->header('Content-Disposition', 'inline; filename="' . $filename . '"');
}

public function descargar(Request $request, string $storage_path)
{
    $minio    = $this->getMinioClient();
    $bucket   = env('MINIO_BUCKET', 'qualitydoc');
    $filename = basename($storage_path);

    $object      = $minio->getObject(['Bucket' => $bucket, 'Key' => $storage_path]);
    $body        = $object['Body']->getContents();
    $contentType = $object['ContentType'] ?? 'application/octet-stream';

    $this->logAuditoria($request, $storage_path, 'Descarga');

    return response($body, 200)
        ->header('Content-Type', $contentType)
        ->header('Content-Disposition', 'attachment; filename="' . $filename . '"');
}

    private function logAuditoria(Request $request, string $storage_path, string $accion): void
    {
        try {
            \DB::insert("INSERT INTO logs_auditoria 
                (usuario_id, usuario_nombre, departamento_nombre, documento_codigo, documento_nombre, version_documento, accion, ip_origen)
                VALUES (?, ?, ?, ?, ?, ?, ?, ?)", [
                Session::get('user_id'),
                Session::get('user_name'),
                $this->resolveDepartamento(),
                basename($storage_path),
                basename($storage_path),
                '1.0',
                $accion,
                $request->ip(),
            ]);
        } catch (\Exception $e) {
            \Log::error("Error log auditoría: {$e->getMessage()}");
        }
    }



public function buscar(Request $request)
{
    $q            = $request->query('q');
    $departamento = $this->resolveDepartamento();
    $fastapi      = $this->getFastApiUrl();

    $response = Http::timeout(5)->get("{$fastapi}/search/autocomplete", [
        'q'            => $q,
        'departamento' => $departamento,
    ]);

    return response()->json(
        $response->successful() ? $response->json() : ['results' => []]
    );
}
}

