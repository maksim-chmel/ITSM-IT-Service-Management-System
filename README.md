# ITSM — IT Service Management System

A full-featured IT service management web application built with ASP.NET Core 8 MVC. The system supports the full ticket lifecycle, from submission by a user to resolution by a technician, featuring an automated assignment engine, a knowledge base, team discussions, and a soft-delete archive.

The project is pre-configured to run with Docker and Docker Compose for a one-command setup.

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

| Component         | Technology                               |
|-------------------|------------------------------------------|
| **Backend**       | ASP.NET Core 8 MVC                       |
| **Database**      | PostgreSQL 16                            |
| **ORM**           | Entity Framework Core 8                  |
| **Authentication**| ASP.NET Core Identity (cookie-based)     |
| **Frontend**      | Razor Views, Bootstrap 5                 |
| **Containerization**| Docker, Docker Compose                   |

---

## Getting Started

This project is configured to run entirely within Docker. All you need is Docker and Docker Compose installed.

### 1. Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (includes Docker Compose)

### 2. Configuration

1.  **Create a `.env` file:**
    Copy the provided example file `.env.example` to a new file named `.env`.

    ```bash
    cp .env.example .env
    ```

2.  **Set your password:**
    Open the new `.env` file and replace `your_super_secret_password_here` with a strong, unique password for the PostgreSQL database.

    ```ini
    # .env
    POSTGRES_USER=itsm
    POSTGRES_PASSWORD=your_new_strong_password
    ```

### 3. Run the Application

Execute the following command from the root directory of the project (where `docker-compose.yml` is located):

```bash
docker compose up --build
```

If your setup still uses the legacy plugin name, use `docker-compose up --build`.

**What happens next?**
- Docker Compose will build the .NET application image.
- It will start a PostgreSQL container and a container for the web application.
- On first startup, the web application will automatically apply all **existing** Entity Framework migrations to create the database schema.
- For demo/recruiter convenience in Docker, the app will also seed demo roles/users (can be disabled via `SEED_DEMO=false`).

The application will be available at **`http://localhost:8080`**.

### 4. Demo Accounts (Docker)

When started via Docker Compose, the following accounts are created automatically:

- `admin@itsm.local` / `Admin123!` (Admin)
- `coordinator@itsm.local` / `Coordinator123!` (Coordinator)
- `technician@itsm.local` / `Technician123!` (Technician)
- `user@itsm.local` / `User123!` (User)

You can also register a new user at `http://localhost:8080/Auth/Register`.

---

## Project Structure Highlights

- **/Controllers:** Thin controllers responsible for routing requests and returning views.
- **/Services:** Contains all business logic, neatly separated by domain (e.g., `TicketService`, `AutoServiceService`).
- **/Data:** Houses the `DBaseContext`, migrations, and database seeders.
- **/Models:** Defines the core domain entities (e.g., `Ticket`, `User`, `Discussion`).
- **/ViewModels:** Strongly-typed models used to pass data to and from the Razor Views, ensuring a clean separation from domain models.
- **/Enums:** Centralized definitions for `Status`, `TicketPriority`, `UserRoles`, etc.
- **`docker-compose.yml`:** Defines the services, networks, and volumes for the entire application stack.
- **`Program.cs`:** Configures dependency injection, authentication, and the database connection, reading secrets from environment variables.
