import httpx
from app.core.database import get_db
from app.services.document_listener import handle_documento_aprobado

DOTNET_API = "http://dotnet-service:80"

async def sync_check():
    print("🔄 Iniciando sync_check...", flush=True)
    try:
        # 1. Obtener todos los aprobados desde C#
        async with httpx.AsyncClient(timeout=10) as client:
            response = await client.get(f"{DOTNET_API}/api/documentos")
            if response.status_code != 200:
                print(f"❌ sync_check: C# no respondió correctamente", flush=True)
                return
            aprobados = response.json()

        # 2. Obtener version_ids ya indexados en MongoDB
        db = get_db()
        collection = db["documentos_indexados"]
        indexados = await collection.distinct("version_id")
        indexados_set = set(str(v) for v in indexados)

        # 3. Comparar y procesar faltantes
        faltantes = [
            doc for doc in aprobados
            if str(doc["versionId"]) not in indexados_set
        ]

        if not faltantes:
            print("✅ sync_check: Todos los documentos están indexados", flush=True)
            return

        print(f"⚠️ sync_check: {len(faltantes)} documento(s) sin indexar, procesando...", flush=True)

        for doc in faltantes:
            await handle_documento_aprobado(doc["versionId"])

        print("✅ sync_check completo", flush=True)

    except Exception as e:
        print(f"❌ sync_check error: {e}", flush=True)