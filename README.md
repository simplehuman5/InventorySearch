# Inventory Search

A Blazor app that lets you upload images and find similar ones using AI (CLIP embeddings + PostgreSQL pgvector). Comparison is done by checking cosine similarity.
<br/>More to come

## What it does

- Upload images and automatically generate embeddings
- Find duplicate/similar images using cosine similarity
- Store everything in PostgreSQL with vector search

## Tech stack

- .NET 10 / Blazor
- Entity Framework Core
- PostgreSQL + pgvector + npgsql
- ONNX Runtime (CLIP model - 512)
- More to come

## To do (what's left)
- Unit tests
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
- Download manually from [Hugging Face](https://huggingface.co/Xenova/clip-vit-base-patch32/blob/main/onnx/model_q4f16.onnx) by Xenova and put it in `wwwroot/models/`
- Note this is quantized version which means compressed. For full version, recommended to export the full version using hugging face optimum
- Or set `"AutoDownload": true` in `appsettings.json`

### 4. Run it

```bash
dotnet run --project InventorySearch/InventorySearch
```

## License
- My license