<!DOCTYPE html>
<html lang="es" class="scroll-smooth antialiased dark">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>NormaQ - Explorador Documental ISO</title>
    <script src="https://cdn.tailwindcss.com"></script>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
    
    <script>
        tailwind.config = {
            darkMode: 'class',
            theme: {
                extend: {
                    fontFamily: {
                        sans: ['Inter', 'sans-serif'],
                    },
                    colors: {
                        'dark-bg': '#050505',
                        'dark-panel': '#0a0a0a',
                        'dark-border': '#1a1a1a',
                        'dark-input': '#111111',
                        'brand-purple': '#9333ea',
                    }
                }
            }
        }

        // View Transitions API
        function toggleTheme(event) {
            const isCurrentlyDark = document.documentElement.classList.contains('dark');
            
            if (!document.startViewTransition) {
                document.documentElement.classList.toggle('dark');
                updateToggleText(!isCurrentlyDark);
                return;
            }

            const x = event?.clientX ?? window.innerWidth / 2;
            const y = event?.clientY ?? window.innerHeight / 2;
            const endRadius = Math.hypot(
                Math.max(x, window.innerWidth - x),
                Math.max(y, window.innerHeight - y)
            );

            const transition = document.startViewTransition(() => {
                document.documentElement.classList.toggle('dark');
                updateToggleText(!isCurrentlyDark);
            });

            transition.ready.then(() => {
                const clipPath = [
                    `circle(0px at ${x}px ${y}px)`,
                    `circle(${endRadius}px at ${x}px ${y}px)`
                ];

                document.documentElement.animate(
                    { clipPath: clipPath },
                    {
                        duration: 600,
                        easing: "ease-in-out",
                        pseudoElement: "::view-transition-new(root)",
                    }
                );
            });
        }

        function updateToggleText(willBeDark) {
            const iconEl = document.getElementById('toggle-icon');
            if(willBeDark) {
                iconEl.className = 'w-4 h-4 rounded-full bg-yellow-500 shadow-[0_0_10px_rgba(234,179,8,0.5)]';
            } else {
                iconEl.className = 'w-4 h-4 rounded-full bg-orange-400';
            }
        }
    </script>

    <style>
        ::view-transition-old(root), ::view-transition-new(root) { animation: none; mix-blend-mode: normal; }
        ::view-transition-old(root) { z-index: 1; }
        ::view-transition-new(root) { z-index: 9999; }
        
        /* Custom Scrollbar */
        ::-webkit-scrollbar { width: 6px; height: 6px; }
        ::-webkit-scrollbar-track { background: transparent; }
        ::-webkit-scrollbar-thumb { background-color: rgba(156, 163, 175, 0.3); border-radius: 10px; }
        .dark ::-webkit-scrollbar-thumb { background-color: rgba(75, 85, 99, 0.4); }
        
        /* Ocultar el marcador nativo del acordeón (details) */
        details > summary { list-style: none; }
        details > summary::-webkit-details-marker { display: none; }
    </style>
</head>
<!-- Usamos h-screen y overflow-hidden en el body para que el explorador tenga su propio scroll interno -->
<body class="bg-[#F8FAFC] dark:bg-dark-bg text-slate-900 dark:text-white font-sans h-screen flex flex-col transition-colors duration-200 overflow-hidden">

    <!-- TOP NAVBAR -->
    <nav class="flex-shrink-0 z-40 w-full bg-white dark:bg-dark-panel border-b border-slate-200 dark:border-dark-border shadow-sm">
        <div class="w-full px-4 sm:px-6 lg:px-8">
            <div class="flex justify-between items-center h-16">
                
                <div class="flex items-center gap-3">
                    <div class="w-8 h-8 rounded-lg bg-brand-purple flex items-center justify-center text-white font-bold text-sm shadow-[0_0_10px_rgba(147,51,234,0.3)]">
                        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
                    </div>
                    <div class="flex items-baseline gap-2">
                        <span class="text-xl font-bold tracking-tight text-slate-900 dark:text-white">NormaQ</span>
                        <span class="text-xs font-medium text-slate-400 dark:text-gray-500 bg-slate-100 dark:bg-dark-border px-2 py-0.5 rounded-full">Gestión ISO 9001</span>
                    </div>
                </div>

                <div class="flex items-center gap-6">
                    <div class="hidden sm:flex items-center gap-3 pr-6 border-r border-slate-200 dark:border-dark-border">
                        <div class="text-right">
                            <!-- Datos traídos de C# (ViewModel) -->
                            <p class="text-xs text-slate-500 dark:text-gray-400 font-medium">{{ $vm->DepartamentoActivoNombre ?? 'Selecciona un depto' }}</p>
                            <p class="text-sm font-semibold text-slate-800 dark:text-gray-200">{{ $vm->UsuarioNombre ?? 'Usuario Operativo' }}</p>
                        </div>
                        <div class="w-9 h-9 rounded-full bg-brand-purple/10 flex items-center justify-center text-brand-purple border border-brand-purple/20">
                            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
                        </div>
                    </div>

                    <button onclick="toggleTheme(event)" class="p-2 rounded-full hover:bg-slate-100 dark:hover:bg-dark-border transition-colors">
                        <div id="toggle-icon" class="w-4 h-4 rounded-full bg-yellow-500 shadow-[0_0_10px_rgba(234,179,8,0.5)]"></div>
                    </button>
                </div>

            </div>
        </div>
    </nav>

    <!-- CONTENIDO PRINCIPAL (Layout Dividido: Sidebar + Main) -->
    <div class="flex-grow flex overflow-hidden">
        
        <!-- SIDEBAR DE FILTROS -->
        <aside class="w-64 lg:w-72 flex-shrink-0 bg-slate-50/50 dark:bg-[#0a0a0a] border-r border-slate-200 dark:border-dark-border overflow-y-auto p-5 hidden md:block">
            
            <div class="mb-8">
                <h3 class="text-xs font-bold text-slate-400 dark:text-gray-500 uppercase tracking-widest mb-3">Filtrar por Estado</h3>
                <ul class="space-y-1.5">
                    <!-- Estado Activo -->
                    <li>
                        <a href="#" class="flex items-center gap-3 px-3 py-2 rounded-lg bg-white dark:bg-dark-input border border-brand-purple/30 dark:border-brand-purple/50 shadow-sm transition-colors">
                            <div class="w-2.5 h-2.5 rounded-full bg-slate-400 dark:bg-gray-500"></div>
                            <span class="text-sm font-semibold text-brand-purple dark:text-brand-purple">Todos</span>
                        </a>
                    </li>
                    <li>
                        <a href="#" class="flex items-center gap-3 px-3 py-2 rounded-lg hover:bg-white dark:hover:bg-dark-input border border-transparent hover:border-slate-200 dark:hover:border-dark-border transition-colors">
                            <div class="w-2.5 h-2.5 rounded-full bg-emerald-500"></div>
                            <span class="text-sm font-medium text-slate-600 dark:text-gray-300">Aprobado</span>
                        </a>
                    </li>
                    <li>
                        <a href="#" class="flex items-center gap-3 px-3 py-2 rounded-lg hover:bg-white dark:hover:bg-dark-input border border-transparent hover:border-slate-200 dark:hover:border-dark-border transition-colors">
                            <div class="w-2.5 h-2.5 rounded-full bg-amber-500"></div>
                            <span class="text-sm font-medium text-slate-600 dark:text-gray-300">En revisión</span>
                        </a>
                    </li>
                    <li>
                        <a href="#" class="flex items-center gap-3 px-3 py-2 rounded-lg hover:bg-white dark:hover:bg-dark-input border border-transparent hover:border-slate-200 dark:hover:border-dark-border transition-colors">
                            <div class="w-2.5 h-2.5 rounded-full bg-slate-400"></div>
                            <span class="text-sm font-medium text-slate-600 dark:text-gray-300">Borrador</span>
                        </a>
                    </li>
                    <li>
                        <a href="#" class="flex items-center gap-3 px-3 py-2 rounded-lg hover:bg-white dark:hover:bg-dark-input border border-transparent hover:border-slate-200 dark:hover:border-dark-border transition-colors">
                            <div class="w-2.5 h-2.5 rounded-full bg-red-500"></div>
                            <span class="text-sm font-medium text-slate-600 dark:text-gray-300">Obsoleto</span>
                        </a>
                    </li>
                </ul>
            </div>

            <div>
                <h3 class="text-xs font-bold text-slate-400 dark:text-gray-500 uppercase tracking-widest mb-3">Nivel ISO</h3>
                <ul class="space-y-1.5">
                    <li>
                        <a href="#" class="flex items-center gap-3 px-3 py-2 rounded-lg hover:bg-white dark:hover:bg-dark-input border border-transparent hover:border-slate-200 dark:hover:border-dark-border transition-colors">
                            <svg class="w-4 h-4 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 10h16M4 14h16M4 18h16"></path></svg>
                            <span class="text-sm font-medium text-slate-600 dark:text-gray-300">Todos los niveles</span>
                        </a>
                    </li>
                    <li>
                        <a href="#" class="flex items-center gap-3 px-3 py-2 rounded-lg hover:bg-white dark:hover:bg-dark-input border border-transparent hover:border-slate-200 dark:hover:border-dark-border transition-colors">
                            <div class="w-4 h-4 rounded bg-blue-500/20 flex items-center justify-center border border-blue-500/50">
                                <div class="w-1.5 h-1.5 bg-blue-500 rounded-sm"></div>
                            </div>
                            <span class="text-sm font-medium text-slate-600 dark:text-gray-300">Nivel 1 — Manual</span>
                        </a>
                    </li>
                    <li>
                        <a href="#" class="flex items-center gap-3 px-3 py-2 rounded-lg hover:bg-white dark:hover:bg-dark-input border border-transparent hover:border-slate-200 dark:hover:border-dark-border transition-colors">
                            <div class="w-4 h-4 rounded bg-amber-500/20 flex items-center justify-center border border-amber-500/50">
                                <svg class="w-3 h-3 text-amber-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"></path></svg>
                            </div>
                            <span class="text-sm font-medium text-slate-600 dark:text-gray-300">Nivel 2 — Procedimientos</span>
                        </a>
                    </li>
                    <li>
                        <a href="#" class="flex items-center gap-3 px-3 py-2 rounded-lg hover:bg-white dark:hover:bg-dark-input border border-transparent hover:border-slate-200 dark:hover:border-dark-border transition-colors">
                            <div class="w-4 h-4 rounded bg-slate-500/20 flex items-center justify-center border border-slate-500/50">
                                <svg class="w-3 h-3 text-slate-500 dark:text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"></path></svg>
                            </div>
                            <span class="text-sm font-medium text-slate-600 dark:text-gray-300">Nivel 3 — Instrucciones</span>
                        </a>
                    </li>
                    <!-- Estado Activo de Nivel (Basado en tu imagen) -->
                    <li>
                        <a href="#" class="flex items-center gap-3 px-3 py-2 rounded-lg bg-white dark:bg-dark-input border border-brand-purple/30 dark:border-brand-purple/50 shadow-sm transition-colors">
                            <div class="w-4 h-4 rounded bg-yellow-500/20 flex items-center justify-center border border-yellow-500/50">
                                <svg class="w-3 h-3 text-yellow-600 dark:text-yellow-500" fill="currentColor" viewBox="0 0 20 20"><path d="M2 6a2 2 0 012-2h5l2 2h5a2 2 0 012 2v6a2 2 0 01-2 2H4a2 2 0 01-2-2V6z"></path></svg>
                            </div>
                            <span class="text-sm font-semibold text-brand-purple dark:text-brand-purple">Nivel 4 — Registros</span>
                        </a>
                    </li>
                </ul>
            </div>
            
        </aside>

        <!-- ÁREA PRINCIPAL: EXPLORADOR DOCUMENTAL -->
        <main class="flex-1 flex flex-col bg-white dark:bg-dark-bg min-w-0">
            
            <!-- Top Bar del Explorador -->
            <div class="flex-shrink-0 border-b border-slate-200 dark:border-dark-border px-6 py-4 flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white dark:bg-dark-bg">
                
                <!-- Breadcrumbs -->
                <div class="flex items-center gap-2 text-sm text-slate-500 dark:text-gray-400 font-medium">
                    <span class="hover:text-brand-purple cursor-pointer transition-colors">Producción</span>
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path></svg>
                    <span class="text-slate-800 dark:text-gray-200">ISO 9001</span>
                    <span class="text-slate-400 dark:text-gray-600">—</span>
                    <span class="text-brand-purple">Todos los niveles</span>
                </div>

                <!-- Buscador y Acciones -->
                <div class="flex items-center gap-3 w-full sm:w-auto">
                    <div class="relative flex-1 sm:w-64">
                        <input type="text" placeholder="Buscar documento..." class="w-full pl-9 pr-4 py-2 bg-slate-50 dark:bg-dark-input border border-slate-200 dark:border-dark-border rounded-lg text-sm text-slate-800 dark:text-white placeholder-slate-400 dark:placeholder-gray-500 focus:outline-none focus:border-brand-purple focus:ring-1 focus:ring-brand-purple transition-all">
                        <svg class="w-4 h-4 text-slate-400 absolute left-3 top-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path></svg>
                    </div>
                    <!-- Botón "Nuevo Documento" según requerimiento, solo visible si hay rol adecuado (ej. admin) -->
                    <button class="flex-shrink-0 flex items-center gap-2 px-4 py-2 bg-brand-purple hover:bg-brand-purple-hover text-white text-sm font-semibold rounded-lg shadow-sm transition-colors">
                        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path></svg>
                        <span class="hidden sm:inline">Nuevo documento</span>
                    </button>
                </div>
            </div>

            <!-- CONTENEDOR DEL ÁRBOL (Blade Integrado con el C# ViewModel) -->
            <div class="flex-1 overflow-y-auto p-6">
                
                @if(isset($vm) && $vm->ArbolDocumental != null && count($vm->ArbolDocumental) > 0)
                    <div class="max-w-4xl space-y-4">
                        <!-- Ciclo real de los niveles que manda C# -->
                        @foreach($vm->ArbolDocumental as $nivel)
                            
                            <!-- Elemento de Carpeta Colapsable (Acordeón nativo de HTML5) -->
                            <details class="group bg-white dark:bg-dark-panel border border-slate-200 dark:border-dark-border rounded-xl shadow-sm overflow-hidden" open>
                                
                                <summary class="flex items-center justify-between p-4 cursor-pointer hover:bg-slate-50 dark:hover:bg-[#111] transition-colors select-none">
                                    <div class="flex items-center gap-3">
                                        <!-- Flecha que rota al abrir/cerrar -->
                                        <svg class="w-4 h-4 text-slate-400 transition-transform duration-200 group-open:rotate-90" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path></svg>
                                        
                                        <!-- Icono de Carpeta -->
                                        <svg class="w-5 h-5 text-yellow-500" fill="currentColor" viewBox="0 0 20 20"><path d="M2 6a2 2 0 012-2h5l2 2h5a2 2 0 012 2v6a2 2 0 01-2 2H4a2 2 0 01-2-2V6z"></path></svg>
                                        
                                        <h3 class="font-semibold text-slate-800 dark:text-gray-200">
                                            Nivel {{ $nivel->Numero }} — {{ $nivel->Nombre }}
                                        </h3>
                                    </div>
                                    <span class="text-xs font-medium text-slate-500 dark:text-gray-500 bg-slate-100 dark:bg-dark-input px-2.5 py-1 rounded-full">
                                        {{ count($nivel->DocumentosLogicos) }} docs
                                    </span>
                                </summary>
                                
                                <!-- Contenido de la Carpeta (Archivos) -->
                                <div class="px-4 pb-4 pt-1 border-t border-slate-100 dark:border-dark-border">
                                    <div class="pl-7 space-y-2 mt-2">
                                        
                                        @if(count($nivel->DocumentosLogicos) == 0)
                                            <p class="text-sm text-slate-400 dark:text-gray-500 italic py-2">No hay documentos en este nivel.</p>
                                        @else
                                            @foreach($nivel->DocumentosLogicos as $doc)
                                                <!-- Archivo Individual -->
                                                <div class="flex items-center justify-between p-3 rounded-lg hover:bg-slate-50 dark:hover:bg-[#151515] border border-transparent hover:border-slate-200 dark:hover:border-dark-border transition-colors group/doc">
                                                    
                                                    <div class="flex items-start gap-3">
                                                        <svg class="w-5 h-5 text-slate-400 dark:text-gray-500 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"></path></svg>
                                                        <div>
                                                            <div class="flex items-center gap-2">
                                                                <span class="text-xs font-mono font-medium text-brand-purple dark:text-brand-purple bg-brand-purple/10 px-1.5 py-0.5 rounded">{{ $doc->Codigo }}</span>
                                                                <span class="text-sm font-medium text-slate-700 dark:text-gray-300 group-hover/doc:text-brand-purple transition-colors">{{ $doc->Nombre }}</span>
                                                            </div>
                                                            
                                                            <!-- Sub-lista de Versiones de ese documento -->
                                                            @if(count($doc->VersionesFisicas) > 0)
                                                                @php $ultimaVersion = $doc->VersionesFisicas[0]; @endphp
                                                                <div class="flex items-center gap-3 mt-1.5">
                                                                    <span class="text-xs text-slate-500 dark:text-gray-500">v{{ $ultimaVersion->VersionMayor }}.{{ $ultimaVersion->VersionMenor }}</span>
                                                                    
                                                                    @if($ultimaVersion->Estado == 'Aprobado')
                                                                        <span class="flex items-center gap-1 text-[10px] uppercase tracking-wide font-bold text-emerald-600 dark:text-emerald-500"><div class="w-1.5 h-1.5 rounded-full bg-emerald-500"></div> Aprobado</span>
                                                                    @elseif($ultimaVersion->Estado == 'Revision')
                                                                        <span class="flex items-center gap-1 text-[10px] uppercase tracking-wide font-bold text-amber-600 dark:text-amber-500"><div class="w-1.5 h-1.5 rounded-full bg-amber-500"></div> En Revisión</span>
                                                                    @else
                                                                        <span class="flex items-center gap-1 text-[10px] uppercase tracking-wide font-bold text-slate-500 dark:text-gray-400"><div class="w-1.5 h-1.5 rounded-full bg-slate-400"></div> {{ $ultimaVersion->Estado }}</span>
                                                                    @endif

                                                                    <!-- Alerta si requiere intervención del usuario -->
                                                                    @if($ultimaVersion->RequiereMiIntervencion)
                                                                        <span class="bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-400 text-[10px] px-2 py-0.5 rounded-full font-semibold ml-2 border border-amber-200 dark:border-amber-800">Tu firma requerida</span>
                                                                    @endif
                                                                </div>
                                                            @else
                                                                <span class="text-xs text-slate-400 dark:text-gray-600 mt-1 block">Sin versiones físicas</span>
                                                            @endif
                                                        </div>
                                                    </div>
                                                    
                                                    <button class="p-2 text-slate-400 hover:text-brand-purple opacity-0 group-hover/doc:opacity-100 transition-all">
                                                        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"></path></svg>
                                                    </button>
                                                </div>
                                            @endforeach
                                        @endif

                                    </div>
                                </div>
                            </details>

                        @endforeach
                    </div>
                @else
                    <!-- Fallback Visual: Si entras a la vista sin datos de C# -->
                    <div class="h-full flex flex-col items-center justify-center text-center p-8">
                        <div class="w-16 h-16 rounded-2xl bg-slate-100 dark:bg-dark-panel border border-slate-200 dark:border-dark-border flex items-center justify-center text-slate-400 dark:text-gray-600 mb-4 shadow-sm">
                            <svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"></path></svg>
                        </div>
                        <h3 class="text-lg font-semibold text-slate-800 dark:text-white mb-2">Conectando con el Servidor...</h3>
                        <p class="text-sm text-slate-500 dark:text-gray-400 max-w-md">La estructura del explorador documental está lista. Esperando que el controlador C# inyecte la variable <code>$vm->ArbolDocumental</code>.</p>
                    </div>
                @endif
                
            </div>
        </main>
    </div>

</body>
</html>