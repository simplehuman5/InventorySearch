# InventorySearch Copilot Instructions

## Project Overview
- Blazor WebAssembly app targeting .NET 10
- Image similarity search using CLIP embeddings (512 dimensions)
- PostgreSQL with pgvector extension for vector storage and similarity search
- Cosine similarity for comparing image embeddings

## Tech Stack
- .NET 10 / Blazor WebAssembly
- Entity Framework Core with Npgsql
- PostgreSQL + pgvector
- ONNX Runtime (CLIP model)

## Coding Standards
- Use async/await patterns for all I/O operations
- Follow existing EF Core patterns for data access
- Prefer Blazor component patterns over MVC or Razor Pages
- Use dependency injection for services

## Architecture
- Server project: `InventorySearch` - API, data access, ONNX inference
- Client project: `InventorySearch.Client` - Blazor WebAssembly UI components

## Tech Constraints
- ONNX Runtime for CLIP model inference
- Cosine similarity for image matching via pgvector
- Npgsql for PostgreSQL connectivity
- Vector dimension: 512
