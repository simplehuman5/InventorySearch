# Inventory Search

A Blazor app that lets you upload images and find similar ones using AI (CLIP embeddings + PostgreSQL pgvector).
More to come

## What it does

- Upload images and automatically generate embeddings
- Find duplicate/similar images using cosine similarity
- Store everything in PostgreSQL with vector search

## Tech used

- .NET 10 / Blazor
- Entity Framework Core
- PostgreSQL + pgvector + npgsql
- ONNX Runtime (CLIP model - 512)
- More to come

## Getting started

### 1. Set up PostgreSQL

```sql
CREATE DATABASE inventory;
CREATE EXTENSION vector;

CREATE TABLE images (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL UNIQUE,
    embedding vector(512) NOT NULL,
    filename VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### 2. Configure your connection string

```bash
cd InventorySearch/InventorySearch
dotnet user-secrets set "ConnectionStrings:InventoryDb" "Host=localhost;Port=5432;Database=inventory;Username=youruser;Password=yourpassword"
```

### 3. Get the ONNX model

Either:
- Download manually from [Hugging Face](https://huggingface.co/openai/clip-vit-base-patch32/tree/main/onnx) and put it in `wwwroot/models/`
- Or set `"AutoDownload": true` in `appsettings.json`

### 4. Run it

```bash
dotnet run --project InventorySearch/InventorySearch
```

## License
- My license