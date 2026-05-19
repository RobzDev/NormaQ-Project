import boto3
from botocore.client import Config
from app.core.config import settings
import tempfile
import os
from urllib.parse import urlparse


def _normalize_endpoint_url(endpoint: str) -> str:
    if not endpoint:
        raise ValueError("MinIO endpoint is required")

    parsed = urlparse(endpoint if "://" in endpoint else f"http://{endpoint}")
    host = parsed.netloc or parsed.path
    return f"{parsed.scheme or 'http'}://{host}"

def get_minio_client():
    client = boto3.client(
        "s3",
        endpoint_url=_normalize_endpoint_url(settings.minio_endpoint),
        aws_access_key_id=settings.minio_access_key,
        aws_secret_access_key=settings.minio_secret_key,
        config=Config(signature_version="s3v4"),
        region_name="us-east-1",
    )

    print(f"🔑 MinIO Client Configured: {_normalize_endpoint_url(settings.minio_endpoint)}", flush=True)
    return client

async def download_file_temp(storage_path: str) -> str:
    """
    Descarga el archivo de MinIO a un archivo temporal.
    Retorna la ruta local del archivo temporal.
    """
    client = get_minio_client()
    
    ext = os.path.splitext(storage_path)[-1]


    with tempfile.NamedTemporaryFile(delete=False, suffix=ext) as tmp:
        tmp_path = tmp.name

  

    client.download_file(
        Bucket=settings.minio_bucket,
        Key=storage_path,
        Filename=tmp_path
    )

    print(f"📥 Archivo descargado temporalmente: {tmp_path}", flush=True)
    return tmp_path