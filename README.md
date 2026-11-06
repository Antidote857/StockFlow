# StockFlow – Inventory Management REST API

StockFlow is a RESTful inventory management API built with ASP.NET Core and C#. It helps manage products, track inventory movements, monitor stock levels and identify products that need replenishment.

## Features

- Product creation, retrieval, updating and deletion.
- Inventory movement tracking:
  - Received stock
  - Sold stock
  - Damaged stock
  - Manual stock adjustments
- Automatic stock quantity updates.
- Low-stock monitoring and reorder calculations.
- Prevention of negative inventory quantities.
- Duplicate SKU detection.
- Protection against deleting products with movement history.
- DTO-based API requests and responses.
- Input validation and meaningful HTTP error responses.
- Interactive Swagger API documentation.
- SQL Server database integration using Entity Framework Core.

## Technology Stack

- C#
- ASP.NET Core Web API
- .NET 10
- Entity Framework Core 10
- Microsoft SQL Server
- Swagger / OpenAPI
- Visual Studio Code

## Requirements

- .NET 10 SDK
- Microsoft SQL Server
- SQL Server Management Studio (optional)
- Visual Studio Code or another C# editor

## Getting Started

### 1. Clone the repository

```bash
git clone YOUR_GITHUB_REPOSITORY_URL
cd StockFlow
```

### 2. Configure the database

Open `appsettings.json` and configure your SQL Server connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StockFlowDb;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;"
  }
}
```

Adjust the server name and authentication settings to match your SQL Server installation.

### 3. Restore dependencies

```bash
dotnet restore
```

### 4. Apply database migrations

```bash
dotnet ef database update
```

If the EF Core command is unavailable, install the EF Core command-line tool:

```bash
dotnet tool install --global dotnet-ef
```

### 5. Run the application

```bash
dotnet run
```

The API will be available at the local address shown in the terminal.

### 6. Open Swagger

When running in the Development environment, open:

```text
http://localhost:5224/swagger
```

Swagger provides an interactive interface for exploring and testing the API endpoints.

## API Endpoints

### Products

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/products` | Retrieve all products |
| GET | `/api/products/{id}` | Retrieve a product |
| POST | `/api/products` | Create a product |
| PUT | `/api/products/{id}` | Update a product |
| DELETE | `/api/products/{id}` | Delete a product |
| GET | `/api/products/low-stock` | Retrieve low-stock products |

### Inventory Movements

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/inventorymovements` | Retrieve all movements |
| GET | `/api/inventorymovements/{id}` | Retrieve a movement |
| GET | `/api/inventorymovements/product/{productId}` | Retrieve movements for a product |
| POST | `/api/inventorymovements` | Record an inventory movement |

## Inventory Movement Types

| Value | Movement | Stock Effect |
|---|---|---|
| 0 | Received | Increases stock |
| 1 | Sold | Decreases stock |
| 2 | Damaged | Decreases stock |
| 3 | Adjustment | Increases or decreases stock |

Movement quantities must follow the validation rules for each movement type. Adjustments cannot have a quantity of zero.

## Low-Stock Monitoring

Products are considered low-stock when:

```text
QuantityInStock <= ReorderLevel
```

The low-stock endpoint also returns:

- Current stock quantity
- Reorder level
- Units to reorder
- Stock status

## Error Handling

StockFlow uses standard HTTP status codes, including:

| Code | Meaning |
|---|---|
| 200 | Successful request |
| 201 | Resource created |
| 204 | Resource updated or deleted |
| 400 | Invalid request or insufficient stock |
| 404 | Resource not found |
| 409 | Conflict, such as a duplicate SKU |

## Database and Architecture

The application uses Entity Framework Core for database access and SQL Server for persistent storage.

Database changes are managed through EF Core migrations.

DTOs separate API request and response structures from database entities.

Inventory movements update product stock quantities, while validation prevents stock from becoming negative or exceeding the supported integer range.

## Security Notes

The current project is intended as a development and portfolio demonstration.

- Configure database credentials securely for your own environment.
- Do not commit passwords, API keys or private secrets.
- Swagger is enabled in the Development environment.
- Authentication and authorization should be added before exposing the API in a production environment.

## Project Structure

```text
StockFlow/
├── Controllers/
├── Data/
├── DTOs/
├── Migrations/
├── Models/
├── Properties/
├── Program.cs
├── StockFlow.csproj
├── appsettings.json
├── appsettings.Development.json
└── README.md
```

## Future Improvements

- Automated unit and integration tests.
- Authentication and role-based authorization.
- Inventory reporting and analytics.
- Pagination and filtering.
- Automated low-stock notifications.
- Frontend dashboard.

## Author

Developed as a portfolio project to demonstrate practical C#, ASP.NET Core Web API, Entity Framework Core, SQL Server and REST API development skills.
