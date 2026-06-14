import re

STOPWORDS = {"de", "la", "el", "los", "las", "un", "una", "application", 
             "pdf", "y", "en", "a", "que", "se", "por", "con", "para"}

def tokenize(*texts: str) -> list[str]:
    tokens = set()
    for text in texts:
        if not text:
            continue
        words = re.split(r"[\s\-_./]+", text.lower())
        for word in words:
            word = word.strip()
            if len(word) >= 2 and word not in STOPWORDS:
                tokens.add(word)
    return list(tokens)

def build_search_tokens(data: dict, metadata: dict) -> list[str]:
    return tokenize(
        data.get("codigoDocumento", ""),
        data.get("nombreDocumento", ""),
        data.get("nivel", ""),
        data.get("normaCodigo", ""),
        data.get("departamento", ""),
        data.get("owner", ""),
        data.get("approvedBy", ""),
        metadata.get("extension", ""),
        metadata.get("full_text", ""),  # <- full text incluido en tokens
    )