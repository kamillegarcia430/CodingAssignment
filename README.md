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
It supports concurrency-safe claiming of tasks, database seeding, and follows a clean architecture structure.

## Quick Start

### Run in GitHub Codespaces

You can instantly run and develop this project in the cloud using [GitHub Codespaces](https://github.com/features/codespaces).

[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/kamillegarcia430/VolunteerScheduler)

Steps:  
1. Click the badge above or go to https://codespaces.new/kamillegarcia430/VolunteerScheduler to create a new Codespace.  
2. Once the Codespace loads, open a terminal.  
3. Run the following command to start the services:  
   ```bash
   docker-compose up --build
   ```  
4. Open the **Ports** tab in Codespaces, find port `5000`, and click **Open in Browser**.  
5. The browser will open the API root URL. Append `/swagger` to the URL to access API docs, e.g.:  
   ```
   https://<codespace-name>-5000.app.github.dev/swagger
   ```
6. You can also access the API endpoints through Postman by updating the `{{base-url}}` to a link like this:
   ```
   https://<codespace-name>-5000.app.github.dev
   ```

### Local Setup

1. Clone the repository:  
   ```bash
   git clone https://github.com/kamillegarcia430/VolunteerScheduler.git
   cd VolunteerScheduler
   ```

2. Build and run using Docker Compose:  
   ```bash
   docker-compose up --build
   ```

3. Or run locally without Docker:  
   ```bash
   dotnet restore
   dotnet build
   dotnet run --project VolunteerScheduler.API
   ```

The API will be available at: `http://localhost:5188`  
Swagger documentation: `http://localhost:5188/swagger`

## API Reference
Main endpoints include:  
- `GET /api/volunteertasks` — List all tasks  
- `GET /api/volunteertasks/{id}` — Get task details  
- `POST /api/volunteertasks` — Create a new task  
- `PUT /api/volunteertasks/{id}` — Update a task  
- `DELETE /api/volunteertasks/{id}` — Delete a task  
- `POST /api/volunteertasks/{id}/claim?parentId={parentId}` — Claim a task  

For full API docs, visit the Swagger UI.

## Sample Requests

### Curl Example — Claim Task
```bash
curl -X POST "http://localhost:5188/api/volunteertasks/1/claim?parentId=2" -H "Content-Type: application/json"
```

### Postman Collection
Import the Postman collection from `/VolunteerScheduler.postman_collection.json`.

## Design Decisions
1. **Clean Architecture** — Separation of concerns between API, Application, Domain, and Infrastructure layers.  
2. **Concurrency Safety** — Locking in claim task logic to prevent race conditions.  
3. **Entity Framework Core** — Used for data access, migrations, and seed data for easy setup.  
4. **Swagger UI** — Auto-generated API documentation.  
5. **Docker Support** — Simplifies local setup without manual DB installation.

## License
This project is licensed under the MIT License.
