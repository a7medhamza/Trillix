# Trillix

An end-to-end e-commerce platform built from scratch with **ASP.NET Core Minimal APIs**, **EF Core (Code First)** and **SQL Server**.

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
| **1. Admin: product management** | Product CRUD, image upload, stock, admin UI | In progress |
| **2. Identity** | Admin login, JWT, role policies (Admin / Customer), refresh tokens | In progress |
| **3. Storefront (catalog)** | Public product listing, product details, search and filtering | Planned |
| **4. Basket** | Add, update and remove items, persisted per customer | Planned |
| **5. Ordering** | Checkout, order lifecycle (status workflow), order history | Planned |
| **6. Payments** | Online payment integration (sandbox), idempotent payment handling | Planned |
| **7. Productionisation** | CI/CD, Docker, cloud deployment, monitoring | Planned |

Categories, product variants, discounts and reviews are deliberately out of scope until the core flow works end to end.

---

## Tech stack

**Backend**
- **.NET 10**, ASP.NET Core **Minimal APIs**
- **EF Core 8** with **Code First** migrations
- **SQL Server**
- **JWT Bearer** authentication, custom **Admin** identity (Argon2 passwords, no ASP.NET Identity)
- **Konscious.Security.Cryptography.Argon2** for password hashing

**Admin UI** (static, served from `wwwroot`)
- **DaisyUI 5** + **Tailwind CSS 4** (browser CDN)
- Vanilla JavaScript (`fetch`, shared `api.js` / `layout.js` / `ui.js`)
- Global theme in `wwwroot/Trillix/css/theme.css` (see `.cursor/rules/trillix-ui.mdc`)

**Planned**
- Customer registration and storefront UI
- FluentValidation, Serilog, Redis caching
- Unit and integration tests, Docker, CI/CD, Azure deployment

---

## Architecture

Trillix is a **modular monolith**: one deployable app with feature modules and clear boundaries.

```
Trillix/
├── Common/                     BaseEntity, AuditableEntity
├── Data/                       AppDbContext, migrations
├── Modules/
│   ├── Catalog/                Products, images, admin product APIs
│   │   └── Storage/            IImageStorage, local disk implementation
│   └── Identity/               Admins, JWT, refresh tokens, auth endpoints
├── wwwroot/
│   └── Trillix/
│       ├── css/theme.css       Design tokens (single source of truth)
│       ├── js/                 api, layout, ui helpers
│       ├── index.html          Storefront placeholder (/Trillix/)
│       └── administration/     Admin pages (/Trillix/administration/)
└── Program.cs
```

Planned modules: `Basket`, `Ordering`, `Payments`.

### Key design decisions

- **Code First + migrations**: schema versioned in Git.
- **Images on disk, metadata in SQL** behind `IImageStorage`.
- **Upload validation**: size limit, extension allow-list, magic-byte check.
- **Product soft delete**: `IsDeleted` on `BaseEntity`; list endpoints exclude deleted rows.
- **Optimistic concurrency** on `Product` (`RowVersion`).
- **Money as `decimal(18,2)`**.
- **Admin JWT** with policies `Admin` and `Client` (Customer APIs later).
- **URL layout**: storefront under `/Trillix/`, admin under `/Trillix/administration/` (`<base href>` on each page).

---

## Getting started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) matching the project (`net10.0` in `Trillix.csproj`)
- SQL Server (LocalDB, Express or Developer)
- EF Core CLI: `dotnet tool install --global dotnet-ef`

### Setup

```bash
git clone https://github.com/<your-username>/Trillix.git
cd Trillix

dotnet ef database update
dotnet run
```

### Configuration

`appsettings.json` defines placeholders for the connection string, image storage path and JWT settings. Use **User Secrets** (or environment variables) for real values:

```bash
dotnet user-secrets set "ConnectionStrings:Default" "<your-connection-string>"
dotnet user-secrets set "Storage:ImagesPath" "uploads/products"
dotnet user-secrets set "Jwt:SigningKey" "<at-least-32-characters>"
dotnet user-secrets set "SeedAdmin:Email" "admin@example.com"
dotnet user-secrets set "SeedAdmin:Password" "<min-12-chars>"
dotnet user-secrets set "SeedAdmin:FullName" "Administrator"
```

On first run, if no admin exists and `SeedAdmin:*` is set, an initial admin account is created.

### Admin UI

After `dotnet run`, open:

| Page | URL |
|---|---|
| Sign in | `/Trillix/administration/login.html` |
| Products | `/Trillix/administration/products.html` |
| Add / edit product | `/Trillix/administration/add-product.html` (edit: `?id=<guid>`) |

Sign in stores the access token in `sessionStorage`. Protected API routes require `Authorization: Bearer <token>`.

### Database

```bash
dotnet ef migrations add <MigrationName> -o Data/Migrations
dotnet ef database update
```

Apply schema changes through migrations only, not manual edits in SSMS.

---

## API overview

All `/api/admin/*` routes require the **Admin** JWT policy unless noted.

### Auth

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/admin/auth/login` | Email + password → `accessToken`, `refreshToken`, `expiresIn` |
| `POST` | `/api/admin/auth/refresh` | Rotate refresh token |
| `POST` | `/api/admin/auth/logout` | Revoke refresh token |

### Products (admin)

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/admin/products` | List products (with `images[]`), excludes soft-deleted |
| `GET` | `/api/admin/products/{id}` | Single product with images |
| `POST` | `/api/admin/products` | Create product |
| `PUT` | `/api/admin/products/{id}` | Update catalog fields (name, slug, SKU, description, `isActive`, `isDeleted`) — not price/stock |
| `PATCH` | `/api/admin/products/{id}/inventory` | Update `price` and `stockQuantity` only |
| `DELETE` | `/api/admin/products/{id}` | Soft delete |
| `POST` | `/api/admin/products/{id}/images` | Upload image (`multipart/form-data`, field `file`, max 5 MB, JPG/PNG/WEBP) |

Static product images: `/media/products/{fileName}`.

---

## Learning goals

- Translate business requirements into a system design
- Structure a codebase as a modular monolith with clear boundaries
- Handle real e-commerce concerns: file uploads, inventory, auth, order workflows (planned)
- Apply testing, CI/CD and deployment practices
- Document and manage a project through issues and phased delivery

---

## Author

**Ahmed Hamza Gouissem**: [GitHub](https://github.com/a7medhamza) · [LinkedIn](https://linkedin.com/in/ahmed-hamza-gouissem)
