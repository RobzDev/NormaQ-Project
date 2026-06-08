# NormaQ / Proyecto DocsManager

NormaQ es una plataforma documental compuesta por varios servicios coordinados que permiten la gestión operativa de documentos, su autenticación centralizada, el almacenamiento de archivos, la consulta de información y la indexación para búsqueda. El repositorio está organizado como una solución multi-servicio con tres aplicaciones principales y una infraestructura compartida de base de datos, mensajería y almacenamiento.

El proyecto combina las siguientes capas:

- Un núcleo web en **ASP.NET Core** encargado de la lógica principal, autenticación por cookies, acceso a SQL Server y publicación de eventos relacionados con documentos.
- Una aplicación complementaria en **Laravel** orientada a funciones operativas, consulta y auditoría dentro del flujo del usuario.
- Un servicio auxiliar en **FastAPI** para indexación, búsqueda y sincronización documental sobre MongoDB.
- Servicios de soporte en **Docker** para SQL Server, PostgreSQL, Redis, MinIO y MongoDB.

La disposición del repositorio responde a una arquitectura orientada a dominio, con separación clara entre la lógica de negocio, la persistencia y la infraestructura. Esto facilita el despliegue local, la evolución por módulos y la incorporación posterior de nuevas capacidades sin mezclar responsabilidades.

## Estructura general del repositorio

- `src/dotnet-core/NormaQ/`: aplicación principal en ASP.NET Core 10.
- `src/php-app/`: aplicación Laravel 13 con frontend basado en Vite y Tailwind.
- `src/fastapi/`: servicio Python con FastAPI para búsqueda e indexación.
- `docker/`: definición de contenedores, proxy Nginx y composición de la infraestructura.
- `db/`: scripts de inicialización y seeding para SQL Server y PostgreSQL.

## Componentes del sistema

### Aplicación principal en ASP.NET Core

El proyecto ubicado en `src/dotnet-core/NormaQ/` actúa como el núcleo del sistema. Desde allí se inicializa el contexto de Entity Framework, la autenticación basada en cookies, la integración con Redis y la conexión con MinIO para almacenamiento de objetos. Durante el arranque, la aplicación aplica migraciones y ejecuta el script de inicialización de SQL Server para asegurar que la base de datos esté lista antes de atender solicitudes.

### Aplicación Laravel

La aplicación situada en `src/php-app/` complementa el flujo operativo del sistema. Su capa de rutas expone funcionalidades de acceso, consulta, auditoría y navegación interna. En el despliegue por Docker, esta aplicación se prepara automáticamente con dependencias, generación de clave y migraciones de base de datos.

### Servicio FastAPI

El módulo `src/fastapi/` se encarga de la indexación y consulta especializada. Su ciclo de vida inicia conexiones con MongoDB, crea índices, ejecuta validaciones de sincronización y mantiene un listener asíncrono para reaccionar a cambios documentales.

### Infraestructura compartida

El directorio `docker/` define la orquestación local con Nginx como punto de entrada HTTP, además de SQL Server, PostgreSQL, Redis, MinIO, MongoDB y los servicios de aplicación. Este enfoque permite levantar todo el ecosistema con un único comando y reproducir el entorno completo en una estación de desarrollo.

## Requisitos previos

Antes de clonar y ejecutar el proyecto, asegúrate de contar con lo siguiente:

- Git.
- Docker Engine y Docker Compose.
- Puertos libres para los servicios expuestos: `80`, `1433`, `5432`, `6379`, `8000`, `9000`, `9001` y `27017`.

## Cómo clonar el repositorio

1. Abre una terminal en la carpeta donde deseas guardar el proyecto.
2. Clona el repositorio.

```bash
git clone <URL_DEL_REPOSITORIO>
```

3. Entra al directorio raíz del proyecto.

```bash
cd NormaQ-Project
```

4. Verifica que la estructura esperada esté presente antes de continuar.

## Configuración inicial recomendada

La forma más estable y reproducible de ejecutar este proyecto es mediante Docker Compose, ya que el repositorio incluye la definición de todos los servicios y sus dependencias.

1. Ubícate en la carpeta `docker/`.
2. Crea o ajusta el archivo de variables de entorno que consume la composición.
3. Define al menos las credenciales y parámetros que usan SQL Server, PostgreSQL, MongoDB, MinIO, Redis y la conexión entre servicios.

Las variables que normalmente debes revisar son las siguientes:

- `SA_PASSWORD`
- `PG_USER`
- `PG_PASSWORD`
- `PG_DB`
- `MONGO_USER`
- `MONGO_PASSWORD`
- `MONGO_DB`
- `MONGO_URI`
- `REDIS_HOST`
- `REDIS_PORT`
- `MINIO_ROOT_USER`
- `MINIO_ROOT_PASSWORD`
- `MINIO_ENDPOINT`
- `MINIO_BUCKET`

## Ejecución completa con Docker

Este es el flujo recomendado para levantar todo el ecosistema local.

1. Cambia a la carpeta de Docker.

```bash
cd docker
```

2. Construye y levanta toda la pila de servicios.

```bash
docker compose up --build
```

3. Espera a que los contenedores completen su arranque inicial. Durante este proceso:

- SQL Server aplica el script de inicialización definido en `db/sqlserver/`.
- PostgreSQL se prepara con los scripts de `db/pgsql/`.
- El servicio .NET intenta ejecutar migraciones hasta completar la conexión con la base de datos.
- Laravel instala dependencias, genera la clave de la aplicación y ejecuta migraciones.
- FastAPI conecta con MongoDB, crea índices y activa su listener interno.

4. Abre la aplicación principal en el navegador.

```text
http://localhost
```

5. Si necesitas acceder a servicios auxiliares directamente, usa las siguientes rutas o puertos según corresponda:

- FastAPI: `http://localhost:8000`
- SQL Server: `localhost:1433`
- PostgreSQL: `localhost:5432`
- Redis: `localhost:6379`
- MinIO API: `http://localhost:9000`
- MinIO Console: `http://localhost:9001`
- MongoDB: `localhost:27017`

## Ejecución por módulos

Si prefieres trabajar sobre un servicio específico sin levantar toda la plataforma, puedes iniciar cada aplicación de manera independiente. Este enfoque es útil para depuración, pruebas focalizadas o desarrollo de una sola capa.

### ASP.NET Core

1. Entra en el proyecto principal.

```bash
cd src/dotnet-core/NormaQ
```

2. Restaura dependencias y ejecuta la aplicación.

```bash
dotnet restore
dotnet watch run
```

### Laravel

1. Entra en la aplicación Laravel.

```bash
cd src/php-app
```

2. Instala dependencias PHP si aún no están presentes.

```bash
composer install
```

3. Prepara la aplicación y ejecuta el servidor local.

```bash
php artisan key:generate
php artisan migrate
php artisan serve
```

4. Si además necesitas compilar recursos frontend, ejecuta:

```bash
npm install
npm run dev
```

### FastAPI

1. Entra en el servicio Python.

```bash
cd src/fastapi
```

2. Instala las dependencias del proyecto.

```bash
pip install -r requirements.txt
```

3. Inicia el servicio.

```bash
uvicorn main:app --reload --host 0.0.0.0 --port 8000
```

## Verificación de funcionamiento

Una vez iniciado el entorno, valida lo siguiente:

- La URL raíz responde a través de Nginx.
- La aplicación .NET completa sus migraciones y expone su ruta de salud en `/health`.
- Laravel puede autenticarse y redirigir a su panel operativo.
- FastAPI responde correctamente en `/health`.
- SQL Server, PostgreSQL, Redis, MinIO y MongoDB quedan accesibles para los servicios que los requieren.

## Observaciones operativas

- El arranque inicial puede tardar más de lo habitual porque el sistema aplica migraciones y valida dependencias entre contenedores.
- Si SQL Server aún no está listo, el servicio .NET reintentará la migración varias veces antes de continuar.
- En Laravel, el contenedor ya contempla la instalación de dependencias y la generación de la clave de aplicación durante el arranque en Docker.
- Para desarrollo local manual, revisa cuidadosamente las variables de conexión de cada servicio antes de ejecutar comandos individuales.

## Propósito del proyecto

Este repositorio está diseñado para centralizar la gestión documental, la autenticación y la búsqueda bajo un esquema modular. La elección de múltiples tecnologías no responde a un objetivo ornamental, sino a una separación funcional clara: el núcleo transaccional se mantiene en ASP.NET Core, las funciones operativas y de interfaz complementaria viven en Laravel, y la capa de indexación y consulta especializada se delega a FastAPI. En conjunto, la solución ofrece una base sólida para procesos documentales con trazabilidad, almacenamiento y consulta distribuida.
