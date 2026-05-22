import asyncio
import json
import httpx
import redis.asyncio as aioredis
from app.core.config import settings
from app.core.database import get_db
from app.services.minio_service import download_file_temp
from app.services.metadata_extractor import extract_metadata
from app.services.tokenizer import build_search_tokens
from app.services.index_service import index_document
import os

CHANNEL = "documents:approved"
DOTNET_API = "http://dotnet-service:80"

async def fetch_documento(version_id: int) -> dict | None:
    async with httpx.AsyncClient(timeout=10) as client:
        response = await client.get(f"{DOTNET_API}/api/documentos/{version_id}")
        if response.status_code == 200:
            return response.json()
        print(f"❌ C# no encontró versión {version_id}: {response.status_code}", flush=True)
        return None

async def handle_documento_aprobado(version_id: int):
    data = await fetch_documento(version_id)
    if not data:
        return

    print(f"📄 Indexando: {data.get('codigoDocumento')}", flush=True)
    tmp_path = None
    try:
        tmp_path = await download_file_temp(data["storagePath"])
        metadata = extract_metadata(tmp_path, data["storagePath"])
        search_tokens = build_search_tokens(data, metadata)
        await index_document(data, metadata, search_tokens)
    except Exception as e:
        print(f"❌ Error indexando: {e}", flush=True)
    finally:
        if tmp_path and os.path.exists(tmp_path):
            os.remove(tmp_path)
            print(f"🧹 Archivo temporal eliminado", flush=True)

async def start_listener():
    redis = aioredis.from_url(
        f"redis://{settings.redis_host}:{settings.redis_port}",
        decode_responses=True
    )
    pubsub = redis.pubsub()
    await pubsub.subscribe(CHANNEL)
    print(f"✅ Escuchando canal: {CHANNEL}", flush=True)

    while True:
        message = await pubsub.get_message(
            ignore_subscribe_messages=True,
            timeout=1.0
        )
        if message:
            try:
                data = json.loads(message["data"])
                tipo = data.get("Tipo")
                
                if tipo == "documento_aprobado":
                    version_id = data.get("versionId") or data.get("VersionId")
                    if version_id:
                        await handle_documento_aprobado(int(version_id))
                elif tipo == "sync_check":
                    # TODO: verificar documentos sin indexar
                    pass

            except Exception as e:
                print(f"❌ Error procesando mensaje: {e}", flush=True)

        await asyncio.sleep(0.01)