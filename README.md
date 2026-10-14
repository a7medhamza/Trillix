# Trillix

An end-to-end e-commerce platform built from scratch with **ASP.NET Core (.NET 8) Minimal APIs**, **EF Core (Code First)** and **SQL Server**.

Trillix is a learning and portfolio project. Its purpose is to practice **system and platform design** and to **manage a project from A to Z**: analysing requirements, designing the architecture, planning the work, implementing it, testing it and shipping it.

---

## Vision

The platform serves two actors:

| Actor | Role | Main goals |
|---|---|---|
| **Admin** (the seller) | Runs the shop | Manage the product catalog, images, prices and stock; later, process orders |
| **Customer** (the buyer) | Uses the storefront | Browse and search products, manage a basket, place and track orders, pay online |

## Project management

The business requirements (the *cahier des charges*) live in this repository's **[Issues](../../issues)**. Each issue describes a piece of business logic and acts as the specification for what gets built. Work is delivered in phases, and each phase closes a set of issues.

---

## Roadmap

| Phase | Scope | Status |
|---|---|---|
| **1. Admin: product management** | Product CRUD, image upload and management, stock | In progress |
| **2. Identity** | Registration, login, roles (Admin / Customer), JWT authentication | Planned |
| **3. Storefront (catalog)** | Public product listing, product details, search and filtering | Planned |
| **4. Basket** | Add, update and remove items, persisted per customer | Planned |
| **5. Ordering** | Checkout, order lifecycle (status workflow), order history | Planned |
| **6. Payments** | Online payment integration (sandbox), idempotent payment handling | Planned |
| **7. Productionisation** | CI/CD, Docker, cloud deployment, monitoring | Planned |

Categories, product variants, discounts and reviews are deliberately out of scope until the core flow works end to end.

---

## Tech stack

**Backend**
- .NET 8 (LTS), ASP.NET Core **Minimal APIs**
- **EF Core 8** with the **Code First** approach and migrations
- **SQL Server** (managed and inspected with SSMS)

**Planned**
- Authentication and authorization with JWT and role-based policies
- Validation (FluentValidation), structured logging (Serilog)
- Caching with Redis (basket, catalog)
- Unit and integration tests
- Docker, GitHub Actions CI/CD, Azure deployment
- A separate front end (admin back-office and storefront), technology to be decided

---

## Architecture

Trillix starts as a **modular monolith**: a single deployable application split into modules with clear boundaries. This keeps the project simple to run and debug while leaving room to extract modules into services later if needed.

Code is organised **by feature (vertical slices)** rather than by technical layer.

```
Trillix/
├── Common/                  Shared base classes and helpers
│   └── BaseEntity.cs
├── Data/                    DbContext, configurations, migrations
│   └── AppDbContext.cs
├── Modules/
│   └── Catalog/             Products and product images
│       ├── Product.cs
│       ├── ProductImage.cs
│       ├── ProductEndpoints.cs
│       └── Storage/         Image storage abstraction and local implementation
└── Program.cs               Composition root
```

Planned modules: `Identity`, `Basket`, `Ordering`, `Payments`.

### Key design decisions

- **Code First + migrations**: C# entities are the source of truth; the schema is versioned in Git and reproducible on any machine.
- **Images stored on disk, metadata in the database**: only the generated file name is persisted in SQL Server. Storage sits behind an `IImageStorage` interface so local disk can be swapped for Azure Blob Storage without touching the endpoints.
- **Upload validation**: file size limit, extension allow-list and file-signature (magic bytes) check, with server-generated file names.
- **Optimistic concurrency**: `Product` carries a `RowVersion` so concurrent stock updates cannot silently overwrite each other.
- **Money as `decimal(18,2)`**, never floating point.
- **Audit fields** (`CreatedAt`, `UpdatedAt`) set automatically through a shared `BaseEntity`.
- **GUID identifiers**, safe to expose in URLs and friendly to a future split into services.
- **Price snapshots on orders** (planned): order items will store the name and unit price at purchase time so later price changes never rewrite history.

---

## Getting started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, Express or Developer edition)
- [SSMS](https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms) (optional, to browse the database)
- EF Core CLI tools: `dotnet tool install --global dotnet-ef`

### Setup

```bash
# 1. Clone the repository
git clone https://github.com/<your-username>/Trillix.git
cd Trillix

# 2. Configure the connection string (see below)

# 3. Create the database
dotnet ef database update

# 4. Run the API
dotnet run
```

### Configuration

`appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\MSSQLLocalDB;Database=Trillix;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Storage": {
    "ImagesPath": "uploads/products"
  }
}
```

Use the same server name you use to connect in SSMS. Never commit real credentials. For SQL authentication or any secret, use user secrets:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "<your-connection-string>"
```

### Working with the database

```bash
# Add a migration after changing entities
dotnet ef migrations add <MigrationName> -o Data/Migrations

# Apply migrations
dotnet ef database update
```

Schema changes are made through code and migrations only, never by editing tables manually in SSMS.

---

## API overview

> The API is under active development. Endpoints below reflect the current phase.

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/admin/products/{id}/images` | Upload an image for a product |

Uploaded images are served from `/media/products/{fileName}`. Admin routes will be protected with authentication and an Admin role in phase 2.

You can test requests with the `Trillix.http` file in Visual Studio.

---

## Learning goals

- Translate business requirements into a system design
- Structure a codebase as a modular monolith with clear boundaries
- Handle real e-commerce concerns: file uploads, inventory consistency, order workflows, idempotent payments
- Apply testing, CI/CD and deployment practices
- Document and manage a project through issues and phased delivery

---

## Author

**<Your name>**: [GitHub](https://github.com/a7medhamza) · [LinkedIn](https://linkedin.com/in/ahmed-hamza-gouissem)
