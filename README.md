<p align="center">
  <img src="https://raw.githubusercontent.com/devicons/devicon/master/icons/dot-net/dot-net-original.svg" alt="ASP.NET Core" width="70" height="70" />
</p>

<h1 align="center">Sistem Optimasi Rute - Web API Backend</h1>

<p align="center">
  <b>High-performance ASP.NET Core 8 Web API for courier delivery route optimization powered by the Ant Colony Optimization (ACO) algorithm.</b>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/C%23-12-239120?style=flat&logo=csharp&logoColor=white" alt="C#" />
  <img src="https://img.shields.io/badge/Entity%20Framework%20Core-8.0-blue?style=flat" alt="EF Core 8" />
  <img src="https://img.shields.io/badge/Microsoft%20SQL%20Server-CC292B?style=flat&logo=microsoftsqlserver&logoColor=white" alt="SQL Server" />
  <img src="https://img.shields.io/badge/JWT-Authentication-black?style=flat&logo=jsonwebtokens" alt="JWT" />
  <img src="https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?style=flat&logo=swagger&logoColor=black" alt="Swagger" />
</p>

---

## 📌 Overview

**Sistem Optimasi Rute Web API** is the core backend service designed for **PT Pos Indonesia (KCP 44188 Malangbong)**. It handles courier dispatching, batch shipment ingestion, distance matrix calculations, and implements a metaheuristic **Ant Colony Optimization (ACO)** engine to solve the Traveling Salesperson Problem (TSP) / Vehicle Routing Problem (VRP) for couriers (*Oranger Antaran*).

The service exposes RESTful endpoints consumed by:
1. **Web Dispatcher Dashboard** (ASP.NET Core MVC) &mdash; for Branch Managers to manage shipments, trigger route optimizations, and assign tasks.
2. **Mobile Courier Application** (Flutter) &mdash; for couriers to receive optimized routes, navigate deliveries, and submit Proof of Delivery (POD).

---

## 🚀 Key Features

- **Ant Colony Optimization (ACO) Engine:**
  - Automated distance & time matrix calculation using geographic coordinates.
  - Multi-iteration pheromone trail updating and heuristic distance evaluation to find shortest, most efficient delivery sequences.
- **Shipment Management (`Kiriman`):**
  - Full CRUD operations and status lifecycle (`Menunggu penugasan pengantaran`, `Sedang Berjalan`, `Selesai`).
  - High-speed Excel spreadsheet upload & bulk insertion using **EPPlus**.
- **Courier Assignment & Dispatching (`Penugasan`):**
  - Real-time courier allocation, payload capacity checks, and assignment status tracking.
- **Proof of Delivery (POD) Service:**
  - Endpoint for couriers to submit digital delivery proof (recipient name, GPS coordinates, timestamp, and photo evidence).
- **Security & Authentication:**
  - JWT Bearer token authentication with role-based claims (`Branch Manager`, `Oranger Antaran`).
  - Password hashing utilizing **BCrypt.Net-Next**.
  - OTP email verification for password recovery via SMTP.
- **Interactive Documentation:** Built-in Swagger UI with Bearer Token authorization support.

---

## 🛠️ Technology Stack

| Layer | Technologies |
|---|---|
| **Framework** | ASP.NET Core 8.0 Web API |
| **Language** | C# 12 |
| **Database & ORM** | Microsoft SQL Server, Entity Framework Core 8 |
| **Security** | JWT (JSON Web Tokens), BCrypt.Net-Next |
| **Data Processing** | EPPlus 8 (Excel Import / Export) |
| **Documentation** | Swagger / Swashbuckle OpenAPI |
| **Architecture** | Repository Pattern with Interface Abstraction & DTOs |

---

## 📂 Project Architecture

```plaintext
SistemOptimasiWebAPI/
├── Controllers/              # API Controllers (Auth, Kiriman, Penugasan, POD, etc.)
├── Data/                     # DbContext & Entity Framework mappings
├── DTOs/                     # Data Transfer Objects (Requests & Responses)
├── Interfaces/               # Repository interfaces for dependency injection
├── Models/
│   ├── DTO/                  # Optimization & routing DTOs
│   └── Entity/               # Database entity models (Kiriman, Penugasan, Pengguna, etc.)
├── Repositories/             # Business logic & ACO optimization implementation
│   ├── OptimizationRepository.cs  # Ant Colony Optimization metaheuristic algorithm
│   ├── KirimanRepository.cs       # Shipment data access
│   ├── PenugasanRepository.cs     # Assignment lifecycle
│   └── ...
├── Properties/
│   └── launchSettings.json   # Kestrel local network profiles (Default: Port 21001)
├── appsettings.json          # Database connection strings, JWT, and SMTP settings
└── Program.cs                # Service container DI, CORS policy, and middleware pipeline
```

---

## ⚙️ Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher
- [Microsoft SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express, or Azure SQL)
- Visual Studio 2022 / JetBrains Rider / VS Code with C# Dev Kit

### Installation & Configuration

1. **Clone the repository:**
   ```bash
   git clone https://github.com/tijartaufikrahman/pos-route-optimization-api.git
   cd pos-route-optimization-api
   ```

2. **Configure Database & Secrets:**
   Open `appsettings.json` and adjust your connection string and credentials:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=db_sistem_optimasi;Trusted_Connection=True;TrustServerCertificate=True;"
     },
     "Jwt": {
       "Key": "YOUR_STRONG_SECRET_KEY_HERE_MIN_32_CHARACTERS",
       "Issuer": "PosMalangbongAPI",
       "Audience": "PosMalangbongApp"
     }
   }
   ```

3. **Restore & Build:**
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Run the API:**
   ```bash
   dotnet run --launch-profile "http"
   ```

5. **Access Swagger UI:**
   Navigate to [http://localhost:21001/swagger](http://localhost:21001/swagger) in your browser.

---

## 🔑 Default User Roles

| Role | Default Username | Default Role ID | Scope |
|---|---|---|---|
| **Branch Manager** | `branch_manager` | `1` | Full administrative control, route scheduling & monitoring |
| **Oranger Antaran** | `oranger_antaran` | `2` | Mobile courier access, route retrieval & POD submission |

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
