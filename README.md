# Volunteer Scheduler

## Table of Contents
- [Overview](#overview)
- [Quick Start](#quick-start)
- [API Reference](#api-reference)
- [Sample Requests](#sample-requests)
- [Design Decisions](#design-decisions)
- [License](#license)

## Overview
Volunteer Scheduler is a backend REST API built with **.NET 6** for managing volunteer tasks in a school or community setting. 
It supports concurrency-safe claiming of tasks, database seeding, and a clean architecture structure.

## Quick Start
```bash
# Clone the repository
git clone https://github.com/kamillegarcia430/CodingAssignment
cd volunteerscheduler

# Build and run using Docker Compose
docker-compose up --build


# Or run locally
dotnet restore
dotnet build
dotnet run
```

The API will be available at: `http://localhost:5188`

Swagger documentation: `http://localhost:5188/swagger`

## API Reference
The main endpoints include:
- `GET /api/volunteertasks` — List all tasks
- `GET /api/volunteertasks/{id}` — Get details of a task
- `POST /api/volunteertasks` — Create a new task
- `PUT /api/volunteertasks/{id}` — Update a task
- `DELETE /api/volunteertasks/{id}` — Delete a task
- `POST /api/volunteertasks/{id}/claim?parentId={parentId}` — Claim a task

For full API docs, visit the Swagger UI.

## Sample Requests

### Curl Example — Claim Task
```bash
curl -X POST "http://localhost:5188/api/tasks/1/claim?parentId=2"      -H "Content-Type: application/json"
```

### Postman Collection
Import the Postman collection from `/VolunteerScheduler.postman_collection.json`.

## Design Decisions
1. **Clean Architecture** — Separation of concerns between API, Application, Domain, and Infrastructure layers.
2. **Concurrency Safety** — Implemented locking in the claim task logic to prevent race conditions.
3. **Entity Framework Core** — For data access with migrations and seed data for easy setup.
4. **Swagger UI** — Auto-generated API documentation.
5. **Docker Support** — Easy to run locally without manual DB setup.

## License
This project is licensed under the MIT License.
