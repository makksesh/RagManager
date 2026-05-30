# Naive RAG Project

Учебный проект для понимания базового RAG pipeline.

## Быстрый старт

### 1. Запуск инфраструктуры

```bash
# Поднять все сервисы (Ollama, ChromaDB, PostgreSQL)
docker compose up -d

# Проверить статус
docker compose ps

# Посмотреть логи
docker compose logs -f
```

### 2. Скачать модели в Ollama

```bash
# Embedding модель
docker exec -it naive_rag_ollama ollama pull nomic-embed-text

# LLM для генерации
docker exec -it naive_rag_ollama ollama pull llama3.2:3b
```

### 3. Запуск приложения

```bash
# Применить миграции БД
dotnet ef database update --project src/NaiveRag.Infrastructure --startup-project src/NaiveRag.CLI

# Запустить CLI
dotnet run --project src/NaiveRag.CLI
```

## Остановка

```bash
# Остановить сервисы (данные сохраняются)
docker compose down

# Остановить + удалить данные (чистый перезапуск)
docker compose down -v
```

## Проверка работоспособности

```bash
# ChromaDB
curl http://localhost:8000/api/v1/heartbeat

# Ollama
curl http://localhost:11434/api/tags

# PostgreSQL
docker exec -it naive_rag_postgres psql -U raguser -d naive_rag -c "SELECT version();"
```