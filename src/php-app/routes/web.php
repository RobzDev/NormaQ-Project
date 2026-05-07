<?php

use Illuminate\Support\Facades\Route;
use App\Http\Controllers\SsoController;

Route::get('/', function () {
    return view('bienvenida');
});

Route::get('/registro', function () {
    return view('registro');
});

Route::get('/hub', [SsoController::class, 'handleHub'])->name('sso.hub');

Route::get('/operativo/dashboard', function () {
    return view('operativo.dashboard');
})->middleware('auth.custom');