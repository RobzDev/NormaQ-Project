Aquí tienes una versión completamente reestructurada y profesional del archivo `README.md`. He integrado el diagrama de arquitectura (en formato de texto estructurado y marcadores), detallado el proceso exacto para clonar, configurar los dos entornos de variables (`.env`) desde sus respectivos archivos de ejemplo, y listado las credenciales por defecto para que el documento sea 100% autoejecutable.

---

# NormaQ / Proyecto DocsManager

NormaQ es una plataforma documental compuesta por varios servicios coordinados que permiten la gestión operativa de documentos, su autenticación centralizada, el almacenamiento de archivos, la consulta de información y la indexación para búsqueda. El repositorio está organizado como una solución multi-servicio con tres aplicaciones principales y una infraestructura compartida de base de datos, mensajería y almacenamiento.

---

## 1. Arquitectura del Sistema

El sistema implementa una arquitectura de microservicios orientada al dominio, comunicada mediante HTTP y eventos asíncronos (Redis Pub/Sub).

```
                      [ NGINX Reverse Proxy (Puerto 80) ]
                                      |
                                      v
                             [ .NET 10 Core MVC ]
                               Servicio Maestro
                         (Login, Registro, Negocio)
                               |            |
         +---------------------+            +---------------------+
         | HTTP (SSO)          | Redis      | Redis               | HTTP
         v                     | Pub/Sub    v                     v
  [ Laravel / PHP ]            |         [ SQL Server ]    [ FastAPI / Python ]
   Portal Operario             |          (Documentos &      Indexación, Búsqueda
    (Solo Lectura)             v            Usuarios)           Autocompletado
         |              [ MinIO Object Storage ]                  |
         |                 (Archivos Físicos)                     |
         v                     ^            ^                     v
  [ PostgreSQL ]               |            |               [ MongoDB ]
   (Auditoría)                 +------------+             (Índice Full-Text)

```

### Capas del Proyecto

* **Núcleo Web (.NET 10 MVC):** Encargado de la lógica principal, autenticación centralizada compartida (Redis SSO), acceso a SQL Server y publicación de eventos relacionados con documentos.
* **Portal Operario (Laravel 13):** Aplicación complementaria orientada a funciones operativas de lectura, consulta y auditoría con persistencia en PostgreSQL.
* **Servicio de Búsqueda (FastAPI):** Servicio auxiliar en Python enfocado en la indexación, búsqueda y sincronización documental sobre MongoDB.
* **Infraestructura:** Orquestación en Docker para SQL Server, PostgreSQL, Redis, MinIO y MongoDB.

---

## 2. Estructura del Repositorio

```
NormaQ-Project/
├── db/                             # Scripts de inicialización y seeding
│   ├── sqlserver/
│   └── pgsql/
├── docker/                         # Configuración de Docker Compose y Nginx
│   ├── .env.example                # Plantilla de entorno para la infraestructura
│   └── nginx/
└── src/                            # Código fuente de las aplicaciones
    ├── dotnet-core/NormaQ/         # Aplicación principal en ASP.NET Core 10
    ├── fastapi/                    # Microservicio de búsqueda (Python)
    └── php-app/                    # Portal operativo (Laravel 13 + Vite)
        └── .env.example            # Plantilla de entorno interna de Laravel

```

---

## 3. Requisitos Previos

Antes de desplegar, asegúrate de tener instalados los siguientes componentes en tu sistema operativo:

* **Git**
* **Docker Engine** y **Docker Compose**
* **Puertos Libres:** Es mandatorio que los siguientes puertos no estén ocupados por instancias locales:
* `80` (Nginx)
* `1433` (SQL Server)
* `5432` (PostgreSQL)
* `6379` (Redis)
* `8000` (FastAPI)
* `9000` & `9001` (MinIO API / Console)
* `27017` (MongoDB)



---

## 4. Instalación y Configuración Inicial

### Paso 1: Clonar el repositorio

```bash
git clone https://github.com/RobzDev/NormaQ-Project.git
cd NormaQ-Project

```

### Paso 2: Configurar Variables de Entorno (.env)

El proyecto requiere la configuración de **dos archivos** `.env` independientes para poder inicializar la infraestructura y el contenedor de Laravel de forma correcta.

1. **Configurar Entorno de Infraestructura (Docker):**
```bash
# Copia el archivo de ejemplo en la raíz de la carpeta de docker
cp docker/.env.example docker/.env

```


2. **Configurar Entorno de la Aplicación Laravel:**
```bash
# Copia el archivo de ejemplo en la raíz del proyecto PHP
cp src/php-app/.env.example src/php-app/.env

```



---

## 5. Credenciales por Defecto y Puertos

Al levantar el entorno con los archivos `.env.example` preconfigurados, los servicios se inicializan con los siguientes accesos:

| Servicio | Componente / BD | Puerto Host | Usuario / Rol | Contraseña por Defecto |
| --- | --- | --- | --- | --- |
| **Nginx** | Proxy Inverso (Web) | `80` | N/A | Acceso Público (`http://localhost`) |
| **SQL Server** | Documentos & Usuarios | `1433` | `sa` | `SecurePassword123!` |
| **PostgreSQL** | Auditoría | `5432` | `postgres_user` | `postgres_password` |
| **MongoDB** | Índice Full-Text | `27017` | `mongo_user` | `mongo_password` |
| **MinIO API** | Almacenamiento de Objetos | `9000` | `minio_user` | `minio_password` |
| **MinIO Console** | Panel de Administración | `9001` | `minio_user` | `minio_password` |
| **Redis** | SSO & Pub/Sub | `6379` | N/A | Sin contraseña (por defecto en red interna) |
| **FastAPI** | API de Búsqueda | `8000` | N/A | Acceso directo a endpoints / docs |

---

## 6. Despliegue con Docker (Flujo Recomendado)

Para levantar todo el ecosistema coordinado de forma automatizada:

```bash
# 1. Navegar al directorio de Docker
cd docker

# 2. Construir e inicializar los contenedores
docker compose up --build

```

### Proceso de inicialización automática en el primer arranque:

1. **SQL Server & PostgreSQL:** Inicializan las bases de datos ejecutando los scripts ubicados en `db/sqlserver/` y `db/pgsql/`.
2. **.NET Core MVC:** Espera la disponibilidad de SQL Server, ejecuta de forma automática las migraciones de *Entity Framework* e inicia el servidor.
3. **Laravel:** Instala dependencias mediante Composer, genera la `APP_KEY` dentro de tu `.env`, ejecuta las migraciones correspondientes y arranca el servidor a través del contenedor.
4. **FastAPI:** Levanta la conexión hacia MongoDB, genera de forma automática los índices de búsqueda y activa el *listener* asíncrono de Redis.

Para ingresar a la aplicación, abre tu navegador web e interactúa directamente en: **`http://localhost`**

