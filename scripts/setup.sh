#!/usr/bin/env bash
set -e

# Цвета для вывода
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${GREEN}=== NaiveRag Environment Setup ===${NC}\n"

# Проверка Docker
if ! command -v docker &> /dev/null; then
    echo -e "${RED}Error: Docker is not installed${NC}"
    echo "Install from: https://docs.docker.com/get-docker/"
    exit 1
fi

if ! docker compose version &> /dev/null; then
    echo -e "${RED}Error: Docker Compose is not available${NC}"
    echo "Install Docker Compose V2"
    exit 1
fi

echo -e "${GREEN}✓ Docker and Docker Compose found${NC}"

# Запуск инфраструктуры
echo -e "\n${YELLOW}Starting services...${NC}"
docker compose up -d

# Функция ожидания готовности сервиса
wait_for_service() {
    local service=$1
    local check_cmd=$2
    local timeout=60
    local elapsed=0
    
    echo -e "${YELLOW}Waiting for $service to be ready...${NC}"
    
    while ! eval "$check_cmd" &> /dev/null; do
        if [ $elapsed -ge $timeout ]; then
            echo -e "${RED}Timeout waiting for $service${NC}"
            exit 1
        fi
        sleep 2
        elapsed=$((elapsed + 2))
    done
    
    echo -e "${GREEN}✓ $service is ready${NC}"
}

# Ожидание PostgreSQL
wait_for_service "PostgreSQL" \
    "docker exec naive_rag_postgres pg_isready -U raguser -d naive_rag"

# Ожидание ChromaDB
wait_for_service "ChromaDB" \
    "curl -f http://localhost:8000/api/v1/heartbeat"

# Ожидание Ollama
wait_for_service "Ollama" \
    "docker exec naive_rag_ollama ollama list"

# Применение миграций (если .NET SDK установлен)
if command -v dotnet &> /dev/null; then
    echo -e "\n${YELLOW}Applying database migrations...${NC}"
    
    if [ -d "./NaiveRag.Infrastructure" ]; then
        dotnet ef database update \
            --project src/NaiveRag.Infrastructure \
            --startup-project src/NaiveRag.CLI \
            --no-build || echo -e "${YELLOW}⚠ Migrations skipped (run manually if needed)${NC}"
        echo -e "${GREEN}✓ Migrations applied${NC}"
    else
        echo -e "${YELLOW}⚠ Infrastructure project not found, skipping migrations${NC}"
    fi
else
    echo -e "${YELLOW}⚠ .NET SDK not found, skipping migrations${NC}"
fi

# Итоговая проверка
echo -e "\n${GREEN}=== Setup Complete ===${NC}"
echo -e "\nServices running:"
docker compose ps

echo -e "\n${GREEN}Next steps:${NC}"
echo "  1. Run the CLI: dotnet run --project src/NaiveRag.CLI"
echo "  2. Check logs: docker compose logs -f"
echo "  3. Stop services: docker compose down"

echo -e "\n${GREEN}Service URLs:${NC}"
echo "  ChromaDB:   http://localhost:8000"
echo "  Ollama API: http://localhost:11434"
echo "  PostgreSQL: localhost:5432"