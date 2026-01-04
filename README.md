[[_TOC_]]

# **Overview**

The **Dataset Ingestion Hub** is the administrative gateway for the ETL (Extract, Transform, Load) pipeline. It manages the lifecycle of dataset records by discovering updates from remote sources, extracting detailed metadata, and tracking the status of AI-powered document analysis.

# **Requirements**
- **Runtime**: .NET 8.0 / .NET 9.0
- **Database**: SQLite (`etl_database.db`)
- **Integration**: Requires a connection to the Python Search Service for vector indexing.

# **Setup**

## **Service Registration**

The Hub utilizes a background worker (`EmbeddingProcessingService`) to monitor and trigger processing.

1. Ensure the `EtlSettings:PythonServiceUrl` in `appsettings.json` is reachable.
2. The system will automatically initialize the SQLite database on first launch.

# **Data Models**

The following models define the core entities within the ingestion ecosystem:

| Model | Purpose | Key Responsibilities |
|--|--|--|
| **`DatasetMetadata`** | **Core Identity** | Acts as the central record for a dataset. Stores Title, Abstract, and unique File Identifier. |
| **`MetadataDocument`** | **Raw Storage** | Stores original, unparsed metadata files (e.g., JSON or ISO 19115) for auditing. |
| **`SupportingDocument`** | **Resource Link** | Tracks documentation related to a dataset (e.g., PDF reports) including download URLs. |
| **`DataFile`** | **Data Payload** | Represents the actual scientific data files intended for end-user download. |
| **`DatasetGeospatialData`** | **Location Data** | Stores geographic footprints (Bounding Boxes) for map-based discovery. |
| **`DatasetRelationship`** | **Connectivity** | Maps Parent, Child, or Sibling links between different scientific records. |
| **`DatasetSupportingDocumentQueue`** | **AI Orchestration** | Tracks which datasets are pending, in-progress, or completed for AI indexing. |

# **User Guide**

## **The Data Journey**

The Hub tracks datasets through three primary states in the workflow:

|Phase|Process|Description|
|--|--|--|
|**1. Discovery**|Remote Synchronization|Scans external APIs to identify new datasets or updates to existing records.|
|**2. Extraction**|Metadata Ingestion|Parses raw documents to store Title, Abstract, and resource links.|
|**3. Queueing**|Intelligence Scheduling|Flags the dataset in the Queue to notify the AI engine that content is ready.|

# **Operating Instructions**

## **Triggering a Processing Cycle**
1. **Discovery**: Use the Hub to perform a "Discover All" operation. This populates the local database with new dataset "stubs."
2. **Extraction**: Once discovered, trigger the Extraction service. This reads full metadata and finds links to PDFs and Word documents.
3. **Queue Monitoring**: The Hub will automatically add these records to the **AI Queue**. No manual file preparation is required.

# **Troubleshooting**

|Issue|Potential Cause|Resolution|
|--|--|--|
|**Discovery Fails**|External API is offline.|Wait 5 minutes and retry the "Discover All" command.|
|**Status Stuck at 'Pending'**|Background Worker is inactive.|Check console logs to ensure `EmbeddingProcessingService` is active.|
|**'Python Service Unreachable'**|Intelligence Engine is down.|Ensure the Python app is running and the URL in `appsettings.json` is correct.|

# **Accessing the Interface**

To interact with the Hub manually, use the built-in Swagger UI:
1. Start the application.
2. Navigate to: `http://localhost:PORT/swagger`
3. Manually trigger **Discovery** and **Extraction** by clicking "Try it out."

