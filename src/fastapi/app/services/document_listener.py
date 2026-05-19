import asyncio
import json
import redis.asyncio as aioredis
from app.services.metadata_extractor import extract_metadata
from app.core.config import settings
from app.services.tokenizer import build_search_tokens
from app.services.index_service import index_document



from app.services.minio_service import download_file_temp
import os

CHANNEL = "documents:approved"


async def handle_document_approved(data: dict):
    print(f"📄 Indexando: {data.get('CodigoDocumento')}", flush=True)
    tmp_path = None
    try:
        tmp_path = await download_file_temp(data["StoragePath"])
        metadata = extract_metadata(tmp_path, data["StoragePath"])
        search_tokens = build_search_tokens(data, metadata)
        await index_document(data, metadata, search_tokens)
    except Exception as e:
        print(f"❌ Error: {e}", flush=True)
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
                await handle_document_approved(data)
            except Exception as e:
                print(f"❌ Error procesando mensaje: {e}", flush=True)

        await asyncio.sleep(0.01)