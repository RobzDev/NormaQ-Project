<?php

use Illuminate\Support\Facades\Route;
use App\Http\Controllers\SsoController;

Route::get('/', function () {
    return view('bienvenida');
});

Route::get('/hub', [SsoController::class, 'handleHub'])->name('sso.hub');

// Ruta protegida por la sesión de Laravel
Route::get('/operativo/dashboard', function () {
    return view('operativo.dashboard');
})->middleware('auth.custom'); // Crearemos este middleware simple luego