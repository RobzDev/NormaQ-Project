from datetime import datetime, timezone
from app.core.database import get_db

async def index_document(data: dict, metadata: dict, search_tokens: list[str]):
   db = get_db()
   collection = db["documentos_indexados"]

    # Si el nuevo documento es aprobado, marcar versiones anteriores del mismo doc como obsoleto
   if data.get("estado", "").lower() == "aprobado":
        await collection.update_many(
            {
                "doc_id": str(data["documentoId"]),
                "estado": "aprobado"
            },
            {"$set": {"estado": "obsoleto"}}
        )

   document = {
    "doc_id": str(data["documentoId"]),
    "version_id": str(data["versionId"]),
    "estado": data.get("estado", "").lower(),
    "display_name": data["nombreDocumento"],
    "search_tokens": search_tokens,
    "full_text": metadata.get("full_text", ""),  # Guardar el texto completo para búsquedas futuras
    "metadata": {
        "codigo": data.get("codigoDocumento"),
        "nivel": data.get("nivel"),
        "norma": data.get("normaCodigo"),
        "departamento": data.get("departamento"),
        "owner": data.get("owner"),
        "approved_by": data.get("approvedBy"),
        "approved_at": data.get("approvedAt"),
        "extension": metadata.get("extension"),
        "mime_type": metadata.get("mime_type"),
        "file_size_kb": metadata.get("file_size_kb"),
        "page_count": metadata.get("page_count"),
        "sheet_count": metadata.get("sheet_count"),
        "line_count": metadata.get("line_count"),
        "note": metadata.get("note"),
        "hash_sha256": metadata.get("hash_sha256"),
    },
    "storage_path": data["storagePath"],
    "indexed_at": datetime.now(timezone.utc),
}

    # Upsert: si el doc_id ya existe lo actualiza, si no lo crea
   await collection.insert_one(document)


   print(f"✅ Documento indexado en MongoDB: {data.get('codigoDocumento')}", flush=True)