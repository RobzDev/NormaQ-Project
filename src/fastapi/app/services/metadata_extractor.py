import os
import hashlib
import magic
import fitz  # pymupdf
import openpyxl
from pathlib import Path

def compute_sha256(path: str) -> str:
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(8192), b""):
            h.update(chunk)
    return h.hexdigest()

def extract_metadata(tmp_path: str, storage_path: str) -> dict:
    ext = Path(storage_path).suffix.lower()
    file_size_kb = round(os.path.getsize(tmp_path) / 1024, 2)
    mime_type = magic.from_file(tmp_path, mime=True)
    hash_sha256 = compute_sha256(tmp_path)

    extra = {}

    # PDF
    if ext == ".pdf":
        try:
            doc = fitz.open(tmp_path)
            extra["page_count"] = doc.page_count
            doc.close()
        except Exception:
            extra["page_count"] = None

    # Excel
    elif ext in (".xlsx", ".xlsm"):
        try:
            wb = openpyxl.load_workbook(tmp_path, read_only=True)
            extra["sheet_count"] = len(wb.sheetnames)
            extra["sheet_names"] = wb.sheetnames
            wb.close()
        except Exception:
            extra["sheet_count"] = None

    # Texto plano / CSV / XML
    elif ext in (".txt", ".csv", ".xml", ".json"):
        try:
            with open(tmp_path, "r", encoding="utf-8", errors="ignore") as f:
                lines = f.readlines()
            extra["line_count"] = len(lines)
        except Exception:
            extra["line_count"] = None

    # CAD / binarios sin parser (DWG, DXF, STL, etc.)
    elif ext in (".dwg", ".dxf", ".stl", ".step", ".igs"):
        extra["note"] = "Archivo CAD/binario, sin extracción de contenido"

    # Word
    elif ext in (".doc", ".docx"):
        extra["note"] = "Documento Word"

    # Cualquier otro
    else:
        extra["note"] = f"Tipo no manejado específicamente: {ext}"

    return {
        "extension": ext,
        "mime_type": mime_type,
        "file_size_kb": file_size_kb,
        "hash_sha256": hash_sha256,
        **extra
    }