# Server Side – ASP.NET Core Fysio Clinic System

This project is a **server-side web application** built in **ASP.NET Core** using **Entity Framework Core** and the **Onion Architecture** pattern.
The system manages various aspects of a physiotherapy clinic — such as patient registration, appointment scheduling, user roles, and more — implementing complex business logic and requirements.

> **Note:** This web application communicates with a separate AI service ("WebAI") to provide enhanced functionality. The AI service itself is not included in this project.

> This project was developed as part of a school assignment for a **Server Side** course.

## 🧰 Technologies & Architecture

- ASP.NET Core (Web API)
- Entity Framework Core (Code First + Migrations)
- Onion Architecture
- Repository and Unit of Work patterns
- Model validation and `ModelState`
- Identity system with roles and permissions
- Unit testing
- Continuous deployment pipeline
- ORM and advanced LINQ queries

## ✅ Features

- 🔐 **Accounts with roles & permissions** (Admin, Therapist, Patient, etc.)
- 📅 **Appointment management** (multiple appointment types, create/update logic)
- 👥 **Patient registry** with business rule validation
- 📄 **Vektis list** integration
- 🧾 Complex business logic based on user stories
- 🤝 Communication with external AI service (WebAI) for enhanced features
- 🧪 Unit tests for critical components
- 📦 Continuous deployment enabled

## 🏗️ Domain-Driven Design

The project follows Onion Architecture:
- **Core**: domain models and interfaces
- **Application**: service layer, DTOs, business logic
- **Infrastructure**: database context, EF migrations
- **Web/API**: controllers, authentication, external service integration
