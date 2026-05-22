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

    public function index()
    {
        $departamento = $this->resolveDepartamento();
        $fastapi      = $this->getFastApiUrl();

        $response = Http::timeout(5)->get("{$fastapi}/search/documentos", [
            'departamento' => $departamento,
        ]);

        $documentos = $response->successful() ? $response->json('results') : [];

        // Agrupar por nivel para estructura virtual de carpetas
        $porNivel = collect($documentos)->groupBy(fn($d) => $d['metadata']['nivel'] ?? 'Sin clasificar');

        return view('operativo.dashboard', [
            'porNivel'     => $porNivel,
            'departamento' => $departamento,
            'userName'     => Session::get('user_name'),
        ]);
    }

    public function preview(Request $request, string $storage_path)
    {
        $minio  = $this->getMinioClient();
        $bucket = env('MINIO_BUCKET', 'qualitydoc');

        $object   = $minio->getObject([
            'Bucket' => $bucket,
            'Key'    => $storage_path,
        ]);

        $body        = $object['Body']->getContents();
        $contentType = $object['ContentType'] ?? 'application/octet-stream';
        $filename    = basename($storage_path);

        // Log de auditoría en PostgreSQL
        $this->logAuditoria($request, $storage_path, 'Lectura');

        return response($body, 200)
            ->header('Content-Type', $contentType)
            ->header('Content-Disposition', "inline; filename=\"{$filename}\"");
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

