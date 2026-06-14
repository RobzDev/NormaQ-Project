#!/bin/bash

# Script para gestionar el inicio y apagado de los microservicios de NormaQ en orden.

# Definimos los docker-compose y sus respectivos .env en orden secuencial
SERVICES=(
  "Infrastructure/docker-compose.infrastructure.yml:Infrastructure/.env"
  "Normative-Service/docker-compose.dotnet.yml:Normative-Service/.env"
  "Searcher-Service/docker-compose.python.yml:Searcher-Service/.env"
  "OperationalWeb-Service/docker-compose.php.yml:OperationalWeb-Service/.env"
)

# Si se pasa un argumento (cualquiera, ej: stop, down, etc.), apagamos
if [ -n "$1" ]; then
  echo "===> Deteniendo contenedores en orden inverso..."
  # Iteramos en orden inverso para apagar primero los consumidores y al final la infraestructura
  for (( idx=${#SERVICES[@]}-1; idx>=0; idx-- )); do
    entry="${SERVICES[idx]}"
    COMPOSE_FILE="${entry%%:*}"
    ENV_FILE="${entry##*:}"
    echo "--------------------------------------------------"
    echo "Deteniendo: $COMPOSE_FILE"
    echo "--------------------------------------------------"
    docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" down
  done
  echo "===> Todos los servicios detenidos correctamente."
else
  echo "===> Levantando contenedores en orden secuencial..."
  for entry in "${SERVICES[@]}"; do
    COMPOSE_FILE="${entry%%:*}"
    ENV_FILE="${entry##*:}"
    echo "--------------------------------------------------"
    echo "Levantando: $COMPOSE_FILE"
    echo "--------------------------------------------------"
    
    if [[ "$COMPOSE_FILE" == *"infrastructure"* ]]; then
      docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d
    else
      docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d --build
    fi
    
    # Damos un pequeño respiro para que el servicio inicie y evitemos colisiones
    echo "Esperando 3 segundos..."
    sleep 3
  done
  echo "===> Todos los servicios iniciados correctamente."
fi
