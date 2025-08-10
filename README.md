# Create a README.md file with the generated markdown content so the user can download it

readme_content = """# Volunteer Scheduler

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
dotnet run --project VolunteerScheduler.API
