<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>NormaQ - Portal Operativo</title>
    <script src="https://cdn.tailwindcss.com"></script>
</head>
<body class="bg-slate-900 text-slate-200 font-sans">

    <nav class="bg-slate-800 border-b border-slate-700 p-4 shadow-lg">
        <div class="container mx-auto flex justify-between items-center">
            <h1 class="text-xl font-bold text-indigo-400">NormaQ <span class="text-slate-400 text-sm font-normal">| Operativo</span></h1>
            <div class="text-sm">
                Sesión iniciada como: <span class="text-white font-semibold">{{ session('user_name') }}</span>
            </div>
        </div>
    </nav>

    <main class="container mx-auto py-10 px-4">
        <div class="bg-emerald-900/30 border border-emerald-500/50 p-4 rounded-lg mb-8 flex items-center gap-4">
            <div class="bg-emerald-500 p-2 rounded-full text-white">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                </svg>
            </div>
            <div>
                <h2 class="text-emerald-400 font-bold">SSO Validado Exitosamente</h2>
                <p class="text-emerald-200/70 text-sm">Los claims han sido transferidos de C# a Laravel mediante el broker de Redis.</p>
            </div>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
            <div class="bg-slate-800 rounded-xl p-6 border border-slate-700 shadow-xl">
                <h3 class="text-indigo-400 font-semibold uppercase tracking-wider text-xs mb-4">Perfil del Operario</h3>
                <div class="space-y-4">
                    <div>
                        <label class="block text-slate-500 text-xs uppercase">ID de Usuario (C# Sync)</label>
                        <p class="text-lg font-mono">{{ session('user_id') }}</p>
                    </div>
                    <div>
                        <label class="block text-slate-500 text-xs uppercase">Email Institucional</label>
                        <p class="text-lg">{{ session('user_email') }}</p>
                    </div>
                </div>
            </div>

            <div class="bg-slate-800 rounded-xl p-6 border border-slate-700 shadow-xl">
                <h3 class="text-indigo-400 font-semibold uppercase tracking-wider text-xs mb-4">Malla de Departamentos Autorizados</h3>
                <p class="text-slate-400 text-sm mb-4">Tienes permisos de lectura/operación en los siguientes IDs:</p>
                
                <div class="flex flex-wrap gap-2">
                    @foreach(session('user_depts', []) as $deptId)
                        <span class="px-3 py-1 bg-indigo-600/20 border border-indigo-500/50 text-indigo-300 rounded-md font-mono">
                            Depto #{{ $deptId }}
                        </span>
                    @endforeach
                </div>
            </div>
        </div>

        <div class="mt-12 p-6 bg-slate-800/50 border border-dashed border-slate-700 rounded-lg text-center">
            <p class="text-slate-500 text-sm">
                Arquitectura Políglota: Laravel actúa como servidor de interfaz para operarios consumiendo la lógica de autenticación de .NET 10.
            </p>
        </div>
    </main>

</body>
</html>