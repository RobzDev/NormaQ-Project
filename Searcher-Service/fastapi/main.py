import asyncio

from fastapi import FastAPI
from contextlib import asynccontextmanager
from app.core.config import settings
from app.core.database import connect_mongo, disconnect_mongo
from app.services.document_listener import start_listener
from app.core.indexes import create_indexes
from app.services.sync_service import sync_check

@asynccontextmanager
async def lifespan(app: FastAPI):
    print("🚀 LIFESPAN START", flush=True)
    await connect_mongo()  # <- aquí
    await create_indexes()
    await sync_check()

    async def runner():
        try:
            await start_listener()
        except Exception as e:
            print(f"💥 LISTENER ERROR: {repr(e)}", flush=True)

    task = asyncio.create_task(runner())
    yield
    task.cancel()
    await disconnect_mongo()

app = FastAPI(
    title="QualityDoc Search Service",
    version="1.0.0",
    lifespan=lifespan
)
@app.get("/health")
async def health():
    return {"status": "ok", "service": "fastapi"}

from app.routers.search import router as search_router
app.include_router(search_router)