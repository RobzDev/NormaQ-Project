<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Portal Operativo — QualityDoc</title>
    <script src="https://cdn.tailwindcss.com"></script>
</head>
<body class="bg-gray-950 text-gray-100 min-h-screen">

    {{-- HEADER --}}
    <header class="bg-gray-900 border-b border-gray-800 px-6 py-4 flex items-center justify-between">
        <div class="flex items-center gap-3">
            <div class="w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center">
                <svg class="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
                </svg>
            </div>
            <span class="font-semibold text-white text-lg">QualityDoc</span>
        </div>

        <div class="flex items-center gap-6">
            {{-- Info del usuario --}}
            <div class="text-right">
                <p class="text-sm font-medium text-white">{{ $userName }}</p>
                <p class="text-xs text-gray-400">{{ $departamento }}</p>
            </div>
            <div class="bg-blue-600/20 border border-blue-500/30 text-blue-400 text-xs font-medium px-3 py-1 rounded-full">
                Operario
            </div>

            {{-- Logout --}}
            <form method="POST" action="{{ route('operativo.logout') }}">
                @csrf
                <button type="submit"
                    class="flex items-center gap-2 text-sm text-gray-400 hover:text-red-400 transition-colors">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"/>
                    </svg>
                    Cerrar sesión
                </button>
            </form>
        </div>
    </header>

    <main class="max-w-6xl mx-auto px-6 py-8">

        {{-- BUSCADOR --}}
        <div class="relative mb-8">
            <input
                type="text"
                id="buscador"
                placeholder="Buscar documento..."
                autocomplete="off"
                class="w-full bg-gray-900 border border-gray-700 rounded-xl px-5 py-3 pl-12 text-sm text-gray-100 placeholder-gray-500 focus:outline-none focus:border-blue-500 transition-colors"
            />
            <svg class="w-5 h-5 text-gray-500 absolute left-4 top-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
            </svg>

            {{-- Dropdown autocompletado --}}
            <div id="autocomplete-results"
                class="hidden absolute z-10 w-full mt-1 bg-gray-900 border border-gray-700 rounded-xl overflow-hidden shadow-xl">
            </div>
        </div>

       

        <div class="space-y-4" id="arbol-documentos">
            @forelse($porNivel as $nivel => $documentos)
                @php $info = $niveles[$nivel] ?? ['nombre' => 'Sin clasificar', 'icon' => '📁', 'color' => 'gray']; @endphp

                <div id="nivel-{{ $loop->index }}" class="border-t border-gray-800 divide-y divide-gray-800/50">
                    {{-- Cabecera del nivel (colapsable) --}}
                      <button onclick="toggleNivel('lista-{{ $loop->index }}')"
                        class="w-full flex items-center justify-between px-5 py-4 hover:bg-gray-800/50 transition-colors">
                        <div class="flex items-center gap-3">
                            <span class="text-xl">📁</span>
                            <span class="font-medium text-white">{{ $nivel }}</span>
                            <span class="text-xs bg-gray-800 text-gray-400 px-2 py-0.5 rounded-full">
                                {{ $documentos->count() }} {{ $documentos->count() === 1 ? 'documento' : 'documentos' }}
                            </span>
                        </div>
                        <svg id="chevron-lista-{{ $loop->index }}" class="w-4 h-4 text-gray-500 transition-transform duration-200"
                            fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
                        </svg>
                    </button>

                    {{-- Lista de documentos --}}
                    <div id="lista-{{ $loop->index }}" class="border-t border-gray-800 divide-y divide-gray-800/50">
                        @foreach($documentos as $doc)
                            <a href="/operativo/documento/{{ $doc['storage_path'] }}"
                                class="flex items-center justify-between px-5 py-3 hover:bg-gray-800/40 transition-colors group">
                                <div class="flex items-center gap-3">
                                    <svg class="w-4 h-4 text-gray-500 group-hover:text-blue-400 transition-colors flex-shrink-0"
                                        fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                                            d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z"/>
                                    </svg>
                                    <div>
                                         <p class="text-xs text-gray-500">
                                            {{ $doc['metadata']['codigo'] }} · {{ $doc['metadata']['norma'] ?? '' }}
                                        </p>
                                        <p class="text-sm text-gray-200 group-hover:text-white transition-colors">
                                          {{ $doc['display_name'] }}
                                        </p>
                                    </div>
                                </div>
                                <div class="flex items-center gap-4 text-xs text-gray-500">
                                    @if($doc['metadata']['owner'])
                                        <span>{{ $doc['metadata']['owner'] }}</span>
                                    @endif
                                    <svg class="w-3 h-3 opacity-0 group-hover:opacity-100 transition-opacity text-blue-400"
                                        fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
                                    </svg>
                                </div>
                            </a>
                        @endforeach
                    </div>
                </div>
            @empty
                <div class="text-center py-16 text-gray-500">
                    <svg class="w-12 h-12 mx-auto mb-3 opacity-30" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                            d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
                    </svg>
                    <p class="text-sm">No hay documentos aprobados para tu departamento.</p>
                </div>
            @endforelse
        </div>
    </main>

    <script>
        // Toggle niveles
        function toggleNivel(id) {
            const el = document.getElementById(id);
            const chevron = document.getElementById('chevron-' + id);
            el.classList.toggle('hidden');
            chevron.classList.toggle('rotate-180');
        }

        // Autocompletado con debounce
        const buscador = document.getElementById('buscador');
        const resultados = document.getElementById('autocomplete-results');
        let debounceTimer;

        buscador.addEventListener('input', function () {
            clearTimeout(debounceTimer);
            const q = this.value.trim();

            if (q.length < 2) {
                resultados.classList.add('hidden');
                return;
            }

            debounceTimer = setTimeout(async () => {
                const res = await fetch(`/operativo/buscar?q=${encodeURIComponent(q)}`);
                const data = await res.json();

                if (!data.results || data.results.length === 0) {
                    resultados.classList.add('hidden');
                    return;
                }

                resultados.innerHTML = data.results.map(doc => `
                    <a href="/operativo/documento/${doc.storage_path}"
                        class="flex items-center justify-between px-4 py-3 hover:bg-gray-800 transition-colors border-b border-gray-800/50 last:border-0">
                        <div>
                            <p class="text-sm text-gray-200">${doc.display_name}</p>
                            <p class="text-xs text-gray-500">${doc.metadata.codigo} · ${doc.metadata.nivel}</p>
                        </div>
                        <svg class="w-3 h-3 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
                        </svg>
                    </a>
                `).join('');

                resultados.classList.remove('hidden');
            }, 300);
        });

        // Cerrar dropdown al hacer clic fuera
        document.addEventListener('click', function (e) {
            if (!buscador.contains(e.target) && !resultados.contains(e.target)) {
                resultados.classList.add('hidden');
            }
        });
    </script>
</body>
</html>