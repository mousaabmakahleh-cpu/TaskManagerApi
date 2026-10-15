# Task Manager API

A RESTful Web API for managing projects and tasks, built with **ASP.NET Core 9**, **Entity Framework Core**, and **PostgreSQL**.

Users can register, log in with **JWT authentication**, and manage their own projects and tasks. Each user can only access **their own data**.

---

## Features

- User registration and login with **JWT** authentication
- Passwords securely hashed with **BCrypt**
- Full CRUD for **Projects** and **Tasks**
- Data ownership: each user can only access their own projects and tasks
- **Filtering** tasks by status, priority, and title search
- **Pagination** for all list endpoints
- **Validation** using Data Annotations with clear error messages
- **Global exception handling** with standard `ProblemDetails` responses
- **API versioning** (`/api/v1/...`)
- Interactive API documentation with **Swagger UI**
- Secrets kept out of source code using **User Secrets**

---

## Tech Stack

| Category | Technology |
|---|---|
| Framework | ASP.NET Core 9 Web API |
| Language | C# |
| Database | PostgreSQL |
| ORM | Entity Framework Core 9 |
| Authentication | JWT Bearer |
| Password Hashing | BCrypt.Net-Next |
| Documentation | OpenAPI + Swagger UI |

---

## Database Design

```
User (1) ────< (many) Project (1) ────< (many) Task
```

- A user has many projects, and a project has many tasks.
- Deleting a project automatically deletes its tasks (cascade delete).

---

## API Endpoints

### Auth

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/v1/auth/register` | Register a new user |
| POST | `/api/v1/auth/login` | Log in and receive a JWT token |

### Projects (requires token)

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/v1/projects` | Get my projects (supports `search`, `page`, `pageSize`) |
| GET | `/api/v1/projects/{id}` | Get a project by id |
| POST | `/api/v1/projects` | Create a new project |
| PUT | `/api/v1/projects/{id}` | Update a project |
| DELETE | `/api/v1/projects/{id}` | Delete a project and all its tasks |

### Tasks (requires token)

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/v1/projects/{projectId}/tasks` | Get tasks of a project (supports `status`, `priority`, `search`, `page`, `pageSize`) |
| POST | `/api/v1/projects/{projectId}/tasks` | Create a task in a project |
| GET | `/api/v1/tasks/{id}` | Get a task by id |
| PATCH | `/api/v1/tasks/{id}` | Partially update a task |
| DELETE | `/api/v1/tasks/{id}` | Delete a task |

**Task status values:** `Todo`, `InProgress`, `Done`
**Task priority values:** `Low`, `Medium`, `High`

---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/)

### 1. Clone the repository

```bash
git clone https://github.com/mousaabmakahleh-cpu/TaskManagerApi.git
cd TaskManagerApi
```

> If the project files are inside a subfolder, `cd` into the folder that contains `TaskManagerApi.csproj`.

### 2. Configure secrets

The connection string and JWT key are **not** stored in the repository. Set them using User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=TaskManagerDb;Username=postgres;Password=YOUR_PASSWORD"
dotnet user-secrets set "Jwt:Key" "YOUR_SECRET_KEY_AT_LEAST_32_CHARACTERS_LONG"
```

### 3. Create the database

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update
```

### 4. Run the API

```bash
dotnet run
```

Then open Swagger UI in your browser:

```
https://localhost:<port>/swagger
```

---

## How to Use

1. **Register** using `POST /api/v1/auth/register`:

```json
{
  "username": "mousaab",
  "password": "123456"
}
```

2. Copy the `token` from the response.
3. Click **Authorize** in Swagger UI and paste the token.
4. Create a project, then add tasks to it.

---

## Project Structure

```
TaskManagerApi/
├── Controllers/     # API endpoints (Auth, Projects, Tasks)
├── Data/            # AppDbContext and database configuration
├── Dtos/            # Request and response models
├── Handlers/        # Global exception handler
├── Migrations/      # EF Core database migrations
├── Models/          # Database entities (User, Project, TaskItem)
└── Program.cs       # App configuration (DB, JWT, Swagger)
```

---

## Author

**Mousaab Makahleh**
GitHub: [@mousaabmakahleh-cpu](https://github.com/mousaabmakahleh-cpu)