# ITSM — IT Service Management System [![.NET CI](https://github.com/maksim-chmel/ITSM-IT-Service-Management-System/actions/workflows/main.yml/badge.svg)](https://github.com/maksim-chmel/ITSM-IT-Service-Management-System/actions/workflows/main.yml) [![Build and deploy](https://github.com/maksim-chmel/ITSM-IT-Service-Management-System/actions/workflows/azure-deploy.yml/badge.svg)](https://github.com/maksim-chmel/ITSM-IT-Service-Management-System/actions/workflows/azure-deploy.yml)

A full-featured IT service management web application built with ASP.NET Core 8 MVC. The system supports the full ticket lifecycle, from submission by a user to resolution by a technician, featuring an automated assignment engine, a knowledge base, team discussions, and a soft-delete archive.

The project runs locally via Docker Compose and is deployed to Azure App Service with CI/CD via GitHub Actions and Azure DevOps Pipelines.

---

## Features

- **Full Ticket Lifecycle:** Create, assign, track, resolve, and reopen tickets with a complete history log.
- **Automated Assignment Engine:** Automatically assigns tickets to the best-suited technician based on category, priority, skill level, and current workload.
- **Role-Based Access Control:** Pre-configured roles (User, Technician, Coordinator, Admin) with distinct permissions.
- **Knowledge Base:** Create and manage support articles to help resolve common issues faster.
- **Team Discussions:** A dedicated space for technicians to collaborate on tickets or general topics.
- **Archive System:** Soft-delete and restore key entities like tickets, users, and articles.
- **Coordinator Dashboard:** Includes summary charts for a quick overview of ticket status and priority distribution.

---

## Tech Stack

| Component            | Technology                              |
|----------------------|-----------------------------------------|
| **Backend**          | ASP.NET Core 8 MVC                      |
| **Database**         | Azure SQL Database (SQL Server)         |
| **ORM**              | Entity Framework Core 8                 |
| **Authentication**   | ASP.NET Core Identity (cookie-based)    |
| **Frontend**         | Razor Views, Bootstrap 5                |
| **Containerization** | Docker, Docker Compose                  |
| **Cloud**            | Azure App Service                       |
| **Monitoring**       | Azure Application Insights              |
| **CI/CD**            | GitHub Actions, Azure DevOps Pipelines  |

---

## Live Demo

The application is deployed to Azure App Service. Demo accounts are pre-seeded:

| Role        | Email                       | Password         |
|-------------|-----------------------------|------------------|
| Admin       | `admin@itsm.local`          | `Admin123!`      |
| Coordinator | `coordinator@itsm.local`    | `Coordinator123!`|
| Technician  | `technician@itsm.local`     | `Technician123!` |
| User        | `user@itsm.local`           | `User123!`       |

---

## Getting Started (Docker)

All you need is Docker and Docker Compose installed.

### 1. Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (includes Docker Compose)

### 2. Configuration

1.  **Create a `.env` file:**
    Copy the provided example file `.env.example` to a new file named `.env`.

    ```bash
    cp .env.example .env
    ```

2.  **Set your password:**
    Open the new `.env` file and set credentials for the PostgreSQL container.

    ```ini
    POSTGRES_DB=itsm
    POSTGRES_USER=itsm
    POSTGRES_PASSWORD=your_new_strong_password
    ```

### 3. Run the Application

```bash
docker compose up --build
```

**What happens next?**
- Docker Compose builds the .NET application image and starts a PostgreSQL container.
- On first startup, EF migrations are applied automatically (`AUTO_MIGRATE=true`).
- Demo roles and users are seeded automatically (`SEED_DEMO=true`).

The application will be available at **`http://localhost:8080`**.

---

## Azure Deployment

The app is deployed to Azure App Service. On each push to `master`, the CI/CD pipeline builds and deploys automatically via:

- **GitHub Actions** — `.github/workflows/azure-deploy.yml`
- **Azure DevOps Pipelines** — `azure-pipelines.yml`

Required Azure App Settings:

| Name                                   | Description                        |
|----------------------------------------|------------------------------------|
| `CONNECTION_STRING`                    | Azure SQL Database connection string |
| `AUTO_MIGRATE`                         | `true` — apply migrations on start |
| `SEED_DEMO`                            | `true` — seed demo accounts        |
| `ENABLE_HTTPS_REDIRECT`                | `true`                             |
| `DATA_PROTECTION_KEYS_PATH`            | `/home/data/keys`                  |
| `APPLICATIONINSIGHTS_CONNECTION_STRING`| Application Insights connection string |

---

## Project Structure Highlights

- **/Controllers:** Thin controllers responsible for routing requests and returning views.
- **/Services:** Contains all business logic, neatly separated by domain (e.g., `TicketService`, `AutoServiceService`).
- **/Data:** Houses the `DBaseContext`, migrations, and database seeders.
- **/Models:** Defines the core domain entities (e.g., `Ticket`, `User`, `Discussion`).
- **/ViewModels:** Strongly-typed models used to pass data to and from the Razor Views, ensuring a clean separation from domain models.
- **/Enums:** Centralized definitions for `Status`, `TicketPriority`, `UserRoles`, etc.
- **`docker-compose.yml`:** Defines the services, networks, and volumes for the local stack.
- **`Program.cs`:** Configures dependency injection, authentication, and the database connection, reading all secrets from environment variables.
