# Dataset Ingestion Hub

## Overview

The **Dataset Ingestion Hub** is the administrative gateway for the ETL (Extract, Transform, Load) pipeline. It manages the complete lifecycle of dataset records by discovering updates from remote sources, extracting detailed metadata from multiple formats (ISO 19115, JSON, JSON-LD, RDF), and orchestrating AI-powered document analysis through integration with the Python Search Service.

Key capabilities include:

- **Dataset Discovery**: Automated synchronization with remote catalog services
- **Metadata Extraction**: Multi-format parsing (ISO 19115, JSON, RDF/Turtle)
- **ETL Orchestration**: Complete pipeline management with status tracking
- **AI Integration**: Seamless handoff to Python service for vector indexing
- **Data Relationships**: Tracks parent-child and sibling relationships between datasets

---

## Requirements

| Component       | Specification                     |
| --------------- | --------------------------------- |
| **Runtime**     | .NET 8.0 0                        |
| **Database**    | SQLite (`etl_database.db`)        |
| **Integration** | Python Search Service (port 8001) |
| **Testing**     | MSTest                            |
| **Logging**     | Serilog (Console + File)          |

---

## Setup

### 1. Prerequisites

- Install .NET 8.0 SDK or later
- Ensure Python Search Service is running (see Python service README)
- SQLite database will be auto-created on first launch

### 2. Configuration

Update `appsettings.json` with your settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=../../etl_database.db"
  },
  "EtlSettings": {
    "MetadataIdentifiersFilePath": "../../metadata-file-identifiers.txt",
    "PythonServiceUrl": "http://localhost:8001",
    "MaxDegreeOfParallelism": 10
  }
}
```

### 3. Database & Storage

- **SQLite Database**: `etl_database.db` (shared with Python service)
- **Logs**: `logs/etl-*.log` (daily rolling logs)
- **Metadata Identifiers**: Text file containing dataset file identifiers for discovery

### 4. Run the Service

```bash
# Navigate to the API project
cd DSH-ETL-2025-API

# Restore dependencies
dotnet restore

# Run the service
dotnet run --project DSH-ETL-2025-API
```

**API Documentation**: http://localhost:5133/swagger

---

## User Guide

The following table describes all available functionalities in the system:

| Functionality              | Endpoint                           | Method | Description                                                                                                                                                   | Use Case                                                                  |
| -------------------------- | ---------------------------------- | ------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------- |
| **Search Datasets**        | `/api/search?q={query}`            | GET    | Performs keyword-based search across dataset metadata (title, abstract). Returns matching datasets with basic information.                                    | Quick keyword lookup for datasets by title or description                 |
| **Get Dataset Details**    | `/api/search/details/{identifier}` | GET    | Retrieves complete dataset information including metadata, geospatial data, relationships, supporting documents, and data files.                              | View full dataset profile with all associated resources and relationships |
| **Get Discovery Stats**    | `/api/search/stats`                | GET    | Returns high-level statistics about the dataset catalog: total datasets count and total unique data providers.                                                | Dashboard metrics and system health monitoring                            |
| **Process Single Dataset** | `/api/etl/process/{identifier}`    | POST   | Triggers the complete ETL pipeline for a specific dataset: discovery, metadata extraction, document parsing, and queueing for AI indexing.                    | Reprocess a specific dataset or handle individual dataset ingestion       |
| **Process All Datasets**   | `/api/etl/process-all`             | POST   | Executes the ETL pipeline for all datasets listed in the metadata identifiers file. Processes datasets in parallel (configurable via MaxDegreeOfParallelism). | Initial bulk ingestion or full catalog refresh                            |
| **Get ETL Status**         | `/api/etl/status`                  | GET    | Returns current ETL processing status including active operations, queue depth, and processing metrics.                                                       | Monitor ETL pipeline health and track processing progress                 |

### Example Usage

#### Search Datasets

```http
GET /api/search?q=water quality
```

#### Get Dataset Details

```http
GET /api/search/details/b0afb78e-1234-5678-90ab-cdef12345678
```

#### Process Single Dataset

```http
POST /api/etl/process/b0afb78e-1234-5678-90ab-cdef12345678
```

#### Get ETL Status

```http
GET /api/etl/status
```

---

## Architecture

The system follows **Clean Architecture** principles with clear separation of concerns:

- **Controllers**: Handle HTTP requests/responses
- **Application Services**: Business logic orchestration
- **Infrastructure**: Data access, external integrations, and background workers
- **Domain**: Core entities and value objects
- **Contracts**: Interfaces and DTOs for inter-layer communication

### Key Design Patterns

- **Dependency Injection (DI)**: Inversion of Control for loose coupling and testability
- **Strategy Pattern**: Document processors and metadata parsers selected dynamically by document type
- **Repository Pattern**: Abstracted data access with generic repositories
- **Unit of Work (UoW)**: Transaction management via `IRepositoryWrapper`
- **Background Services**: `EmbeddingProcessingService` monitors and triggers AI indexing

### Data Models

| Model                                | Purpose          | Key Responsibilities                                                  |
| ------------------------------------ | ---------------- | --------------------------------------------------------------------- |
| **`DatasetMetadata`**                | Core Identity    | Central record storing Title, Abstract, and unique File Identifier    |
| **`MetadataDocument`**               | Raw Storage      | Original, unparsed metadata files (JSON, ISO 19115) for auditing      |
| **`SupportingDocument`**             | Resource Link    | Tracks documentation (PDF reports) including download URLs            |
| **`DataFile`**                       | Data Payload     | Represents actual scientific data files for end-user download         |
| **`DatasetGeospatialData`**          | Location Data    | Stores geographic footprints (Bounding Boxes) for map-based discovery |
| **`DatasetRelationship`**            | Connectivity     | Maps Parent, Child, or Sibling links between scientific records       |
| **`DatasetSupportingDocumentQueue`** | AI Orchestration | Tracks datasets pending, in-progress, or completed for AI indexing    |

---

## Troubleshooting

| Issue                          | Potential Cause               | Resolution                                                            |
| ------------------------------ | ----------------------------- | --------------------------------------------------------------------- |
| **Discovery Fails**            | External API is offline       | Wait 5 minutes and retry the "Process All" command                    |
| **Status Stuck at 'Pending'**  | Background Worker inactive    | Check console logs to ensure `EmbeddingProcessingService` is active   |
| **Python Service Unreachable** | Intelligence Engine is down   | Ensure Python app is running and URL in `appsettings.json` is correct |
| **Database Locked**            | Concurrent access             | Ensure only one instance of the service is running                    |
| **Processing Timeout**         | Large dataset or slow network | Increase timeout settings or process datasets individually            |

---

## Verification & Testing

1. Navigate to: http://localhost:5133/swagger
2. Use the `/api/search/stats` endpoint to verify database connectivity
3. Trigger `/api/etl/process-all` to start ingestion
4. Monitor logs in `logs/etl-*.log` for processing status
5. Check `/api/etl/status` to track processing progress

---

## License

This project is part of the DSH ETL Search & Discovery Platform.
