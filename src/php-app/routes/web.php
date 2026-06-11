<?php

use Illuminate\Support\Facades\Route;
use App\Http\Controllers\SsoController;
use App\Http\Controllers\OperativoController;
use App\Http\Controllers\AuditoriaController;

Route::get('/', function () {
    if (session('is_authenticated')) {
        return redirect('/operativo/dashboard');
    }
    return redirect()->away('/Account/Login')
        ->with('error', 'Debes iniciar sesión para acceder al sistema.');
});

Route::get('/hub', [SsoController::class, 'handleHub'])->name('sso.hub');

Route::middleware('auth.custom')->group(function () {
    Route::get('/operativo/dashboard', [OperativoController::class, 'index']);
    Route::get('/operativo/buscar', [OperativoController::class, 'buscar']);
  
    Route::get('/operativo/versiones/{doc_id}',          [OperativoController::class, 'versiones']);
    Route::get('/operativo/preview/{storage_path}',      [OperativoController::class, 'preview'])
    ->where('storage_path', '.*');
    Route::get('/operativo/descargar/{storage_path}',    [OperativoController::class, 'descargar'])
    ->where('storage_path', '.*');    
});

Route::post('/operativo/logout', function () {
    Session::flush();
    return redirect()->away('/Account/Login');
})->name('operativo.logout');


Route::get('/operativo/buscar', [OperativoController::class, 'buscar']);

Route::get('/auditoria', [AuditoriaController::class, 'index']);

Route::get('/operativo/versiones/{doc_id}', [OperativoController::class, 'versiones']);