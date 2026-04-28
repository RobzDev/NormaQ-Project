<?php

namespace App\Http\Middleware;

use Closure;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Session;
use Symfony\Component\HttpFoundation\Response;

class AuthCustom
{
    public function handle(Request $request, Closure $next): Response
    {
        // Verificamos si la sesión tiene nuestra marca de autenticación del SSO
        if (!Session::has('is_authenticated') || !Session::get('is_authenticated')) {
            // Si no está autenticado, lo mandamos al login central de C#
            // Ajusta el puerto según tu configuración de Kestrel
            return redirect()->away('http://localhost:80/Account/Login');
        }

        return $next($request);
    }
}