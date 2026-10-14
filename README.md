<p align="center">
  <img src="PharmaPOS.Razor/wwwroot/resources/logos/logo.png" alt="PharmaPOS Logo" width="320"/>
</p>

<h1 align="center">PharmaPOS</h1>

<p align="center">
  <strong>A comprehensive Point-of-Sale & Pharmacy Management System</strong>
</p>

<p align="center">
  <a href="#features">Features</a> •
  <a href="#screenshots">Screenshots</a> •
  <a href="#tech-stack">Tech Stack</a> •
  <a href="#architecture--project-structure">Architecture</a> •
  <a href="#-cloud-infrastructure--deployment">Deployment</a> •
  <a href="#installation">Installation</a> •
  <a href="#license">License</a>
</p>

<p align="center">
  <a href="https://github.com/nomanaadma/PharmaPOS.CA/actions/workflows/iac-deploy.yaml">
    <img src="https://github.com/nomanaadma/PharmaPOS.CA/actions/workflows/iac-deploy.yaml/badge.svg" alt="IaC Deploy"/>
  </a>
  <a href="https://github.com/nomanaadma/PharmaPOS.CA/actions/workflows/deploy-app.yaml">
    <img src="https://github.com/nomanaadma/PharmaPOS.CA/actions/workflows/deploy-app.yaml/badge.svg" alt="Build and Deploy APP to Azure"/>
  </a>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 10"/>
  <img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white" alt="C#"/>
  <img src="https://img.shields.io/badge/PostgreSQL-4169E1?style=for-the-badge&logo=postgresql&logoColor=white" alt="PostgreSQL"/>
  <img src="https://img.shields.io/badge/Razor_Pages-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="Razor Pages"/>
  <img src="https://img.shields.io/badge/Terraform-844FBA?style=for-the-badge&logo=terraform&logoColor=white" alt="Terraform"/>
  <img src="https://img.shields.io/badge/Microsoft_Azure-0078D4?style=for-the-badge&logo=microsoft-azure&logoColor=white" alt="Microsoft Azure"/>
  <img src="https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white" alt="Docker"/>
  <img src="https://img.shields.io/badge/Bootstrap-4.x-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white" alt="Bootstrap"/>
  <img src="https://img.shields.io/badge/jQuery-DataTables-0769AD?style=for-the-badge&logo=jquery&logoColor=white" alt="jQuery"/>
</p>

---

## 📋 Overview

**PharmaPOS** is a full-featured pharmacy point-of-sale and inventory management web application. It streamlines pharmacy operations — from medicine inventory tracking and purchase management to customer billing, stock adjustment, and financial reporting — all within a clean, responsive dashboard interface. 

Originally built in PHP, it has been **completely re-architected into a robust, enterprise-grade .NET 10 application** utilizing Clean Architecture, CQRS, and Domain-Driven Design principles.

---

## ✨ Features

### 🏪 Point of Sale (POS) / Billing
- Create and manage customer bills with itemized medicine entries
- Auto-fetch medicine batches with sale price, tax, and discount calculations
- Support for multiple payment methods (Cash, Card, etc.)
- Bill view with printable invoice layout
- Date-range filtering and advanced search on billing records

### 💊 Medicine Management
- Add, edit, view, and delete medicines with detailed attributes (company, generic name, category, unit packing, store location, etc.)
- Batch-level tracking per medicine (batch number, expiry, quantity, sale/purchase prices)
- Medicine categories management (Tablet, Syrup, Liquid, etc.)
- Min-level and reorder-level alerts for low stock

### 📦 Purchase Management
- Record purchases from suppliers with per-batch breakdown
- Automatic stock replenishment on purchase entry
- Tax and discount applied at purchase level
- Purchase history with date-range filtering

### 📊 Stock Adjustment
- **Live Stock** — View all active inventory
- **Expired Stock** — Identify and manage expired medicines
- **Expiring Soon** — Filter medicines expiring within 1, 2, 3, or 6 months
- Manual stock quantity adjustments and batch deletion

### 👥 Customer Management
- Full customer database with name, email, phone, and gender
- Customer search with auto-complete for quick billing
- View customer-level billing history

### 📈 Reports & Analytics
- **POS/Bill Report** — Detailed billing reports with filters
- **Purchase Report** — Supplier purchase analysis
- **Inventory Report** — Stock valuation and movement
- **Income & Expense Stats** — Revenue, profit, and expense charts
- Admin dashboard with 12-month trend charts (revenue, income, transactions)

### 🔐 Authentication & User Management
- Secure cookie-based authentication via ASP.NET Core Identity
- Declarative Role and Permission-based authorization
- Granular permission system enforced via MediatR pipeline behaviors (`[AuthorizeCommand]`)

### 👤 Role-Based Access Control (RBAC)
- Custom role creation with per-route and per-command permission assignment
- Pre-configured roles for Admin, Store Manager, and Employee
- Middleware catches authorization exceptions and handles UI-friendly 403 Forbidden views securely.
- Pre-configured roles:

| Role | Description | Key Permissions |
|---|---|---|
| **Admin** | Full system access | All modules, user management, system settings, reports |
| **Store Manager** | Store operations | Billing, purchases, stock, medicines, customers, payment methods |
| **Employee** | Front-desk staff | Customer management, POS/billing (create, edit, view, delete) |

> Roles are fully customizable — create new roles and assign granular per-route permissions from the admin panel.

### ⚙️ System Settings
- Configurable pharmacy name, logo, favicon, and branding
- Contact information and address management
- Currency and date format configuration
- Supplier management (name, email, phone, address)
- Payment method configuration

---

## 📸 Screenshots

### Login
![Login](PharmaPOS.Razor/wwwroot/resources/screenshots/login.png)

### POS / Billing
| New Bill | Billing List |
|---|---|
| ![New Bill](PharmaPOS.Razor/wwwroot/resources/screenshots/new_bill.png) | ![POS](PharmaPOS.Razor/wwwroot/resources/screenshots/pos.png) |

### Medicine Management
| Medicine List | Add Medicine | Medicine Categories |
|---|---|---|
| ![Medicines](PharmaPOS.Razor/wwwroot/resources/screenshots/medicines.png) | ![Add Medicine](PharmaPOS.Razor/wwwroot/resources/screenshots/add_medicine.png) | ![Categories](PharmaPOS.Razor/wwwroot/resources/screenshots/medicine_categories.png) |

### Purchase Management
| Purchase List | Add Purchase |
|---|---|
| ![Purchases](PharmaPOS.Razor/wwwroot/resources/screenshots/purchases.png) | ![Add Purchase](PharmaPOS.Razor/wwwroot/resources/screenshots/add_purchase.png) |

### Stock Adjustment
| Live Stock | Expired Stock |
|---|---|
| ![Live Stock](PharmaPOS.Razor/wwwroot/resources/screenshots/live_stock.png) | ![Expired Stock](PharmaPOS.Razor/wwwroot/resources/screenshots/expire_stock.png) |

### Customer Management
| Customer List | Add Customer |
|---|---|
| ![Customers](PharmaPOS.Razor/wwwroot/resources/screenshots/customers.png) | ![Add Customer](PharmaPOS.Razor/wwwroot/resources/screenshots/add_customer.png) |

### Reports
![Reports](PharmaPOS.Razor/wwwroot/resources/screenshots/reports.png)

### User Management & RBAC
| Users | Add User | User Roles | Permissions |
|---|---|---|---|
| ![Users](PharmaPOS.Razor/wwwroot/resources/screenshots/users.png) | ![Add User](PharmaPOS.Razor/wwwroot/resources/screenshots/add_user.png) | ![Roles](PharmaPOS.Razor/wwwroot/resources/screenshots/user_roles.png) | ![Permissions](PharmaPOS.Razor/wwwroot/resources/screenshots/user_permissions.png) |

### Settings & Configuration
| System Settings | Suppliers | Add Supplier | Payment Methods | Add Payment Method |
|---|---|---|---|---|
| ![Settings](PharmaPOS.Razor/wwwroot/resources/screenshots/settings.png) | ![Suppliers](PharmaPOS.Razor/wwwroot/resources/screenshots/suppliers.png) | ![Add Supplier](PharmaPOS.Razor/wwwroot/resources/screenshots/add_supplier.png) | ![Payment Methods](PharmaPOS.Razor/wwwroot/resources/screenshots/payment_methods.png) | ![Add Payment](PharmaPOS.Razor/wwwroot/resources/screenshots/add_payment_method.png) |

### Database Schema (ERD)
<p align="center">
  <img src="PharmaPOS.Razor/wwwroot/resources/screenshots/erd.png" alt="PharmaPOS ERD - Database Schema" width="100%"/>
</p>

### ☁️ Cloud Infrastructure & CI/CD Architecture
| Cloud Architecture & Deployment Flow | Azure Resource Group Stack |
|---|---|
| ![Cloud Architecture](PharmaPOS.Razor/wwwroot/resources/screenshots/infra_1.jpg) | ![Azure Resource Group](PharmaPOS.Razor/wwwroot/resources/screenshots/infra_2.jpg) |

#### End-to-End CI/CD Pipeline Workflow
<p align="center">
  <img src="PharmaPOS.Razor/wwwroot/resources/screenshots/infra_3.jpg" alt="PharmaPOS CI/CD Deployment Process" width="100%"/>
</p>

---

## 🛠️ Tech Stack

### Backend & Architecture
| Technology | Purpose |
|---|---|
| **.NET 10 / C#** | Core server-side framework |
| **Clean Architecture** | Separation of concerns (Domain, Application, Infrastructure, Presentation) |
| **CQRS (MediatR)** | Separation of Commands and Queries |
| **Entity Framework Core** | ORM for PostgreSQL database interactions |
| **PostgreSQL** | Relational database |
| **FluentValidation** | Robust API and command request validation |
| **Mapster** | High-performance object mapping |
| **Outbox Pattern** | Transactional guarantees for asynchronous Domain Events (via BackgroundService) |

### Frontend
| Technology | Purpose |
|---|---|
| **ASP.NET Core Razor Pages** | Server-rendered UI |
| **Bootstrap 4** | Responsive UI framework |
| **jQuery & AJAX** | DOM manipulation and dynamic data loading |
| **DataTables** | Advanced data tables with sorting, searching, and pagination |
| **Select2 / Autocomplete** | Enhanced search inputs |
| **Chart.js** | Dashboard analytics charts |
| **Flatpickr** | Modern date picker for forms |
| **Summernote** | Rich text editor for notes |
| **Dropzone.js** | Drag-and-drop file uploads |
| **Moment.js** | Date/time parsing and formatting |
| **Line Awesome** | Icon library |
| **Inter (Google Fonts)** | Typography |

### Cloud, Infrastructure & DevOps
| Technology | Purpose |
|---|---|
| **Microsoft Azure** | Cloud hosting platform (Container Apps, ACR, PostgreSQL Flexible Server) |
| **Terraform (IaC)** | Declarative Infrastructure as Code for automated Azure provisioning |
| **Docker** | Containerization with multi-stage build optimization |
| **Azure Container Apps** | Serverless micro-container hosting with auto-scaling and HTTPS ingress |
| **Azure Container Registry** | Secure OCI private container registry for versioned image storage |
| **Azure PostgreSQL Flexible Server** | Managed PostgreSQL 16 database with SSL enforcement |
| **Azure Log Analytics** | Centralized diagnostic logging and container monitoring |
| **GitHub Actions** | Automated CI/CD pipelines for IaC and application deployment |

---

## 🏗️ Architecture & Project Structure

The solution heavily follows **Clean Architecture** and **Domain-Driven Design (DDD)**:

```
PharmaPOS.slnx
├── PharmaPOS.Domain/        # Enterprise logic, Entities, Aggregates, Domain Events
├── PharmaPOS.Contracts/     # DTOs, API Requests/Responses
├── PharmaPOS.Shared/        # Enums, Constants, Shared Utilities
├── PharmaPOS.Application/   # Use cases (CQRS), MediatR Handlers, Validators, Authorization Behaviors
├── PharmaPOS.Infrastructure/# EF Core DbContext, Repositories, Background Jobs (OutboxProcessor)
├── PharmaPOS.Razor/         # Presentation Layer (Razor Pages, Program.cs, UI Assets, Middlewares)
├── iac/                     # Infrastructure as Code (Terraform configs for Azure resources)
└── .github/workflows/       # Automated CI/CD Pipelines (IaC deploy, App build & deploy)
```

**Key Architectural Patterns:**
- **Outbox Pattern:** When domain events (like `BillDeletedEvent`) are raised, they are serialized to an `OutboxMessages` table within the same database transaction. A hosted background service (`OutboxMessageProcessor`) safely processes them asynchronously with Polly retry pipelines.
- **Pipeline Behaviors:** MediatR behaviors are used for Cross-Cutting Concerns like **Validation** and **Authorization** (`[AuthorizeCommand]`).
- **Rich Domain Model:** Business rules and state changes (like updating stock quantities) are encapsulated inside Domain Aggregate Roots (e.g. `Bill`, `Batch`).

### Database Schema & Tables

The database consists of the following tables:

| Table | Description |
|---|---|
| `customers` | Customer records |
| `medicines` | Medicine catalog |
| `medicine_category` | Medicine categories (Tablet, Syrup, etc.) |
| `medicine_batch` | Batch-level stock tracking (qty, expiry, prices) |
| `medicine_purchase` | Purchase orders from suppliers |
| `medicine_bill` | Sales/billing records |
| `bill_items` | Line items per bill |
| `suppliers` | Supplier contacts |
| `payment_method` | Payment method configuration |
| `setting` | System settings (JSON) |
| `media` | Uploaded media files |
| `AspNetRoles` | Stores application roles (e.g., Admin, Employee, Store Manager) |
| `AspNetUserRoles` | Junction table mapping users to their assigned roles |
| `AspNetRoleClaims` | Stores permissions granted to specific roles |
| `AspNetUserClaims` | Stores custom claims of users such as gender, fullname |

---

## ☁️ Cloud Infrastructure & Deployment

PharmaPOS is fully automated for cloud-native deployment to **Microsoft Azure**, using **Terraform** for Infrastructure as Code (IaC) and **GitHub Actions** for end-to-end continuous integration and continuous deployment (CI/CD).

[![IaC Deploy](https://github.com/nomanaadma/PharmaPOS.CA/actions/workflows/iac-deploy.yaml/badge.svg)](https://github.com/nomanaadma/PharmaPOS.CA/actions/workflows/iac-deploy.yaml)
[![Build and Deploy APP to Azure](https://github.com/nomanaadma/PharmaPOS.CA/actions/workflows/deploy-app.yaml/badge.svg)](https://github.com/nomanaadma/PharmaPOS.CA/actions/workflows/deploy-app.yaml)

### 🏛️ Azure Cloud Architecture (`/iac`)

All cloud infrastructure is declared as code inside the [`iac/`](iac/) directory using HashiCorp Terraform and the `azurerm` provider (v4.7.0). State is managed remotely and locked in an Azure Blob Storage container (`myiacfile/terraform`).

The provisioned architecture includes:

- **Azure Container Apps (ACA)** (`pca-razor-app-dev`):
  - Serverless micro-container hosting the ASP.NET Core Razor Pages application.
  - Configured with automatic horizontal scaling (1 to 2 replicas), 0.25 vCPU, and 0.5 GiB memory.
  - Single revision mode with zero-downtime rolling deployments and external HTTPS ingress on port `8080`.
- **Azure Container Registry (ACR)** (`pcaregistrydev`):
  - Secure private container registry storing versioned Docker images tagged by commit SHA and `latest`.
- **Azure Database for PostgreSQL Flexible Server** (`pharmapos-psql-deva1q`):
  - Managed PostgreSQL 16 Flexible Server instance with 32 GB storage (`B_Standard_B1ms` SKU).
  - Secure SSL enforcement and Azure firewall rules configured for container app access.
- **Azure Log Analytics Workspace (LAW)** (`pcalawdev`):
  - Centralized log aggregation and metrics monitoring with 30-day retention.
- **Azure Container App Environment** (`pcaappenvdev`):
  - Secure managed network environment binding the container apps to the Log Analytics workspace.
- **Azure Resource Group** (`pca-rg`):
  - Located in the **East Asia** region with standard environment tagging.

### 🔄 CI/CD Pipelines (GitHub Actions)

The repository features two automated, decoupled GitHub Actions workflows under [`.github/workflows/`](.github/workflows/):

#### 1. Infrastructure as Code Pipeline (`iac-deploy.yaml`)
Triggered automatically on pushes to `master` affecting the `iac/**` directory:
- **Authentication**: Connects to Azure using an Azure Service Principal (`AZURE_CREDENTIALS`).
- **Validation**: Installs Terraform v1.16.5, runs `terraform init` against remote Azure Blob storage, and validates configuration syntax (`terraform validate`).
- **Execution**: Generates a deterministic plan (`terraform plan`) injecting the database password via secrets, and applies infrastructure changes (`terraform apply -auto-approve`).

#### 2. Application Build & Deploy Pipeline (`deploy-app.yaml`)
Triggered on code commits to `master` (ignoring changes confined to `iac/**`):
- **Job 1: Build & Push (`build-and-deploy`)**:
  1. Sets up the .NET 10 SDK environment.
  2. Authenticates to Azure Container Registry (`azure/docker-login@v2`).
  3. Computes the git commit short SHA (`git rev-parse --short`).
  4. Builds the container image using the multi-stage [PharmaPOS.Dockerfile](PharmaPOS.Dockerfile).
  5. Pushes the image tagged with both the commit SHA (`$ACR_REGISTRY/pcaapp:$IMAGE_TAG`) and `latest`.
- **Job 2: Deploy Container App (`deploy-container-to-ACS`)**:
  1. Authenticates to Azure via Service Principal credentials.
  2. Uses `azure/container-apps-deploy-action@v1` to trigger a revision rollout in Azure Container Apps.
  3. Injects runtime configuration and connection strings:
     - `ASPNETCORE_ENVIRONMENT=Development`
     - `ASPNETCORE_URLS=http://+:8080`
     - `ConnectionStrings__DefaultConnection` (Secure PostgreSQL connection string)

### 🐳 Containerization & Local Development

The application can also be run locally using Docker:

- **Multi-Stage Dockerfile**: [PharmaPOS.Dockerfile](PharmaPOS.Dockerfile) uses `mcr.microsoft.com/dotnet/sdk:10.0` for cached building and publishing, and copies binaries into a lightweight `mcr.microsoft.com/dotnet/aspnet:10.0` runtime image exposing port `8080`.
- **Docker Compose**: [docker-compose.yml](docker-compose.yml) spins up a local PostgreSQL instance:
  ```bash
  docker compose up -d
  ```


---

## 🚀 Installation

### Prerequisites
- **.NET 10 SDK**
- **PostgreSQL** (running locally or via Docker)

### Step 1: Clone the Repository
```bash
git clone https://github.com/nomanaadma/PharmaPOS.CA.git
cd PharmaPOS
```

### Step 2: Database Setup
1. Create a PostgreSQL database (e.g., `pharmapos`).
2. Update the connection string in `PharmaPOS.Razor/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=5432;Database=pharmapos;User ID=postgres;Password=your_password;"
}
```
3. Apply Entity Framework Migrations to generate the schema:
```bash
dotnet ef database update --project PharmaPOS.Infrastructure --startup-project PharmaPOS.Razor
```

### Step 3: Run the Application
```bash
dotnet run --project PharmaPOS.Razor
```

The application will be accessible at `https://localhost:5001` or `http://localhost:5000`.

---

## 🔒 Security Features

- **Defense-in-Depth Authorization** — Enforced both at the Razor Page route level and deep within the Application layer via MediatR pipelines.
- **Server-Side Data Validation** — Critical financial math (billing totals, tax, discounts) is strictly re-calculated and verified server-side.
- **CSRF Protection** — ASP.NET Core built-in Antiforgery token validation.
- **XSS Prevention** — Razor's automatic HTML encoding.
- **SQL Injection Prevention** — Via Entity Framework Core parameterized queries.

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/your-feature`)
3. Commit your changes (`git commit -m 'Add your feature'`)
4. Push to the branch (`git push origin feature/your-feature`)
5. Open a Pull Request

---

## 📄 License

This project is open source and available under the [MIT License](LICENSE).

---

<p align="center">
  Made with ❤️ By Noman Shoukat
</p>
