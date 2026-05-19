from app.core.database import get_db

async def create_indexes():
    db = get_db()
    collection = db["documentos_indexados"]

    # Búsqueda por departamento (filtro principal en ambos endpoints)
    await collection.create_index("metadata.departamento")

    # Autocompletado: departamento + tokens
    await collection.create_index([
        ("metadata.departamento", 1),
        ("search_tokens", 1)
    ])

    # Evitar duplicados por doc_id
    await collection.create_index("doc_id", unique=True)

    print("✅ Índices MongoDB creados", flush=True)