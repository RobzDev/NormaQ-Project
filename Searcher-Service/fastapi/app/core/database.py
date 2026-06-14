from motor.motor_asyncio import AsyncIOMotorClient
from app.core.config import settings

client: AsyncIOMotorClient = None

async def connect_mongo():
    global client
    client = AsyncIOMotorClient(settings.mongo_uri)
    print(f"✅ MongoDB conectado", flush=True)

async def disconnect_mongo():
    global client
    if client:
        client.close()
        print("MongoDB desconectado")

def get_db():
    if client is None:
        raise RuntimeError("MongoDB no está conectado")
    db_name = settings.mongo_uri.split("/")[-1].split("?")[0]
    return client[db_name]