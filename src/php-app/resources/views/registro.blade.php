<!DOCTYPE html>
<html lang="es" class="scroll-smooth antialiased dark">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>NormaQ - Registro de Usuario</title>
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
                        'dark-bg': '#0a0a0a',
                        'dark-panel': '#111111',
                        'dark-border': '#1f1f1f',
                        'dark-input': '#161616',
                        'brand-purple': '#9333ea',
                        'brand-purple-hover': '#7e22ce',
                    },
                    animation: {
                        'float': 'float 6s ease-in-out infinite',
                        'float-delayed': 'float 6s ease-in-out infinite 3s',
                    },
                    keyframes: {
                        float: {
                            '0%, 100%': { transform: 'translateY(0)' },
                            '50%': { transform: 'translateY(-10px)' },
                        }
                    }
                }
            }
        }

        // View Transitions API: Efecto premium en ambas direcciones corregido
        function toggleTheme(event) {
            const isCurrentlyDark = document.documentElement.classList.contains('dark');
            
            // Si el navegador no soporta la API, hace el cambio normal
            if (!document.startViewTransition) {
                document.documentElement.classList.toggle('dark');
                updateToggleText(!isCurrentlyDark);
                return;
            }

            // Calculamos desde dónde hizo clic el usuario para el "Efecto Gota"
            const x = event?.clientX ?? window.innerWidth / 2;
            const y = event?.clientY ?? window.innerHeight / 2;
            const endRadius = Math.hypot(
                Math.max(x, window.innerWidth - x),
                Math.max(y, window.innerHeight - y)
            );

            // Iniciamos la transición fluida
            const transition = document.startViewTransition(() => {
                document.documentElement.classList.toggle('dark');
                updateToggleText(!isCurrentlyDark);
            });

            // El secreto para que funcione en ambas direcciones: 
            // SIEMPRE animamos la vista NUEVA expandiéndose, sin importar si es clara u oscura.
            transition.ready.then(() => {
                const clipPath = [
                    `circle(0px at ${x}px ${y}px)`,
                    `circle(${endRadius}px at ${x}px ${y}px)`
                ];

                document.documentElement.animate(
                    {
                        clipPath: clipPath,
                    },
                    {
                        duration: 600,
                        easing: "ease-in-out",
                        pseudoElement: "::view-transition-new(root)",
                    }
                );
            });
        }

        function updateToggleText(willBeDark) {
            const textEl = document.getElementById('toggle-text');
            const iconEl = document.getElementById('toggle-icon');
            if(willBeDark) {
                textEl.textContent = 'Modo oscuro';
                iconEl.className = 'w-4 h-4 rounded-full bg-yellow-500 shadow-[0_0_10px_rgba(234,179,8,0.5)]';
            } else {
                textEl.textContent = 'Modo claro';
                iconEl.className = 'w-4 h-4 rounded-full bg-orange-400';
            }
        }
    </script>

    <style>
        /* Desactiva el fundido por defecto de la API para usar nuestro círculo */
        ::view-transition-old(root),
        ::view-transition-new(root) {
            animation: none;
            mix-blend-mode: normal;
        }

        /* Garantiza que la nueva capa (ya sea clara u oscura) siempre aparezca por encima expandiéndose */
        ::view-transition-old(root) {
            z-index: 1;
        }
        ::view-transition-new(root) {
            z-index: 9999;
        }
    </style>
</head>
<body class="bg-[#F8FAFC] dark:bg-dark-bg text-slate-900 dark:text-white font-sans min-h-screen flex flex-col relative overflow-x-hidden transition-colors duration-200">

    <!-- Toggle de Modo Claro/Oscuro -->
    <div class="absolute top-6 right-6 lg:top-8 lg:right-12 z-50">
        <button onclick="toggleTheme(event)" class="flex items-center gap-2 px-4 py-2.5 rounded-full border border-slate-200 dark:border-dark-border bg-white/80 dark:bg-dark-panel/80 backdrop-blur-md cursor-pointer hover:bg-slate-100 dark:hover:bg-dark-border transition-all shadow-sm">
            <div id="toggle-icon" class="w-4 h-4 rounded-full bg-yellow-500 shadow-[0_0_10px_rgba(234,179,8,0.5)]"></div>
            <span id="toggle-text" class="text-sm text-slate-600 dark:text-gray-300 font-medium">Modo oscuro</span>
        </button>
    </div>

    <div class="flex flex-col-reverse lg:flex-row flex-grow w-full min-h-screen">

        <!-- ========================================== -->
        <!-- MITAD IZQUIERDA: VISUAL (NUEVO USUARIO)      -->
        <!-- ========================================== -->
        <div class="hidden lg:flex w-1/2 items-center justify-center p-8 xl:p-16 relative bg-slate-100 dark:bg-[#0f0f0f] border-r border-slate-200 dark:border-dark-border overflow-hidden">
            
            <div class="absolute inset-0 flex items-center justify-center pointer-events-none">
                <div class="w-[500px] h-[500px] bg-brand-purple/10 dark:bg-brand-purple/5 rounded-full blur-[100px]"></div>
            </div>

            <div class="relative w-full max-w-[450px] h-[500px] flex items-center justify-center">
                
                <!-- Tarjeta 1: Atrás -->
                <div class="absolute top-16 -left-4 animate-float-delayed z-0 opacity-80 dark:opacity-60">
                    <div class="w-64 bg-white dark:bg-dark-panel border border-slate-200 dark:border-dark-border rounded-2xl p-5 shadow-xl transform -rotate-6">
                        <div class="flex items-center gap-3 mb-4">
                            <div class="w-8 h-8 rounded bg-slate-100 dark:bg-gray-800 flex items-center justify-center text-slate-500 dark:text-gray-400">
                                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"></path></svg>
                            </div>
                            <p class="text-sm font-semibold text-slate-700 dark:text-gray-300">Permisos del Sistema</p>
                        </div>
                        <div class="space-y-3">
                            <div class="flex items-center justify-between">
                                <div class="h-2 w-24 bg-slate-200 dark:bg-gray-700 rounded"></div>
                                <div class="w-8 h-4 rounded-full bg-brand-purple/20 flex items-center p-0.5"><div class="w-3 h-3 rounded-full bg-brand-purple ml-auto"></div></div>
                            </div>
                            <div class="flex items-center justify-between">
                                <div class="h-2 w-16 bg-slate-200 dark:bg-gray-700 rounded"></div>
                                <div class="w-8 h-4 rounded-full bg-slate-200 dark:bg-gray-700 flex items-center p-0.5"><div class="w-3 h-3 rounded-full bg-slate-400 dark:bg-gray-500"></div></div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Tarjeta 2: Frente -->
                <div class="relative z-10 animate-float">
                    <div class="w-80 bg-white dark:bg-dark-panel border border-slate-200 dark:border-dark-border rounded-2xl p-6 shadow-2xl dark:shadow-[0_20px_50px_rgba(0,0,0,0.5)]">
                        <div class="flex flex-col items-center mb-6">
                            <div class="w-20 h-20 rounded-full bg-[#F8FAFC] dark:bg-dark-bg border-2 border-brand-purple p-1 mb-4 shadow-[0_0_15px_rgba(147,51,234,0.2)] dark:shadow-[0_0_15px_rgba(147,51,234,0.4)]">
                                <div class="w-full h-full rounded-full bg-brand-purple/10 dark:bg-dark-input flex items-center justify-center text-brand-purple">
                                    <svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
                                </div>
                            </div>
                            <h3 class="text-lg font-semibold text-slate-800 dark:text-white">Nuevo Operador</h3>
                            <p class="text-sm text-slate-500 dark:text-gray-500">Configurando espacio...</p>
                        </div>
                        
                        <div class="space-y-4">
                            <div class="w-full bg-slate-50 dark:bg-dark-bg rounded-lg p-3 border border-slate-100 dark:border-dark-border flex items-center justify-between">
                                <div class="flex flex-col gap-1.5">
                                    <div class="h-2 w-20 bg-slate-200 dark:bg-gray-700 rounded"></div>
                                    <div class="h-2 w-32 bg-slate-300 dark:bg-gray-800 rounded"></div>
                                </div>
                                <div class="px-2 py-1 rounded-md bg-emerald-100 dark:bg-emerald-500/10 border border-emerald-200 dark:border-emerald-500/20 text-emerald-600 dark:text-emerald-500 text-[10px] font-bold flex items-center gap-1.5">
                                    <div class="w-1.5 h-1.5 rounded-full bg-emerald-500"></div> Validando
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

            </div>
        </div>

        <!-- ========================================== -->
        <!-- MITAD DERECHA: FORMULARIO DE REGISTRO        -->
        <!-- ========================================== -->
        <div class="w-full lg:w-1/2 flex flex-col justify-center px-6 sm:px-12 xl:px-24 py-12 relative bg-white dark:bg-dark-bg">
            
            <div class="w-full max-w-md mx-auto">
                
                <!-- Logo Top Left Corregido (Eliminado 'absolute' para que fluya) -->
                <div class="flex items-center gap-3 mb-8 mt-8 lg:mt-0">
                    <div class="w-8 h-8 rounded-lg bg-brand-purple flex items-center justify-center text-white font-bold text-sm shadow-[0_0_10px_rgba(147,51,234,0.3)]">
                        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
                    </div>
                    <span class="text-xl font-bold tracking-tight text-slate-800 dark:text-white">NormaQ</span>
                </div>
                
                <!-- Encabezado -->
                <div class="mb-8">
                    <p class="text-sm font-medium text-slate-500 dark:text-gray-400 mb-1">Únete a la plataforma</p>
                    <h1 class="text-3xl sm:text-4xl font-bold tracking-tight mb-2 text-slate-900 dark:text-white">
                        Crea tu cuenta para tu <br><span class="text-brand-purple">panel de gestión ISO</span>
                    </h1>
                    <p class="text-slate-500 dark:text-gray-400 text-sm">
                        Ingresa tus datos para habilitar tu espacio normativo.
                    </p>
                </div>

                <!-- FORMULARIO CORREGIDO -->
                <form class="space-y-4" action="#" method="POST">
                    
                    <div>
                        <label class="block text-sm font-medium text-slate-700 dark:text-gray-300 mb-1.5">Nombre completo</label>
                        <div class="relative">
                            <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none">
                                <svg class="h-5 w-5 text-slate-400 dark:text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
                            </div>
                            <input type="text" placeholder="Ej. Edwin Montes" class="w-full pl-10 pr-4 py-3 bg-slate-50 dark:bg-dark-input border border-slate-200 dark:border-dark-border rounded-xl text-slate-900 dark:text-white placeholder-slate-400 dark:placeholder-gray-600 focus:outline-none focus:border-brand-purple focus:ring-1 focus:ring-brand-purple transition-all text-sm">
                        </div>
                    </div>

                    <div>
                        <label class="block text-sm font-medium text-slate-700 dark:text-gray-300 mb-1.5">Correo electrónico</label>
                        <div class="relative">
                            <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none">
                                <svg class="h-5 w-5 text-slate-400 dark:text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"></path></svg>
                            </div>
                            <input type="email" placeholder="edwinmonte@empresa.com" class="w-full pl-10 pr-4 py-3 bg-slate-50 dark:bg-dark-input border border-slate-200 dark:border-dark-border rounded-xl text-slate-900 dark:text-white placeholder-slate-400 dark:placeholder-gray-600 focus:outline-none focus:border-brand-purple focus:ring-1 focus:ring-brand-purple transition-all text-sm">
                        </div>
                    </div>

                    <div>
                        <label class="block text-sm font-medium text-slate-700 dark:text-gray-300 mb-1.5">Departamento</label>
                        <div class="relative">
                            <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none">
                                <svg class="h-5 w-5 text-slate-400 dark:text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"></path></svg>
                            </div>
                            
                            <select class="w-full pl-10 pr-10 py-3 bg-slate-50 dark:bg-dark-input border border-slate-200 dark:border-dark-border rounded-xl text-slate-600 dark:text-gray-300 focus:outline-none focus:border-brand-purple focus:ring-1 focus:ring-brand-purple transition-all text-sm appearance-none cursor-pointer">
                                <option value="" disabled selected class="text-slate-400">-- Seleccione un Departamento --</option>
                                <option value="calidad" class="bg-white dark:bg-dark-panel text-slate-800 dark:text-white">Calidad</option>
                                <option value="operaciones" class="bg-white dark:bg-dark-panel text-slate-800 dark:text-white">Operaciones</option>
                                <option value="auditoria" class="bg-white dark:bg-dark-panel text-slate-800 dark:text-white">Auditoría Interna</option>
                                <option value="sistemas" class="bg-white dark:bg-dark-panel text-slate-800 dark:text-white">Sistemas / IT</option>
                            </select>
                            
                            <div class="absolute inset-y-0 right-0 pr-4 flex items-center pointer-events-none">
                                <svg class="h-4 w-4 text-slate-400 dark:text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path></svg>
                            </div>
                        </div>
                    </div>

                    <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                        <div>
                            <label class="block text-sm font-medium text-slate-700 dark:text-gray-300 mb-1.5">Contraseña</label>
                            <div class="relative">
                                <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none">
                                    <svg class="h-5 w-5 text-slate-400 dark:text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"></path></svg>
                                </div>
                                <input type="password" placeholder="••••••••" class="w-full pl-10 pr-4 py-3 bg-slate-50 dark:bg-dark-input border border-slate-200 dark:border-dark-border rounded-xl text-slate-900 dark:text-white placeholder-slate-400 dark:placeholder-gray-600 focus:outline-none focus:border-brand-purple focus:ring-1 focus:ring-brand-purple transition-all text-sm">
                            </div>
                        </div>

                        <div>
                            <label class="block text-sm font-medium text-slate-700 dark:text-gray-300 mb-1.5">Confirmar</label>
                            <div class="relative">
                                <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none">
                                    <svg class="h-5 w-5 text-slate-400 dark:text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"></path></svg>
                                </div>
                                <input type="password" placeholder="••••••••" class="w-full pl-10 pr-4 py-3 bg-slate-50 dark:bg-dark-input border border-slate-200 dark:border-dark-border rounded-xl text-slate-900 dark:text-white placeholder-slate-400 dark:placeholder-gray-600 focus:outline-none focus:border-brand-purple focus:ring-1 focus:ring-brand-purple transition-all text-sm">
                            </div>
                        </div>
                    </div>

                    <div class="flex items-center text-sm pt-2 mb-4">
                        <label class="flex items-center gap-3 cursor-pointer group">
                            <div class="relative flex items-center justify-center w-5 h-5 rounded border border-slate-300 dark:border-dark-border bg-white dark:bg-dark-input group-hover:border-brand-purple transition-colors">
                                <input type="checkbox" class="opacity-0 absolute w-full h-full cursor-pointer peer">
                                <svg class="w-3 h-3 text-brand-purple opacity-0 peer-checked:opacity-100 transition-opacity" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="3"><path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7"></path></svg>
                            </div>
                            <span class="text-slate-600 dark:text-gray-400">Acepto los <a href="#" class="text-brand-purple hover:underline">términos de servicio</a> y normativas.</span>
                        </label>
                    </div>

                    <div class="pt-2">
                        <button type="submit" class="w-full py-3.5 bg-brand-purple hover:bg-brand-purple-hover text-white rounded-xl font-medium shadow-[0_0_15px_rgba(147,51,234,0.3)] dark:shadow-[0_0_15px_rgba(147,51,234,0.4)] transition-all">
                            Crear Cuenta
                        </button>
                    </div>

                </form>

                <div class="mt-8 text-center pb-8 lg:pb-0">
                    <p class="text-sm text-slate-500 dark:text-gray-400">
                        ¿Ya tienes una cuenta operativa? 
                        <a href="/php-app" class="text-brand-purple font-medium hover:underline ml-1">Inicia sesión aquí</a>
                    </p>
                </div>

            </div>
        </div>

    </div>

</body>
</html>