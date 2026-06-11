<!DOCTYPE html>
<html lang="es" class="dark antialiased">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Versiones — QualityDoc</title>
    <script src="https://cdn.tailwindcss.com"></script>
    <link href="https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&family=Space+Grotesk:wght@600;700&display=swap" rel="stylesheet">
    <script>
        tailwind.config = {
            darkMode: 'class',
            theme: { extend: { fontFamily: { sans: ['Plus Jakarta Sans', 'sans-serif'], display: ['Space Grotesk', 'sans-serif'] } } }
        }
    </script>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { background-color: #030305; min-height: 100vh; }
        html:not(.dark) body { background-color: #e2e8f0; }
        .glass-panel {
            background: rgba(255,255,255,0.03); backdrop-filter: blur(16px);
            border: 1px solid rgba(255,255,255,0.05);
        }
        html:not(.dark) .glass-panel {
            background: rgba(255,255,255,0.95);
            border: 1px solid rgba(148,163,184,0.3);
            box-shadow: 0 10px 30px -5px rgba(0,0,0,0.08);
        }
    </style>
</head>
<body class="font-sans text-slate-900 dark:text-white pb-20">

    {{-- HEADER --}}
    <header class="fixed top-0 left-0 right-0 h-20 z-50 bg-white/90 dark:bg-black/20 backdrop-blur-md border-b border-slate-300 dark:border-white/5 flex items-center justify-between px-6 md:px-10 shadow-sm dark:shadow-none">
        <div class="flex items-center gap-4">
            <a href="/operativo/dashboard" class="w-10 h-10 rounded-xl bg-slate-200 dark:bg-white/5 border border-slate-300 dark:border-white/10 flex items-center justify-center text-slate-600 dark:text-slate-400 hover:bg-slate-300 dark:hover:bg-white/10 transition-all">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/>
                </svg>
            </a>
            <div class="flex flex-col">
                <span class="font-display font-bold text-lg tracking-tight leading-none">QualityDoc<span class="text-indigo-500">.</span></span>
                <span class="text-[10px] font-bold text-slate-500 uppercase tracking-widest leading-none mt-1">Versiones del Documento</span>
            </div>
        </div>
        <div class="hidden md:flex items-center gap-3">
            <div class="text-right">
                <p class="text-sm font-bold text-slate-800 dark:text-white leading-none">{{ $userName }}</p>
                <p class="text-[10px] text-slate-500 uppercase tracking-wider mt-1">{{ $departamento }}</p>
            </div>
        </div>
    </header>

    <main class="max-w-5xl mx-auto px-6 pt-32 space-y-8">

        @if($aprobada)

        {{-- SECCIÓN VERSIÓN APROBADA --}}
        <div class="glass-panel rounded-[24px] overflow-hidden shadow-xl">
            
            {{-- CORREGIDO: Cabecera limpia sin duplicación de divs internos --}}
            <div id="preview-header-container" class="bg-emerald-50 dark:bg-emerald-500/10 border-b border-emerald-200 dark:border-emerald-500/20 px-6 py-4 flex items-center justify-between">
                <div class="flex items-center gap-3">
                    <span id="preview-estado-dot" class="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></span>
                    <h2 id="preview-estado-label" class="text-sm font-bold font-display uppercase tracking-widest text-emerald-700 dark:text-emerald-400">Versión Vigente</h2>
                </div>
                <div class="flex items-center gap-3">
                    <span class="text-xs text-slate-500 font-mono">{{ $aprobada['metadata']['codigo'] ?? '' }}</span>
                    <span id="preview-badge" class="text-[10px] font-bold uppercase tracking-widest px-2 py-0.5 rounded-full bg-emerald-100 dark:bg-emerald-500/20 text-emerald-700 dark:text-emerald-400 border border-emerald-200 dark:border-emerald-500/30">Aprobado</span>
                </div>
            </div>

            {{-- BOTÓN VOLVER (Inicialmente Oculto) --}}
            <div id="btn-volver" class="hidden px-6 py-3 bg-amber-50 dark:bg-amber-500/5 border-b border-amber-200 dark:border-amber-500/10 flex items-center justify-between transition-all">
                <span class="text-xs font-bold text-amber-700 dark:text-amber-400">Viendo una versión obsoleta</span>
                <button onclick="restaurarAprobada()"
                    class="px-3 py-1.5 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-bold transition-all shadow-md">
                    ← Volver a versión vigente
                </button>
            </div>

            {{-- VISTA PREVIA --}}
            <div id="preview-container" class="w-full bg-slate-100 dark:bg-black/30" style="min-height: 600px;">
                @php $ext = strtolower($aprobada['metadata']['extension'] ?? ''); @endphp

                @if($ext === '.pdf' || $ext === '.txt')
                    <iframe src="/operativo/preview/{{ $aprobada['storage_path'] }}"
                        class="w-full" style="height: 600px;" frameborder="0"></iframe>

                @elseif($ext === '.docx')
                    <iframe src="/operativo/preview/{{ $aprobada['storage_path'] }}?convert=pdf"
                        class="w-full" style="height: 600px;" frameborder="0"></iframe>

                @else
                    <div class="flex flex-col items-center justify-center py-24 gap-4 text-slate-500">
                        <svg class="w-16 h-16 opacity-30" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
                        </svg>
                        <p class="text-sm font-bold">Vista previa no disponible para archivos <span class="font-mono">{{ $ext }}</span></p>
                    </div>
                @endif
            </div>

            {{-- INFO + DESCARGA --}}
            <div class="px-6 py-4 border-t border-slate-200 dark:border-white/5 flex items-center justify-between flex-wrap gap-4">
                <div class="flex items-center gap-4 text-xs text-slate-500">
                    @if(!empty($aprobada['metadata']['owner']))
                        <span>Elaboró: <span class="font-bold text-slate-700 dark:text-slate-300">{{ $aprobada['metadata']['owner'] }}</span></span>
                    @endif
                    @if(!empty($aprobada['metadata']['approved_by']))
                        <span>Aprobó: <span class="font-bold text-slate-700 dark:text-slate-300">{{ $aprobada['metadata']['approved_by'] }}</span></span>
                    @endif
                    @if(!empty($aprobada['metadata']['file_size_kb']))
                        <span>{{ $aprobada['metadata']['file_size_kb'] }} KB</span>
                    @endif
                </div>
                <a id="download-link" href="/operativo/descargar/{{ $aprobada['storage_path'] }}"
                    class="flex items-center gap-2 px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold transition-all shadow-lg shadow-indigo-500/20">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"/>
                    </svg>
                    Descargar
                </a>
            </div>
        </div>

        @else
            <div class="glass-panel rounded-[24px] p-12 text-center text-slate-500">
                <p class="text-sm font-bold">Este documento no tiene una versión aprobada activa.</p>
            </div>
        @endif

        {{-- SECCIÓN VERSIONES OBSOLETAS --}}
        @if($obsoletas->count() > 0)
        <div class="glass-panel rounded-[24px] overflow-hidden shadow-xl">
            <div class="bg-slate-100 dark:bg-black/20 border-b border-slate-300 dark:border-white/5 px-6 py-4">
                <h2 class="text-sm font-bold font-display uppercase tracking-widest text-slate-600 dark:text-slate-500">Versiones Obsoletas</h2>
            </div>

            <div class="divide-y divide-slate-200 dark:divide-white/5">
                @foreach($obsoletas as $version)
                <div class="px-5 py-4 flex items-center justify-between hover:bg-slate-50 dark:hover:bg-white/5 transition-colors">
                    <div class="flex items-center gap-4">
                        <div class="w-8 h-8 rounded bg-amber-100 dark:bg-amber-500/10 flex items-center justify-center text-amber-600 dark:text-amber-400">
                            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z"/>
                            </svg>
                        </div>
                        <div>
                            <p class="text-sm font-bold text-slate-700 dark:text-slate-300">{{ $version['display_name'] }}</p>
                            <div class="flex items-center gap-2 mt-1">
                                <span class="text-[10px] font-mono font-bold text-slate-500 bg-slate-200 dark:bg-black/30 px-1.5 py-0.5 rounded">{{ $version['metadata']['codigo'] }}</span>
                                <span class="text-[10px] font-bold uppercase tracking-widest px-2 py-0.5 rounded-full bg-amber-100 dark:bg-amber-500/20 text-amber-700 dark:text-amber-400 border border-amber-200 dark:border-amber-500/30">Obsoleto</span>
                                @if(!empty($version['metadata']['file_size_kb']))
                                    <span class="text-[10px] text-slate-500">{{ $version['metadata']['file_size_kb'] }} KB</span>
                                @endif
                            </div>
                        </div>
                    </div>
                    <div class="flex items-center gap-2">
                        @php $extObs = strtolower($version['metadata']['extension'] ?? ''); @endphp
                        @if(in_array($extObs, ['.pdf', '.txt', '.docx']))
                            <button onclick="cargarPreview('{{ $version['storage_path'] }}', '{{ $extObs }}')"
                                class="px-3 py-1.5 rounded-lg bg-slate-200 dark:bg-white/5 hover:bg-slate-300 dark:hover:bg-white/10 text-xs font-bold text-slate-600 dark:text-slate-400 transition-all border border-slate-300 dark:border-white/10">
                                Ver
                            </button>
                        @else
                            <span class="px-3 py-1.5 rounded-lg bg-slate-100 dark:bg-white/5 text-xs font-bold text-slate-400 border border-slate-200 dark:border-white/5">
                                No previsualizable
                            </span>
                        @endif
                        <a href="/operativo/descargar/{{ $version['storage_path'] }}"
                            class="px-3 py-1.5 rounded-lg bg-indigo-50 dark:bg-indigo-500/10 hover:bg-indigo-100 dark:hover:bg-indigo-500/20 text-xs font-bold text-indigo-600 dark:text-indigo-400 transition-all border border-indigo-200 dark:border-indigo-500/20">
                            Descargar
                        </a>
                    </div>
                </div>
                @endforeach
            </div>

            {{-- PAGINACIÓN OBSOLETAS --}}
            @if($lastPage > 1)
            <div class="px-6 py-4 border-t border-slate-200 dark:border-white/5 flex items-center justify-between">
                <span class="text-xs text-slate-500 font-medium">Página {{ $page }} de {{ $lastPage }}</span>
                <div class="flex items-center gap-2">
                    @if($page > 1)
                        <a href="?page={{ $page - 1 }}" class="px-3 py-1.5 rounded-lg bg-slate-200 dark:bg-white/5 text-xs font-bold text-slate-600 dark:text-slate-400 hover:bg-slate-300 dark:hover:bg-white/10 transition-all border border-slate-300 dark:border-white/10">← Anterior</a>
                    @endif
                    @if($page < $lastPage)
                        <a href="?page={{ $page + 1 }}" class="px-3 py-1.5 rounded-lg bg-indigo-600 text-xs font-bold text-white hover:bg-indigo-700 transition-all">Siguiente →</a>
                    @endif
                </div>
            </div>
            @endif
        </div>
        @endif

    </main>

    {{-- SCRIPTS CORREGIDOS --}}
    <script>
        // Mapeo global de la versión aprobada desde PHP/Blade
        @if($aprobada)
            const aprobadaPath = '{{ $aprobada['storage_path'] }}';
            const aprobadaExt  = '{{ strtolower($aprobada['metadata']['extension'] ?? '') }}';
        @else
            const aprobadaPath = null;
            const aprobadaExt  = null;
        @endif

        function cargarPreview(storagePath, ext) {
            const container = document.getElementById('preview-container');
            const btnVolver = document.getElementById('btn-volver');
            const headerBox = document.getElementById('preview-header-container');
            const label     = document.getElementById('preview-estado-label');
            const badge     = document.getElementById('preview-badge');
            const dot       = document.getElementById('preview-estado-dot');
            const download  = document.getElementById('download-link');

            // 1. Cambiar header a modo Obsoleto (Amber)
            btnVolver.classList.remove('hidden');
            headerBox.className = 'bg-amber-50 dark:bg-amber-500/10 border-b border-amber-200 dark:border-amber-500/20 px-6 py-4 flex items-center justify-between';
            label.textContent = 'Viendo versión obsoleta';
            label.className = 'text-sm font-bold font-display uppercase tracking-widest text-amber-700 dark:text-amber-400';
            dot.className   = 'w-2 h-2 rounded-full bg-amber-500'; // Quita la animación de pulso
            badge.textContent = 'Obsoleto';
            badge.className = 'text-[10px] font-bold uppercase tracking-widest px-2 py-0.5 rounded-full bg-amber-100 dark:bg-amber-500/20 text-amber-700 dark:text-amber-400 border border-amber-200 dark:border-amber-500/30';
            
            // Actualizar link de descarga para bajar la versión obsoleta que se está visualizando
            if(download) download.href = `/operativo/descargar/${storagePath}`;

            // 2. Insertar Loading Spinner
            const url = ext === '.docx' ? `/operativo/preview/${storagePath}?convert=pdf` : `/operativo/preview/${storagePath}`;
            container.innerHTML = `
                <div class="flex items-center justify-center py-24 gap-3 text-slate-500">
                    <svg class="w-5 h-5 animate-spin text-indigo-500" fill="none" viewBox="0 0 24 24">
                        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
                    </svg>
                    <span class="text-xs font-bold uppercase tracking-widest text-slate-400">Cargando vista previa...</span>
                </div>
            `;

            // 3. Cargar Iframe
            setTimeout(() => {
                container.innerHTML = `<iframe src="${url}" class="w-full" style="height:600px;" frameborder="0"></iframe>`;
            }, 200);
        }

        function restaurarAprobada() {
            if (!aprobadaPath) return;

            const container = document.getElementById('preview-container');
            const btnVolver = document.getElementById('btn-volver');
            const headerBox = document.getElementById('preview-header-container');
            const label     = document.getElementById('preview-estado-label');
            const badge     = document.getElementById('preview-badge');
            const dot       = document.getElementById('preview-estado-dot');
            const download  = document.getElementById('download-link');

            // 1. Restaurar header a modo Aprobado/Vigente (Emerald)
            btnVolver.classList.add('hidden');
            headerBox.className = 'bg-emerald-50 dark:bg-emerald-500/10 border-b border-emerald-200 dark:border-emerald-500/20 px-6 py-4 flex items-center justify-between';
            label.textContent = 'Versión Vigente';
            label.className = 'text-sm font-bold font-display uppercase tracking-widest text-emerald-700 dark:text-emerald-400';
            dot.className   = 'w-2 h-2 rounded-full bg-emerald-500 animate-pulse';
            badge.textContent = 'Aprobado';
            badge.className = 'text-[10px] font-bold uppercase tracking-widest px-2 py-0.5 rounded-full bg-emerald-100 dark:bg-emerald-500/20 text-emerald-700 dark:text-emerald-400 border border-emerald-200 dark:border-emerald-500/30';
            
            // Restaurar link de descarga original
            if(download) download.href = `/operativo/descargar/${aprobadaPath}`;

            // 2. Renderizar Iframe vigente
            const url = aprobadaExt === '.docx' ? `/operativo/preview/${aprobadaPath}?convert=pdf` : `/operativo/preview/${aprobadaPath}`;
            container.innerHTML = `<iframe src="${url}" class="w-full" style="height:600px;" frameborder="0"></iframe>`;
        }
    </script>
</body>
</html>