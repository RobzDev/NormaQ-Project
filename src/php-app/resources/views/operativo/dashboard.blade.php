<!DOCTYPE html>
<html lang="es" class="dark antialiased">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Portal Operativo — QualityDoc</title>
    
    <script src="https://cdn.tailwindcss.com"></script>
    <link href="https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&family=Space+Grotesk:wght@600;700&display=swap" rel="stylesheet">

    <script>
        tailwind.config = {
            darkMode: 'class',
            theme: {
                extend: {
                    fontFamily: { 
                        sans: ['Plus Jakarta Sans', 'sans-serif'],
                        display: ['Space Grotesk', 'sans-serif'],
                    },
                    animation: {
                        'fade-in-up': 'fadeInUp 0.6s cubic-bezier(0.16, 1, 0.3, 1) forwards',
                    },
                    keyframes: {
                        fadeInUp: {
                            '0%': { opacity: '0', transform: 'translateY(20px)' },
                            '100%': { opacity: '1', transform: 'translateY(0)' },
                        }
                    }
                }
            }
        }

        function toggleTheme(event) {
            const html = document.documentElement;
            if (!document.startViewTransition) { html.classList.toggle('dark'); return; }
            const x = event?.clientX ?? window.innerWidth / 2;
            const y = event?.clientY ?? window.innerHeight / 2;
            const r = Math.hypot(Math.max(x, window.innerWidth - x), Math.max(y, window.innerHeight - y));
            const transition = document.startViewTransition(() => { html.classList.toggle('dark'); });
            transition.ready.then(() => {
                document.documentElement.animate(
                    { clipPath: [`circle(0px at ${x}px ${y}px)`, `circle(${r}px at ${x}px ${y}px)`] },
                    { duration: 600, easing: 'cubic-bezier(0.4,0,0.2,1)', pseudoElement: '::view-transition-new(root)' }
                );
            });
        }
    </script>

    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { 
            background-color: #030305; transition: background-color 0.5s ease;
            min-height: 100vh; overflow-x: hidden;
        }
        /* Ajuste Modo Claro: Fondo un poco más profundo para que contraste con las tarjetas blancas */
        html:not(.dark) body { background-color: #e2e8f0; }

        ::view-transition-old(root), ::view-transition-new(root) { animation: none; mix-blend-mode: normal; }
        ::view-transition-old(root) { z-index: 1; } ::view-transition-new(root) { z-index: 9999; }

        /* Custom Scrollbar */
        ::-webkit-scrollbar { width: 6px; }
        ::-webkit-scrollbar-track { background: transparent; }
        ::-webkit-scrollbar-thumb { background-color: rgba(99, 102, 241, 0.3); border-radius: 10px; }
        .dark ::-webkit-scrollbar-thumb { background-color: rgba(255, 255, 255, 0.15); }

        .glass-panel {
            background: rgba(255, 255, 255, 0.03); backdrop-filter: blur(16px);
            border: 1px solid rgba(255, 255, 255, 0.05);
        }
        /* Ajuste Modo Claro: Panel mucho más sólido con sombra definida */
        html:not(.dark) .glass-panel {
            background: rgba(255, 255, 255, 0.95); 
            border: 1px solid rgba(148, 163, 184, 0.3);
            box-shadow: 0 10px 30px -5px rgba(0, 0, 0, 0.08);
        }

        .search-input {
            background: rgba(255,255,255,0.03); border: 1px solid rgba(255,255,255,0.1); color: #f8fafc;
            transition: all 0.3s;
        }
        /* Ajuste Modo Claro: Buscador sólido con borde marcado */
        html:not(.dark) .search-input {
            background: #ffffff; border: 1px solid #cbd5e1; color: #1e293b;
            box-shadow: 0 2px 4px rgba(0,0,0,0.02);
        }
        .search-input:focus { border-color: #6366f1; box-shadow: 0 0 25px -5px rgba(99,102,241,0.3); outline: none; }
        html:not(.dark) .search-input:focus { border-color: #6366f1; box-shadow: 0 0 0 3px rgba(99,102,241,0.15); }
    </style>
</head>
<body class="font-sans text-slate-900 dark:text-white relative selection:bg-indigo-500 selection:text-white pb-20">

    <canvas id="particles" class="fixed inset-0 z-0 pointer-events-none"></canvas>

    <div class="fixed top-[-10%] left-[-5%] w-[500px] h-[500px] bg-indigo-600/10 rounded-full blur-[120px] pointer-events-none z-0 hidden dark:block"></div>
    <div class="fixed bottom-[-10%] right-[-5%] w-[400px] h-[400px] bg-emerald-600/10 rounded-full blur-[120px] pointer-events-none z-0 hidden dark:block"></div>

    {{-- HEADER GLASSMORPHISM --}}
    <header class="fixed top-0 left-0 right-0 h-20 z-50 bg-white/90 dark:bg-black/20 backdrop-blur-md border-b border-slate-300 dark:border-white/5 flex items-center justify-between px-6 md:px-10 shadow-sm dark:shadow-none">
        
        <div class="flex items-center gap-3">
            <div class="w-10 h-10 rounded-xl bg-indigo-600 flex items-center justify-center shadow-lg shadow-indigo-500/20">
                <svg class="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
                </svg>
            </div>
            <div class="flex flex-col">
                <span class="font-display font-bold text-lg tracking-tight leading-none">QualityDoc<span class="text-indigo-500">.</span></span>
                <span class="text-[10px] font-bold text-slate-500 uppercase tracking-widest leading-none mt-1">Portal Operativo</span>
            </div>
        </div>

        <div class="flex items-center gap-5">
            
            <button onclick="toggleTheme(event)" class="w-10 h-10 rounded-xl bg-slate-200 hover:bg-slate-300 dark:bg-white/5 border border-transparent dark:border-white/10 flex items-center justify-center transition-all hover:scale-105 text-slate-600 dark:text-amber-300">
                <svg class="w-4 h-4 block dark:hidden" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z"></path></svg>
                <svg class="w-4 h-4 hidden dark:block" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z"></path></svg>
            </button>

            <div class="h-8 w-px bg-slate-300 dark:bg-white/10 hidden md:block"></div>

            <div class="hidden md:flex items-center gap-3">
                <div class="text-right">
                    <p class="text-sm font-bold text-slate-800 dark:text-white leading-none">{{ $userName }}</p>
                    <p class="text-[10px] text-slate-500 uppercase tracking-wider mt-1">{{ $departamento }}</p>
                </div>
                <div class="w-10 h-10 rounded-full bg-emerald-100 dark:bg-emerald-500/10 border border-emerald-200 dark:border-emerald-500/20 flex items-center justify-center text-emerald-700 dark:text-emerald-400 font-bold text-sm">
                    {{ substr($userName, 0, 1) }}
                </div>
            </div>

            {{-- Logout --}}
            <form method="POST" action="{{ route('operativo.logout') }}" class="m-0 p-0">
                @csrf
                <button type="submit" class="w-10 h-10 md:w-auto md:px-4 md:py-2 rounded-xl bg-rose-50 dark:bg-rose-500/10 hover:bg-rose-100 dark:hover:bg-rose-500/20 border border-rose-200 dark:border-rose-500/20 text-rose-600 dark:text-rose-400 flex items-center justify-center gap-2 transition-all shadow-sm dark:shadow-none">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"/>
                    </svg>
                    <span class="text-xs font-bold hidden md:inline">Salir</span>
                </button>
            </form>
        </div>
    </header>

    {{-- MAIN CONTENT --}}
    <main class="relative z-10 max-w-5xl mx-auto px-6 pt-32 animate-fade-in-up">

        {{-- BUSCADOR PREMIUM --}}
        <div class="relative mb-10 group z-30">
            <div class="absolute inset-y-0 left-0 pl-5 flex items-center pointer-events-none">
                <svg class="w-5 h-5 text-slate-400 group-focus-within:text-indigo-500 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
                </svg>
            </div>
            <input 
                type="text" 
                id="buscador" 
                placeholder="Buscar documentos por código, nombre o norma..." 
                autocomplete="off"
                class="search-input w-full rounded-2xl py-4 pl-14 pr-5 text-sm font-medium"
            />
            
            {{-- Dropdown de Resultados --}}
            <div id="autocomplete-results" class="hidden absolute top-full left-0 w-full mt-2 glass-panel rounded-2xl overflow-hidden shadow-2xl z-40 max-h-80 overflow-y-auto transform origin-top transition-all scale-95 opacity-0">
                </div>
        </div>

        {{-- CONTENEDOR DEL ÁRBOL DE DOCUMENTOS --}}
        <div class="glass-panel rounded-[24px] overflow-hidden shadow-xl">
            
            <div class="bg-slate-100 dark:bg-black/20 border-b border-slate-300 dark:border-white/5 px-6 py-4 flex items-center justify-between">
                <h2 class="text-sm font-bold font-display uppercase tracking-widest text-slate-600 dark:text-slate-500">Documentos Autorizados</h2>
                <div class="flex items-center gap-2">
                    <span class="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></span>
                    <span class="text-xs text-slate-500 dark:text-slate-400 font-medium">Directorio Actualizado</span>
                </div>
            </div>

            <div class="p-4 md:p-6 space-y-4" id="arbol-documentos">
                
                @forelse($porNivel as $nivel => $documentos)
                    @php 
                        $info = $niveles[$nivel] ?? ['nombre' => 'Sin clasificar', 'icon' => 'folder', 'color' => 'indigo']; 
                    @endphp

                    <div id="nivel-{{ $loop->index }}" class="bg-white dark:bg-white/5 rounded-xl border border-slate-300 dark:border-white/5 overflow-hidden transition-all shadow-sm dark:shadow-none">
                        
                        {{-- Botón Cabecera Nivel --}}
                        <button onclick="toggleNivel('lista-{{ $loop->index }}', 'chevron-lista-{{ $loop->index }}')" class="w-full flex items-center justify-between px-5 py-4 hover:bg-slate-50 dark:hover:bg-white/5 transition-colors group">
                            <div class="flex items-center gap-4">
                                <div class="w-10 h-10 rounded-lg bg-indigo-50 dark:bg-indigo-500/20 text-indigo-600 dark:text-indigo-400 border border-indigo-100 dark:border-transparent flex items-center justify-center group-hover:scale-110 transition-transform">
                                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z"></path></svg>
                                </div>
                                <div class="text-left">
                                    <span class="block font-bold text-slate-800 dark:text-white text-sm">{{ $nivel }}</span>
                                    <span class="block text-[10px] text-slate-500 font-bold uppercase tracking-widest mt-0.5">
                                        {{ $documentos->count() }} {{ $documentos->count() === 1 ? 'Archivo' : 'Archivos' }}
                                    </span>
                                </div>
                            </div>
                            <div class="w-8 h-8 rounded-full bg-slate-100 dark:bg-black/30 flex items-center justify-center">
                                <svg id="chevron-lista-{{ $loop->index }}" class="w-4 h-4 text-slate-500 transition-transform duration-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/>
                                </svg>
                            </div>
                        </button>

                        {{-- Lista Contenido --}}
                        <div id="lista-{{ $loop->index }}" class="hidden border-t border-slate-200 dark:border-white/5 bg-slate-50 dark:bg-black/20 px-3 py-3 space-y-2">
                            @foreach($documentos as $doc)
                                <a href="/operativo/documento/{{ $doc['storage_path'] }}" class="flex items-center justify-between p-3 rounded-xl bg-transparent hover:bg-white dark:hover:bg-white/10 transition-all hover:-translate-y-0.5 group border border-transparent hover:border-slate-300 dark:hover:border-white/10 hover:shadow-sm">
                                    <div class="flex items-center gap-4">
                                        <div class="w-8 h-8 rounded bg-slate-200 dark:bg-white/5 flex items-center justify-center text-slate-500 group-hover:text-indigo-600 group-hover:bg-indigo-100 dark:group-hover:bg-indigo-500/20 transition-colors">
                                            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z"/>
                                            </svg>
                                        </div>
                                        <div>
                                            <p class="text-sm font-bold text-slate-700 dark:text-slate-200 group-hover:text-indigo-600 dark:group-hover:text-indigo-400 transition-colors">
                                                {{ $doc['display_name'] }}
                                            </p>
                                            <div class="flex items-center gap-2 mt-1">
                                                <span class="text-[10px] font-mono font-bold text-slate-500 dark:text-slate-400 bg-slate-200 dark:bg-black/30 px-1.5 py-0.5 rounded">{{ $doc['metadata']['codigo'] }}</span>
                                                @if(!empty($doc['metadata']['norma']))
                                                    <span class="text-[10px] font-medium text-slate-500">{{ $doc['metadata']['norma'] }}</span>
                                                @endif
                                            </div>
                                        </div>
                                    </div>
                                    <div class="flex items-center gap-4">
                                        @if($doc['metadata']['owner'])
                                            <span class="hidden sm:block text-[10px] font-bold uppercase tracking-widest text-slate-500 bg-white dark:bg-white/5 border border-slate-300 dark:border-white/10 px-2 py-1 rounded-md shadow-sm dark:shadow-none">
                                                Propiedad: {{ $doc['metadata']['owner'] }}
                                            </span>
                                        @endif
                                        <div class="w-8 h-8 rounded-full bg-slate-100 dark:bg-white/5 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity border border-slate-200 dark:border-transparent">
                                            <svg class="w-4 h-4 text-indigo-600 dark:text-indigo-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
                                        </div>
                                    </div>
                                </a>
                            @endforeach
                        </div>
                    </div>

                @empty
                    <div class="text-center py-20 px-6">
                        <div class="w-20 h-20 mx-auto bg-slate-200 dark:bg-white/5 rounded-full flex items-center justify-center mb-6 shadow-inner">
                            <svg class="w-10 h-10 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
                            </svg>
                        </div>
                        <h3 class="text-lg font-bold text-slate-800 dark:text-white mb-2 font-display">Bandeja Vacía</h3>
                        <p class="text-sm font-medium text-slate-500 dark:text-slate-400 max-w-sm mx-auto">Tu departamento actualmente no tiene documentos aprobados o liberados para visualización.</p>
                    </div>
                @endforelse

            </div>
        </div>
    </main>

    <script>
        function toggleNivel(listaId, chevronId) {
            const lista = document.getElementById(listaId);
            const chevron = document.getElementById(chevronId);
            
            if (lista.classList.contains('hidden')) {
                lista.classList.remove('hidden');
                chevron.classList.add('rotate-180');
            } else {
                lista.classList.add('hidden');
                chevron.classList.remove('rotate-180');
            }
        }

        const buscador = document.getElementById('buscador');
        const resultados = document.getElementById('autocomplete-results');
        let debounceTimer;

        buscador.addEventListener('input', function () {
            clearTimeout(debounceTimer);
            const q = this.value.trim();

            if (q.length < 2) {
                ocultarResultados();
                return;
            }

            resultados.innerHTML = `
                <div class="p-6 flex items-center justify-center gap-3 text-slate-500 dark:text-slate-400">
                    <svg class="w-5 h-5 animate-spin text-indigo-500" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
                    <span class="text-xs font-bold uppercase tracking-widest">Buscando...</span>
                </div>
            `;
            mostrarResultados();

            debounceTimer = setTimeout(async () => {
                try {
                    const res = await fetch(`/operativo/buscar?q=${encodeURIComponent(q)}`);
                    const data = await res.json();

                    if (!data.results || data.results.length === 0) {
                        resultados.innerHTML = `
                            <div class="p-6 text-center text-slate-600 dark:text-slate-500">
                                <p class="text-sm font-bold">No se encontraron coincidencias para "${q}"</p>
                            </div>
                        `;
                        return;
                    }

                    resultados.innerHTML = data.results.map(doc => `
                        <a href="/operativo/documento/${doc.storage_path}" class="flex items-center justify-between px-5 py-4 hover:bg-slate-100 dark:hover:bg-white/5 transition-colors border-b border-slate-200 dark:border-white/5 last:border-0 group">
                            <div class="flex items-center gap-4">
                                <div class="w-10 h-10 rounded-lg bg-indigo-50 dark:bg-indigo-500/10 border border-indigo-100 dark:border-transparent flex items-center justify-center text-indigo-600 dark:text-indigo-400">
                                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z"/></svg>
                                </div>
                                <div>
                                    <p class="text-sm font-bold text-slate-800 dark:text-slate-200 group-hover:text-indigo-600 dark:group-hover:text-indigo-400 transition-colors">${doc.display_name}</p>
                                    <div class="flex items-center gap-2 mt-1">
                                        <span class="text-[10px] font-mono font-bold text-slate-500 bg-slate-200 dark:bg-black/30 px-1.5 rounded">${doc.metadata.codigo}</span>
                                        <span class="text-[10px] font-medium text-slate-500">${doc.metadata.nivel}</span>
                                    </div>
                                </div>
                            </div>
                            <div class="w-8 h-8 rounded-full bg-slate-200 dark:bg-white/5 flex items-center justify-center group-hover:translate-x-1 transition-transform">
                                <svg class="w-4 h-4 text-slate-500 group-hover:text-indigo-600 dark:group-hover:text-indigo-400 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
                            </div>
                        </a>
                    `).join('');
                } catch (error) {
                    resultados.innerHTML = `<div class="p-4 text-center text-red-500 text-sm font-bold">Error en la comunicación con el servidor.</div>`;
                }
            }, 350);
        });

        function mostrarResultados() {
            resultados.classList.remove('hidden');
            setTimeout(() => {
                resultados.classList.remove('scale-95', 'opacity-0');
                resultados.classList.add('scale-100', 'opacity-100');
            }, 10);
        }

        function ocultarResultados() {
            resultados.classList.remove('scale-100', 'opacity-100');
            resultados.classList.add('scale-95', 'opacity-0');
            setTimeout(() => { resultados.classList.add('hidden'); }, 150);
        }

        document.addEventListener('click', function (e) {
            if (!buscador.contains(e.target) && !resultados.contains(e.target)) {
                ocultarResultados();
            }
        });

        const canvas = document.getElementById('particles');
        const ctx = canvas.getContext('2d');
        let W, H, particles = [];
        let mouse = { x: null, y: null, radius: 150 };

        window.addEventListener('mousemove', (e) => { mouse.x = e.x; mouse.y = e.y; });
        function resize() { W = canvas.width = window.innerWidth; H = canvas.height = window.innerHeight; init(); }
        
        class Particle {
            constructor() {
                this.x = Math.random() * W; this.y = Math.random() * H;
                this.r = Math.random() * 2 + 0.5; this.density = (Math.random() * 30) + 1;
                this.opacity = Math.random() * 0.5 + 0.1;
                this.vx = (Math.random() - 0.5) * 0.4; this.vy = (Math.random() - 0.5) * 0.4;
            }
            draw(color) {
                ctx.beginPath(); ctx.arc(this.x, this.y, this.r, 0, Math.PI * 2);
                ctx.fillStyle = `rgba(${color}, ${this.opacity})`; ctx.fill();
            }
            update() {
                this.x += this.vx; this.y += this.vy;
                if (this.x < 0 || this.x > W) this.vx *= -1;
                if (this.y < 0 || this.y > H) this.vy *= -1;
                let dx = mouse.x - this.x; let dy = mouse.y - this.y;
                let distance = Math.sqrt(dx*dx + dy*dy);
                if (distance < mouse.radius) {
                    const force = (mouse.radius - distance) / mouse.radius;
                    this.x -= (dx / distance) * force * this.density; this.y -= (dy / distance) * force * this.density;
                }
            }
        }

        function init() {
            particles = []; const count = Math.floor((W * H) / 9000);
            for (let i = 0; i < count; i++) particles.push(new Particle());
        }

        function animate() {
            ctx.clearRect(0, 0, W, H);
            const isDark = document.documentElement.classList.contains('dark');
            const color = isDark ? '255, 255, 255' : '99, 102, 241';
            particles.forEach(p => { p.update(); p.draw(color); });
            requestAnimationFrame(animate);
        }

        window.addEventListener('resize', resize);
        resize(); animate();
    </script>
</body>
</html>