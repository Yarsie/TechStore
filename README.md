# TechStore

A pet project of an e-commerce Web API built with **C# and ASP.NET Core .NET 8**.

The main goal of the project is to practice backend development and work with databases, authentication, caching, messaging, API querying, and external services.

## Features

- User registration and login with JWT authentication
- Role-based authorization (`Admin`, `Manager`, `Customer`)
- Product and category management
- Shopping cart and order checkout
- PostgreSQL database with Entity Framework Core
- Redis caching
- RabbitMQ messaging with MassTransit
- OData endpoints for querying products and categories
- Input validation with FluentValidation
- Logging with Serilog
- Swagger / OpenAPI
- AI-powered product description generation using Ollama
- Unit testing with xUnit, Moq, and EF Core In-Memory Database

## Technologies

- **C#**
- **.NET 8**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **PostgreSQL**
- **Redis**
- **RabbitMQ / MassTransit**
- **JWT**
- **OData**
- **FluentValidation**
- **Serilog**
- **Ollama**
- **Swagger / OpenAPI**
- **xUnit**
- **Moq**

## Project Structure

```text
TechStore/
├── TechStore.Api/            # API controllers and configuration
├── TechStore.Application/    # Services, DTOs, interfaces and validation
├── TechStore.Domain/         # Entities and business models
├── TechStore.Infrastructure/ # Database and external service implementations
├── TechStore.Tests/          # Unit tests for services and AI integration
└── docker-compose.yml        # Supporting services
```

## Getting Started

### Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/)

### Run the project

Clone the repository:

```bash
git clone https://github.com/Yarsie/TechStore.git
cd TechStore
```

Start the required services:

```bash
docker compose up -d
```

Run the API:

```bash
dotnet run --project TechStore.Api
```

After starting the application, open Swagger to explore and test the API.

The exact URL and port can be found in:

```text
TechStore.Api/Properties/launchSettings.json
```

### Run Tests

To execute unit tests:

```bash
dotnet test TechStore.Tests
```

## AI Product Descriptions

The project uses **Ollama** to generate product descriptions based on product information and technical specifications.

The generated description can be reviewed and edited before saving the product.

The application uses the `llama3.2:3b` model.

## API

The API includes endpoints for:

- Authentication
- Users
- Products
- Categories
- Shopping cart
- Orders
- AI product descriptions
- OData product and category queries

Swagger provides an interactive way to view and test the available endpoints.
