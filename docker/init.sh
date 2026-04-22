#!/bin/bash
set -e

echo "===== Esperando SQL Server ====="
sleep 5

echo "===== Inicializando SQL Server ====="
SQLCMD_BIN="/opt/mssql-tools18/bin/sqlcmd"
if [ ! -x "$SQLCMD_BIN" ]; then
	SQLCMD_BIN="/opt/mssql-tools/bin/sqlcmd"
fi
"$SQLCMD_BIN" -S sqlserver -U sa -P "$SA_PASSWORD" -C -i /scripts/sqlserver/init.sql
"$SQLCMD_BIN" -S sqlserver -U sa -P "$SA_PASSWORD" -C -i /scripts/sqlserver/seeding.sql

echo "===== Esperando PostgreSQL ====="
sleep 5

echo "===== Inicializando PostgreSQL ====="
PGPASSWORD=$PG_PASSWORD psql -h pgsql -U $PG_USER -d $PG_DB -f /scripts/pgsql/init.sql

# Seeding futuro PostgreSQL
# PGPASSWORD=$PG_PASSWORD psql -h pgsql -U $PG_USER -d $PG_DB -f /scripts/pgsql/seeding.sql

echo "===== Inicialización completa ====="