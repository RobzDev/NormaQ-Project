<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>NormaQ — Gestión Documental ISO</title>
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap" rel="stylesheet">
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }

        :root {
            --bg: #0d0d0f;
            --bg2: #111113;
            --text: #f4f4f5;
            --text2: #71717a;
            --text3: #3f3f46;
            --border: rgba(255,255,255,0.07);
            --accent: #9333ea;
            --accent2: #a855f7;
            --accent-bg: rgba(147,51,234,0.12);
            --accent-border: rgba(147,51,234,0.3);
            --mint: #10b981;
            --mint-text: #34d399;
            --amber-text: #fbbf24;
            --input-bg: #18181b;
            --input-border: rgba(255,255,255,0.08);
            --card: #1c1c1f;
            --card-border: rgba(255,255,255,0.08);
            --placeholder: #3f3f46;
        }

        .light {
            --bg: #fafaf9;
            --bg2: #ffffff;
            --text: #18181b;
            --text2: #71717a;
            --text3: #d4d4d8;
            --border: rgba(0,0,0,0.07);
            --accent: #7c3aed;
            --accent2: #8b5cf6;
            --accent-bg: rgba(124,58,237,0.07);
            --accent-border: rgba(124,58,237,0.2);
            --mint: #059669;
            --mint-text: #059669;
            --amber-text: #d97706;
            --input-bg: #f4f4f5;
            --input-border: rgba(0,0,0,0.09);
            --card: #ffffff;
            --card-border: rgba(0,0,0,0.08);
            --placeholder: #a1a1aa;
        }

        body {
            font-family: 'Inter', sans-serif;
            background: var(--bg);
            color: var(--text);
            height: 100vh;
            overflow: hidden;
            /* Se elimina transition de background aquí para que la API de View Transitions haga la magia */
        }

        /* NAV */
        .nav {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 1rem 2rem;
            border-bottom: 0.5px solid var(--border);
        }
        .logo { display: flex; align-items: center; gap: 9px; }
        .logo-icon {
            width: 28px; height: 28px;
            background: var(--accent);
            border-radius: 7px;
            display: flex; align-items: center; justify-content: center;
            box-shadow: 0 0 14px rgba(147,51,234,0.5);
        }
        .logo-icon svg { width: 15px; height: 15px; stroke: #fff; fill: none; stroke-width: 2.2; }
        .logo-text { font-size: 15px; font-weight: 700; letter-spacing: -.01em; }
        .toggle {
            display: flex; align-items: center; gap: 6px;
            padding: 5px 12px;
            background: transparent;
            border: 0.5px solid var(--border);
            border-radius: 20px;
            cursor: pointer;
            font-size: 12px;
            color: var(--text2);
            font-family: 'Inter', sans-serif;
            transition: all .2s;
        }
        .toggle:hover { border-color: var(--accent-border); color: var(--text); }

        /* LAYOUT */
        .layout {
            display: grid;
            grid-template-columns: 1fr 1fr;
            height: calc(100vh - 53px);
        }

        /* LEFT */
        .left {
            padding: 0 3.5rem;
            display: flex;
            flex-direction: column;
            justify-content: center;
            border-right: 0.5px solid var(--border);
        }
        .greeting { font-size: 13px; color: var(--text2); margin-bottom: .6rem; }
        .left h1 {
            font-size: 32px; font-weight: 800;
            line-height: 1.15;
            letter-spacing: -.025em;
            margin-bottom: .5rem;
        }
        .left h1 span { color: var(--accent2); }
        .subtitle { font-size: 13px; color: var(--text2); margin-bottom: 2rem; line-height: 1.6; }

        .field { margin-bottom: 1rem; }
        .field label { display: block; font-size: 12px; font-weight: 500; color: var(--text2); margin-bottom: .4rem; }
        .input-wrap {
            display: flex; align-items: center; gap: 10px;
            background: var(--input-bg);
            border: 0.5px solid var(--input-border);
            border-radius: 10px;
            padding: 10px 14px;
            transition: border .2s;
        }
        .input-wrap:focus-within { border-color: var(--accent-border); }
        .input-wrap svg { width: 15px; height: 15px; stroke: var(--placeholder); fill: none; stroke-width: 1.8; flex-shrink: 0; }
        .input-wrap input {
            background: transparent; border: none; outline: none;
            font-size: 13px; color: var(--text);
            font-family: 'Inter', sans-serif; width: 100%;
        }
        .input-wrap input::placeholder { color: var(--placeholder); }

        .row { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
        .remember { display: flex; align-items: center; gap: 6px; font-size: 12px; color: var(--text2); cursor: pointer; }
        .remember input { accent-color: var(--accent); }
        .forgot { font-size: 12px; color: var(--accent2); cursor: pointer; text-decoration: none; }
        .forgot:hover { text-decoration: underline; }

        .btn-main {
            width: 100%; padding: 11px;
            background: var(--accent); color: #fff;
            border: none; border-radius: 10px;
            font-size: 14px; font-weight: 600;
            cursor: pointer; font-family: 'Inter', sans-serif;
            box-shadow: 0 4px 20px rgba(147,51,234,0.35);
            transition: opacity .2s;
            margin-bottom: 1rem;
        }
        .btn-main:hover { opacity: .88; }

        .divider {
            display: flex; align-items: center; gap: 10px;
            color: var(--text3); font-size: 11px; margin-bottom: 1rem;
        }
        .divider::before, .divider::after { content: ''; flex: 1; height: 0.5px; background: var(--border); }

        .btn-sso {
            width: 100%; padding: 11px;
            background: transparent; color: var(--text2);
            border: 0.5px solid var(--border);
            border-radius: 10px; font-size: 13px; font-weight: 500;
            cursor: pointer; font-family: 'Inter', sans-serif;
            transition: all .2s;
        }
        .btn-sso:hover { border-color: var(--accent-border); color: var(--text); }

        /* RIGHT */
        .right {
            background: var(--bg2);
            display: flex; align-items: center; justify-content: center;
            position: relative; overflow: hidden;
        }
        .right::before {
            content: '';
            position: absolute;
            width: 400px; height: 400px;
            background: radial-gradient(circle, rgba(147,51,234,0.08) 0%, transparent 70%);
            top: 50%; left: 50%;
            transform: translate(-50%, -50%);
            pointer-events: none;
        }

        /* TARJETAS FLOTANTES */
        .doc-scene { position: relative; width: 320px; height: 340px; }
        .doc-card {
            position: absolute;
            background: var(--card);
            border: 0.5px solid var(--card-border);
            border-radius: 14px;
            padding: 1rem 1.25rem;
            box-shadow: 0 8px 32px rgba(0,0,0,0.35);
            transition: background .3s, border .3s;
        }
        .doc-card.main {
            width: 220px; top: 50%; left: 50%;
            transform: translate(-50%, -50%);
            z-index: 3;
            animation: float1 4s ease-in-out infinite;
        }
        .doc-card.back1 {
            width: 190px; top: 15%; left: 5%;
            z-index: 1; opacity: .6;
            animation: float2 5s ease-in-out infinite;
        }
        .doc-card.back2 {
            width: 180px; bottom: 10%; right: 0;
            z-index: 2; opacity: .75;
            animation: float3 4.5s ease-in-out infinite;
        }

        @keyframes float1 {
            0%, 100% { transform: translate(-50%, -50%) translateY(0); }
            50% { transform: translate(-50%, -50%) translateY(-10px); }
        }
        @keyframes float2 {
            0%, 100% { transform: translateY(0) rotate(-4deg); }
            50% { transform: translateY(-8px) rotate(-4deg); }
        }
        @keyframes float3 {
            0%, 100% { transform: translateY(0) rotate(3deg); }
            50% { transform: translateY(-6px) rotate(3deg); }
        }

        .doc-top { display: flex; align-items: center; gap: 8px; margin-bottom: .75rem; }
        .doc-icon {
            width: 28px; height: 28px; border-radius: 7px;
            display: flex; align-items: center; justify-content: center;
        }
        .doc-icon svg { width: 14px; height: 14px; fill: none; stroke-width: 2; }
        .di-purple { background: rgba(147,51,234,0.15); }
        .di-purple svg { stroke: var(--accent2); }
        .di-mint { background: rgba(16,185,129,0.12); }
        .di-mint svg { stroke: var(--mint-text); }
        .di-amber { background: rgba(245,158,11,0.12); }
        .di-amber svg { stroke: var(--amber-text); }
        .doc-name { font-size: 12px; font-weight: 600; color: var(--text); }
        .doc-meta { font-size: 11px; color: var(--text2); }
        .doc-lines { display: flex; flex-direction: column; gap: 5px; }
        .line { height: 5px; border-radius: 3px; background: var(--border); }
        .line.l1 { width: 100%; }
        .line.l2 { width: 75%; }
        .line.l3 { width: 55%; }
        .doc-badge {
            display: inline-flex; align-items: center; gap: 4px;
            margin-top: .75rem; font-size: 10px; font-weight: 500;
            padding: 3px 9px; border-radius: 5px;
        }
        .badge-green { background: rgba(16,185,129,0.12); color: var(--mint-text); border: 0.5px solid rgba(16,185,129,0.2); }
        .badge-purple { background: rgba(147,51,234,0.12); color: var(--accent2); border: 0.5px solid rgba(147,51,234,0.25); }
        .badge-amber { background: rgba(245,158,11,0.1); color: var(--amber-text); border: 0.5px solid rgba(245,158,11,0.2); }
        .badge-dot { width: 5px; height: 5px; border-radius: 50%; background: currentColor; }

        /* VIEW TRANSITIONS API STYLES */
        ::view-transition-old(root),
        ::view-transition-new(root) {
            animation: none;
            mix-blend-mode: normal;
        }
        ::view-transition-old(root) {
            z-index: 1;
        }
        ::view-transition-new(root) {
            z-index: 9999;
        }
    </style>
</head>
<body id="root">

    <nav class="nav">
        <div class="logo">
            <div class="logo-icon">
                <svg viewBox="0 0 24 24"><path d="M9 12l2 2 4-4M7.835 4.697a3.42 3.42 0 001.946-.806 3.42 3.42 0 014.438 0 3.42 3.42 0 001.946.806 3.42 3.42 0 013.138 3.138 3.42 3.42 0 00.806 1.946 3.42 3.42 0 010 4.438 3.42 3.42 0 00-.806 1.946 3.42 3.42 0 01-3.138 3.138 3.42 3.42 0 00-1.946.806 3.42 3.42 0 01-4.438 0 3.42 3.42 0 00-1.946-.806 3.42 3.42 0 01-3.138-3.138 3.42 3.42 0 00-.806-1.946 3.42 3.42 0 010-4.438 3.42 3.42 0 00.806-1.946 3.42 3.42 0 013.138-3.138z"/></svg>
            </div>
            <span class="logo-text">NormaQ</span>
        </div>
        <button class="toggle" onclick="toggleMode(event)">
            <span id="toggleIcon">☀️</span>
            <span id="toggleLabel">Modo claro</span>
        </button>
    </nav>

    <div class="layout">
        <div class="left">
            <p class="greeting">Bienvenido de nuevo</p>
            <h1>Inicia sesión en tu<br><span>panel de gestión ISO</span></h1>
            <p class="subtitle">Accede a tus documentos normativos y flujos de aprobación.</p>

            <div class="field">
                <label>Correo electrónico</label>
                <div class="input-wrap">
                    <svg viewBox="0 0 24 24"><path d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"/></svg>
                    <input type="email" placeholder="usuario@empresa.com">
                </div>
            </div>

            <div class="field">
                <label>Contraseña</label>
                <div class="input-wrap">
                    <svg viewBox="0 0 24 24"><path d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"/></svg>
                    <input type="password" placeholder="••••••••••••">
                </div>
            </div>

            <div class="row">
                <label class="remember"><input type="checkbox"> Recordarme</label>
                <a href="#" class="forgot">¿Olvidaste tu contraseña?</a>
            </div>

            <button class="btn-main">Iniciar Sesión</button>
            <div class="divider">o continuar con</div>
            <button class="btn-sso">Inicio de Sesión Único (SSO)</button>
        </div>

        <div class="right">
            <div class="doc-scene">

                <div class="doc-card back1">
                    <div class="doc-top">
                        <div class="doc-icon di-amber">
                            <svg viewBox="0 0 24 24"><path d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/></svg>
                        </div>
                        <div>
                            <div class="doc-name">Procedimiento</div>
                            <div class="doc-meta">Rev. 3</div>
                        </div>
                    </div>
                    <div class="doc-lines">
                        <div class="line l1"></div>
                        <div class="line l2"></div>
                    </div>
                    <div class="doc-badge badge-amber"><div class="badge-dot"></div>En revisión</div>
                </div>

                <div class="doc-card main">
                    <div class="doc-top">
                        <div class="doc-icon di-purple">
                            <svg viewBox="0 0 24 24"><path d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/></svg>
                        </div>
                        <div>
                            <div class="doc-name">Manual de Calidad</div>
                            <div class="doc-meta">ISO 9001 · v2.1</div>
                        </div>
                    </div>
                    <div class="doc-lines">
                        <div class="line l1"></div>
                        <div class="line l2"></div>
                        <div class="line l3"></div>
                    </div>
                    <div class="doc-badge badge-green"><div class="badge-dot"></div>Aprobado</div>
                </div>

                <div class="doc-card back2">
                    <div class="doc-top">
                        <div class="doc-icon di-mint">
                            <svg viewBox="0 0 24 24"><path d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/></svg>
                        </div>
                        <div>
                            <div class="doc-name">Normativas ISO</div>
                            <div class="doc-meta">Sincronizado ✓</div>
                        </div>
                    </div>
                    <div class="doc-lines">
                        <div class="line l1"></div>
                        <div class="line l2"></div>
                    </div>
                    <div class="doc-badge badge-purple"><div class="badge-dot"></div>Activo</div>
                </div>

            </div>
        </div>
    </div>

    <script>
        let dark = true;

        function applyTheme(isDark) {
            dark = isDark;
            document.getElementById('root').className = dark ? '' : 'light';
            document.getElementById('toggleIcon').textContent = dark ? '☀️' : '🌙';
            document.getElementById('toggleLabel').textContent = dark ? 'Modo claro' : 'Modo oscuro';
        }

        function toggleMode(event) {
            const isCurrentlyDark = dark;
            
            // Si el navegador no soporta la API, hace el cambio instantáneo
            if (!document.startViewTransition) {
                applyTheme(!isCurrentlyDark);
                return;
            }

            // Calculamos desde dónde hizo clic el usuario
            const x = event?.clientX ?? window.innerWidth / 2;
            const y = event?.clientY ?? window.innerHeight / 2;
            const endRadius = Math.hypot(
                Math.max(x, window.innerWidth - x),
                Math.max(y, window.innerHeight - y)
            );

            // Iniciamos la transición
            const transition = document.startViewTransition(() => {
                applyTheme(!isCurrentlyDark);
            });

            // Animamos la capa nueva expandiéndose
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
    </script>

</body>
</html>