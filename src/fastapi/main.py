from fastapi import FastAPI
from contextlib import asynccontextmanager
from app.core.config import settings
from app.core.database import connect_mongo, disconnect_mongo

@asynccontextmanager
async def lifespan(app: FastAPI):
    await connect_mongo()
    yield
    await disconnect_mongo()

app = FastAPI(
    title="QualityDoc Search Service",
    version="1.0.0",
    lifespan=lifespan
)

@app.get("/health")
async def health():
    return {"status": "ok", "service": "fastapi"}