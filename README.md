# ITSM — IT Service Management System

A full-featured IT service management web application built with ASP.NET Core 8 MVC. The system supports the full ticket lifecycle — from submission by a user through triage by a coordinator to resolution by a technician — with automatic ticket assignment, a knowledge base, team discussions, and a soft-delete archive.

---

## Features

### Ticket Management
- Users submit tickets with a title, description, category and subcategory
- Coordinators review all tickets, set priority, assign technicians manually or trigger auto-assignment
- Technicians work their queue, add progress comments, and resolve or cancel tickets
- Full ticket history log tracks every status change and admin comment
- Tickets can be reopened after resolution

### Auto-Assignment Algorithm
The coordinator can trigger automatic ticket distribution with a single button. The algorithm:
1. Selects all unassigned tickets ordered by priority (Critical → High → Medium → Low)
2. Finds technicians whose assigned categories match the ticket's category
3. Scores each candidate: `(ticket priority × technician skill level) − active ticket count`
4. Assigns the ticket to the highest-scoring technician and sets status to `In Progress`

This balances workload and ensures that critical tickets go to the most qualified available technician.

### Role System
| Role | Capabilities |
|---|---|
| **User** | Submit tickets, view own ticket history |
| **Technician** | View assigned tickets, add progress notes, resolve/cancel |
| **Coordinator** | Review all tickets, assign priority and technician, trigger auto-assignment, manage categories |
| **Admin** | Full user management, role assignment, access to archive |

### Knowledge Base
- Articles organized by category
- Technicians and coordinators can create, edit, and soft-delete articles
- Archive with restore capability

### Discussions
- Threaded discussions separate from tickets
- Archive of closed discussions

### Archive & Soft Delete
- Tickets, users, categories, and articles are soft-deleted (not permanently removed)
- Dedicated archive section with selective restore

### Other
- Ticket filtering by category, priority, and status
- Status and priority distribution charts on the coordinator dashboard
- Technician skill levels (Junior / Middle / Senior / Expert) factored into auto-assignment
- Category-to-technician qualification assignments
- Global exception middleware with user-friendly error redirect

---

## Tech Stack

| | |
|---|---|
| Framework | ASP.NET Core 8 MVC |
| ORM | Entity Framework Core 8 + SQL Server |
| Auth | ASP.NET Core Identity (cookie-based) |
| Frontend | Razor Views + Bootstrap |

---

## Project Structure

```
ITSM/
├── Controllers/          # 12 controllers, each scoped to one domain area
├── Services/             # Business logic with interface/implementation pairs
│   ├── Ticket/           # Ticket CRUD, history, status transitions
│   ├── TicketAssignment/ # Manual assignment, priority update
│   ├── Authomatization/  # Auto-assignment algorithm
│   ├── Qualification/    # Technician category assignments
│   ├── Archive/          # Soft-delete restore logic
│   ├── Charts/           # Status/priority chart data
│   ├── Discussion/       # Threaded discussions
│   ├── KnowledgeBase/    # Article management
│   └── ...
├── Models/               # Domain models (Ticket, User, TicketHistory, ...)
├── ViewModels/           # Strongly typed view models (Create/, Manage/)
├── Enums/                # Status, TicketPriority, UserRoles, SkillLevel
├── Middleware/           # Global exception handler
├── DB/                   # DbContext, role seeder
└── Program.cs
```

---

## Ticket Lifecycle

```
New -> Open -> In Progress -> Resolved -> Done
                 |                         |
              Canceled                 Reopened -> In Progress
```

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (or SQL Server Express / LocalDB)

### Configuration

Update the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=ITSM;Trusted_Connection=True;"
}
```

### Run

```bash
cd ITSM
dotnet run
```

The database will be created and roles will be seeded automatically on first startup. Navigate to `/Auth/Register` to create your first account, then assign roles via the Admin panel.

---

## Architecture Notes

- **Service layer** — all business logic lives in dedicated services behind interfaces, keeping controllers thin
- **ViewModels** — domain models are never passed directly to views; separate ViewModels handle all create/edit/display scenarios
- **Soft delete** — entities implement `ISoftDeletableEntity` allowing generic restore logic via `RestoreEntitiesAsync<T>`
- **BaseController** — shared `SetTempDataMessage` helper keeps success/error notification logic in one place
- **Role seeding** — `SeedRoles.Initialize` runs on startup, ensuring all four roles exist before any user registers
