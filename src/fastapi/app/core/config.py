from pydantic_settings import BaseSettings

class Settings(BaseSettings):
    mongo_uri: str
    redis_host: str
    redis_port: int = 6379
    minio_endpoint: str
    minio_access_key: str
    minio_secret_key: str
    minio_bucket: str

    class Config:
        env_file = ".env"

settings = Settings()