from fastapi import APIRouter, Query
from app.core.database import get_db
import re

router = APIRouter(prefix="/search", tags=["search"])

@router.get("/autocomplete")
async def autocomplete(
    q: str = Query(..., min_length=1),
    departamento: str = Query(...)
):
    db = get_db()
    collection = db["documentos_indexados"]
    q_clean = q.strip()

    # Buscamos de forma insensible a mayúsculas dentro de display_name, metadata.codigo o full_text
    # Nota: Si tu base de datos es muy grande, te recomiendo crear un índice de texto en MongoDB 
    # y usar {"$text": {"$search": q_clean}} para máxima eficiencia.
    query_filter = {
        "metadata.departamento": departamento,
        "$or": [
            {"display_name": {"$regex": re.escape(q_clean), "$options": "i"}},
            {"metadata.codigo": {"$regex": re.escape(q_clean), "$options": "i"}},
            {"full_text": {"$regex": re.escape(q_clean), "$options": "i"}}
        ]
    }

    cursor = collection.find(
        query_filter,
        {
            "_id": 0,
            "display_name": 1,
            "metadata.codigo": 1,
            "metadata.nivel": 1,
            "metadata.norma": 1,
            "full_text": 1,
            "estado": 1,
            
            "metadata.owner": 1,
            "storage_path": 1
        }
    ).limit(8)

    results = await cursor.to_list(length=8)
    
    for doc in results:
        # Generar el snippet dinámico con marcas HTML
        doc["snippet"] = build_snippet(doc.get("full_text", ""), q_clean)
        
        # Opcional: Remover el text completo del payload para ahorrar ancho de banda de red
        if "full_text" in doc:
            del doc["full_text"]

    return {"results": results}


def build_snippet(text: str, query: str, radius: int = 60) -> str:
    """
    Busca la query dentro del texto de forma insensible a mayúsculas,
    extrae un fragmento a su alrededor y resalta la coincidencia con <mark>.
    """
    if not text or not query:
        return ""
    
    # Limpiar espacios
    query_clean = query.strip()
    
    # Buscar la posición de la coincidencia ignorando mayúsculas/minúsculas
    match = re.search(re.escape(query_clean), text, re.IGNORECASE)
    
    if not match:
        # Si no hay match directo en el full_text, devolvemos los primeros caracteres como fallback
        return text[:radius * 2] + "..." if len(text) > radius * 2 else text

    start_idx, end_idx = match.start(), match.end()
    
    # Calcular los límites del fragmento alrededor del match
    snippet_start = max(0, start_idx - radius)
    snippet_end = min(len(text), end_idx + radius)
    
    # Extraer el fragmento original
    fragment = text[snippet_start:snippet_end]
    
    # Re-calcular los índices del match dentro del fragmento extraído
    match_in_fragment = re.search(re.escape(query_clean), fragment, re.IGNORECASE)
    
    if match_in_fragment:
        f_start, f_end = match_in_fragment.start(), match_in_fragment.end()
        # Envolver la palabra exacta encontrada con las etiquetas <mark>
        highlighted = (
            fragment[:f_start] + 
            f"<mark>{fragment[f_start:f_end]}</mark>" + 
            fragment[f_end:]
        )
    else:
        highlighted = fragment

    # Añadir elipsis si el texto fue recortado
    if snippet_start > 0:
        highlighted = "..." + highlighted
    if snippet_end < len(text):
        highlighted = highlighted + "..."
        
    return highlighted

@router.get("/documentos")
async def get_documentos(
    departamento: str = Query(...),
    estado: str = Query(None)
):
    db = get_db()
    collection = db["documentos_indexados"]

    filtro = {"metadata.departamento": departamento}
    if estado:
        filtro["estado"] = estado.lower()

    cursor = collection.find(
        filtro,
        {
            "_id": 0,
            "display_name": 1,
            "estado": 1,
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