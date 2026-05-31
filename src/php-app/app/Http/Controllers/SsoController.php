<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;
use Illuminate\Support\Facades\Http;
use Illuminate\Support\Facades\Session;

class SsoController extends Controller
{
    public function handleHub(Request $request)
    {
        $token = $request->query('token');
        $validateUrl = config('services.normaq_sso.validate_url', 'http://dotnet-service/api/sso/validate');

        if (!$token) {
            return redirect()->away('/Account/Login');
        }

        // 1. Ejecución: Llamada backend-to-backend al endpoint de C#
        // En Docker, el backend .NET se resuelve por nombre de servicio.
        $response = Http::timeout(5)->get($validateUrl, [
            'token' => $token,
        ]);

        // 2. Validación: Si C# dice que el token es inválido o expiró
        if ($response->failed()) {
            return response()->json(['error' => 'Token SSO inválido o expirado'], 401);
        }

        // 3. Procesamiento: C# nos devuelve el JSON con los claims
        $userData = $response->json();

        // 4. Persistencia: Guardamos los datos en la sesión de Laravel
        Session::put('user_id', $userData['UsuarioId']);
        Session::put('user_name', $userData['Nombre']);
        Session::put('user_email', $userData['Email']);
        Session::put('user_dept', data_get($userData, 'DepartamentoOperario.Nombre', data_get($userData, 'DepartamentoOperario', '')));
        Session::put('is_authenticated', true);

        // 5. Redirección al Dashboard Operativo
        return redirect('/operativo/dashboard');
    }
}