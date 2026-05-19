from datetime import datetime, timezone
from app.core.database import get_db

async def index_document(data: dict, metadata: dict, search_tokens: list[str]):
    db = get_db()
    collection = db["documentos_indexados"]

    document = {
        "doc_id": data["DocId"],
        "display_name": data["DisplayName"],
        "search_tokens": search_tokens,
        "metadata": {
            "codigo": data.get("CodigoDocumento"),
            "nivel": data.get("NivelDocumento"),
            "norma": data.get("Norma"),
            "departamento": data.get("Departamento"),
            "owner": data.get("Owner"),
            "approved_by": data.get("ApprovedBy"),
            "approved_at": data.get("ApprovedAt"),
            "extension": metadata.get("extension"),
            "mime_type": metadata.get("mime_type"),
            "file_size_kb": metadata.get("file_size_kb"),
            "page_count": metadata.get("page_count"),
            "sheet_count": metadata.get("sheet_count"),
            "line_count": metadata.get("line_count"),
            "note": metadata.get("note"),
            "hash_sha256": metadata.get("hash_sha256"),
        },
        "storage_path": data["StoragePath"],
        "indexed_at": datetime.now(timezone.utc),
    }

    # Upsert: si el doc_id ya existe lo actualiza, si no lo crea
    await collection.update_one(
        {"doc_id": data["DocId"]},
        {"$set": document},
        upsert=True
    )

    print(f"✅ Documento indexado en MongoDB: {data.get('CodigoDocumento')}", flush=True)