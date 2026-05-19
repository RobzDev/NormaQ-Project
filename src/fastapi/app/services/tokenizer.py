import re

STOPWORDS = {"de", "la", "el", "los", "las", "un", "una", "application", "pdf", "y", "en", "a"}

def tokenize(*texts: str) -> list[str]:
    tokens = set()
    for text in texts:
        if not text:
            continue
        # Agregar el texto completo en lowercase como token (búsqueda exacta)
        tokens.add(text.lower().strip())
        # Separar y agregar partes individuales
        words = re.split(r"[\s\-_./]+", text.lower())
        for word in words:
            word = word.strip()
            if len(word) >= 2 and word not in STOPWORDS:
                tokens.add(word)
    return list(tokens)

def build_search_tokens(data: dict, metadata: dict) -> list[str]:
    return tokenize(
        data.get("CodigoDocumento", ""),
        data.get("DisplayName", ""),
        data.get("NivelDocumento", ""),
        data.get("Norma", ""),
        data.get("Departamento", ""),
        data.get("Owner", ""),
        data.get("ApprovedBy", ""),
        metadata.get("extension", ""),
    )