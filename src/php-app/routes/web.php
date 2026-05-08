<?php

use Illuminate\Support\Facades\Route;
use App\Http\Controllers\SsoController;

Route::get('/', function () {
    return view('bienvenida');
});

Route::get('/hub', [SsoController::class, 'handleHub'])->name('sso.hub');

// 1. RUTA DE PRUEBA (Usa esta para ver tus diseños sin tener que iniciar sesión)
Route::get('/diseno', function () {
    return view('operativo.dashboard'); 
});

// 2. RUTA OFICIAL PROTEGIDA (La que usará tu equipo con el middleware)
Route::get('/operativo/dashboard', function () {
    return view('operativo.dashboard');
})->middleware('auth.custom');