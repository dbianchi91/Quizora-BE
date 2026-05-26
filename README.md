# Quizora — Backend API

Quizora is an intelligent quiz and exam platform. This repository contains the ASP.NET Core backend, structured as a **modular monolith** following Clean Architecture principles.

Users can take timed exams or study-mode sessions, receive AI-powered tutoring, and track their progress through analytics. Creators build and publish quiz content. Admins manage the platform.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 9 / ASP.NET Core (Minimal API) |
| ORM | EF Core (per-module DbContext) |
| Complex reads | Dapper |
| Messaging | MediatR (CQRS + pipeline behaviors) |
| Validation | FluentValidation |
| Caching | Redis (StackExchange.Redis) |
| Database | SQL Server |
| AI | Anthropic SDK (Claude `claude-sonnet-4-6`) |
| Auth | JWT RS256 + refresh token rotation |
| Testing | xUnit, NSubstitute, Testcontainers |

---

## Architecture

The application is a **modular monolith** — a single deployable unit partitioned into 5 independent feature modules. Each module owns its domain, application logic, infrastructure, and API endpoints.

```
src/Modules/{Module}/
├── {Module}.Domain/           # Entities, Value Objects, Domain Events
├── {Module}.Application/      # Commands, Queries, Handlers, Validators, DTOs
└── {Module}.Infrastructure/   # EF Core DbContext, Repositories, Endpoints
```

`Quizora.Api` is a thin host that wires up middleware and registers each module via `AddXxxModule()`. All business logic lives inside modules.

`Quizora.SharedKernel` provides shared primitives: `Result<T>`, `Error`, `BaseEntity<TId>`, `ValueObject`, `IModuleEndpoints`, `HttpErrorMapper`.

### Key Patterns

- **CQRS via MediatR** — Commands and Queries are records implementing `IRequest<Result<T>>`. Pipeline behaviors handle validation and logging.
- **Result pattern** — Business logic never throws. All operations return `Result<T>` or `Result`. Errors are mapped to HTTP responses via `HttpErrorMapper`.
- **Factory methods on entities** — `User.Create(...)`, `Quiz.Create(...)`, etc. validate inputs and raise domain events. The application layer never calls constructors directly.
- **Value Objects** — Primitive obsession is avoided; typed values like `Email`, `UserId`, `Score` enforce their own invariants.
- **Outbox pattern** — Domain events are persisted in the same transaction as business data and published asynchronously, ensuring reliable inter-module communication.

---

## Modules

### Identity

Handles user accounts, authentication, and authorization.

- Registration with email uniqueness validation and password hashing
- JWT login with RS256 signing and refresh token rotation (IP-tracked)
- Three roles: **User**, **Creator**, **Admin**
- Admin endpoints to list users and grant/revoke the Creator role

| Method | Path | Access |
|---|---|---|
| POST | `/api/v1/auth/register` | Public |
| POST | `/api/v1/auth/login` | Public |
| POST | `/api/v1/auth/refresh` | Public |
| POST | `/api/v1/auth/logout` | Authenticated |
| GET | `/api/v1/users/me` | Authenticated |
| GET | `/api/v1/admin/users` | Admin |
| POST | `/api/v1/admin/users/{id}/grant-creator` | Admin |
| POST | `/api/v1/admin/users/{id}/revoke-creator` | Admin |

---

### QuizManagement

Allows Creators to build and publish quiz content.

- Hierarchical category system (parent → child)
- Questions with multiple-choice options, difficulty levels (Easy / Medium / Hard), and a correct-answer flag
- Quizzes start as drafts; publishing requires at least one question
- Configurable exam settings per quiz: time limit, points for correct/wrong/skipped answers

| Method | Path | Access |
|---|---|---|
| GET | `/api/v1/categories` | Public |
| GET | `/api/v1/quizzes` | Public |
| GET | `/api/v1/quizzes/{id}` | Public |
| POST | `/api/v1/quizzes` | Creator |
| PUT | `/api/v1/quizzes/{id}` | Creator |
| DELETE | `/api/v1/quizzes/{id}` | Creator |
| POST | `/api/v1/quizzes/{id}/publish` | Creator |
| POST | `/api/v1/quizzes/{id}/questions` | Creator |
| GET | `/api/v1/questions` | Creator |
| POST | `/api/v1/questions` | Creator |

---

### ExamEngine

Manages the lifecycle of exam and study sessions.

- Two session types: **Exam** (timed, enforced deadline) and **Study** (unlimited time, review mode)
- Session lifecycle: `Start → Answer → Submit` (or timeout / abandon)
- All questions are delivered to the client in a single `GET /state` call; navigation between questions is client-side
- Per-question scoring with configurable point values; normalized final score (0–100)
- Automatic timeout via a background job
- Publishes `ExamCompletedEvent` and `ExamTimedOutEvent` consumed by Analytics and AITutor

| Method | Path | Description |
|---|---|---|
| POST | `/api/v1/exams/start` | Start a new session |
| GET | `/api/v1/exams/{id}/state` | Get full session state + all questions |
| POST | `/api/v1/exams/{id}/answer` | Record an answer |
| POST | `/api/v1/exams/{id}/submit` | Submit and finalize scoring |
| POST | `/api/v1/exams/{id}/abandon` | Abandon an incomplete session |
| GET | `/api/v1/exams/{id}/results` | Fetch final score and statistics |
| GET | `/api/v1/exams/history` | List the user's past attempts |
| GET | `/api/v1/exams/simulations/{quizId}` | List study sessions for a quiz |

---

### AITutor

Provides AI-powered tutoring and personalized study plans via the Claude API.

- Streaming chat (Server-Sent Events) with conversation history persistence
- System prompt is dynamically built from the user's exam history and weak areas
- Study plans are automatically generated after each completed exam (`ExamCompletedEvent` listener)
- Study plans can also be regenerated manually

| Method | Path | Description |
|---|---|---|
| POST | `/api/v1/ai/chat/stream` | Stream a chat response (SSE) |
| GET | `/api/v1/ai/study-plan` | Fetch current study plan |
| POST | `/api/v1/ai/study-plan/generate` | Regenerate study plan manually |

---

### Analytics

Tracks performance metrics and exposes leaderboards.

- Personal statistics: total attempts, best score, average score, completion rate
- Category-level performance breakdown
- Paginated exam history
- Global leaderboard (public, optionally filtered by category)
- Admin overview dashboard (total users, exams taken, platform averages)
- Uses **Dapper** for complex aggregation queries; listens to `ExamCompletedEvent` to record results asynchronously

| Method | Path | Access |
|---|---|---|
| GET | `/api/v1/analytics/me` | Authenticated |
| GET | `/api/v1/analytics/me/categories` | Authenticated |
| GET | `/api/v1/analytics/me/history` | Authenticated |
| GET | `/api/v1/analytics/leaderboard` | Public |
| GET | `/api/v1/analytics/admin/overview` | Admin |

---

## Getting Started

### Prerequisites

- .NET 9 SDK
- SQL Server (or Docker)
- Redis (or Docker)
- Docker (required for integration tests via Testcontainers)

### Configuration

Edit `Quizora.Api/appsettings.json` (or use user secrets):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=Quizora;...",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "PrivateKeyPem": "...",
    "PublicKeyPem": "..."
  },
  "AI": {
    "Model": "claude-sonnet-4-6"
  }
}
```

### Running

```powershell
# Build
dotnet build Quizora-be.sln

# Run API
dotnet run --project Quizora.Api/Quizora-be
```

### Testing

```powershell
# Unit tests (xUnit + NSubstitute)
dotnet test tests/Quizora.UnitTests/

# Integration tests (requires Docker — spins up MSSQL and Redis via Testcontainers)
dotnet test tests/Quizora.IntegrationTests/

# Architecture rule enforcement
dotnet test tests/Quizora.ArchTests/

# Single test
dotnet test tests/Quizora.UnitTests/ --filter "FullyQualifiedName~MyTestClass"
```

---

## Project Structure

```
Quizora-be.sln
├── Quizora.Api/                        # Thin host — middleware, module registration
├── Quizora.SharedKernel/               # Result<T>, Error, BaseEntity, ValueObject, ...
├── src/
│   └── Modules/
│       ├── Identity/
│       ├── QuizManagement/
│       ├── ExamEngine/
│       ├── AITutor/
│       └── Analytics/
└── tests/
    ├── Quizora.UnitTests/
    ├── Quizora.IntegrationTests/
    └── Quizora.ArchTests/
```
