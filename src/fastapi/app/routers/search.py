from fastapi import APIRouter, Query
from app.core.database import get_db

router = APIRouter(prefix="/search", tags=["search"])

@router.get("/autocomplete")
async def autocomplete(
    q: str = Query(..., min_length=1),
    departamento: str = Query(...)
):
    db = get_db()
    collection = db["documentos_indexados"]
    q_lower = q.lower().strip()

    cursor = collection.find(
        {
            "metadata.departamento": departamento,
            "search_tokens": {"$regex": f"^{q_lower}"}
        },
        {
            "_id": 0,
            "display_name": 1,
            "metadata.codigo": 1,
            "metadata.nivel": 1,
            "metadata.norma": 1,
            "metadata.owner": 1,
            "metadata.approved_at": 1,
            "storage_path": 1
        }
    ).limit(8)

    results = await cursor.to_list(length=8)
    return {"results": results}

@router.get("/documentos")
async def get_documentos(
    departamento: str = Query(...)
):
    db = get_db()
    collection = db["documentos_indexados"]

    cursor = collection.find(
        {"metadata.departamento": departamento},
        {
            "_id": 0,
            "display_name": 1,
            "metadata.codigo": 1,
            "metadata.nivel": 1,
            "metadata.norma": 1,
            "metadata.owner": 1,
            "metadata.approved_at": 1,
            "storage_path": 1
        }
    )

    results = await cursor.to_list(length=None)
    return {"results": results}