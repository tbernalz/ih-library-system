# IH Library System

📚 **A modern, AI-powered library management system built with .NET 10.0 and Clean Architecture**

[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-blue.svg)](https://docs.microsoft.com/en-us/aspnet/core/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-blue.svg)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED.svg)](https://www.docker.com/)

> A comprehensive library management system demonstrating modern software development practices, Clean Architecture, and AI integration.

## 🧠 Overview

The IH Library System is an API designed to manage library operations efficiently. Built with Clean Architecture principles and modern .NET development patterns.

## 📑 Table of Contents

- [🚀 Features](#-features)
- [📚 Tech Stack](#-tech-stack)
- [📦 Setup and Installation](#-setup-and-installation)
- [📁 Project Structure](#-project-structure)
- [🔧 API Documentation](#-api-documentation)
- [📄 License](#-license)
- [✉️ Contact](#️-contact)

## 🚀 Features

### 📖 Library Management

- **Book Management**: Complete CRUD operations with ISBN validation
- **Author Management**: Comprehensive author profiles
- **Loan System**: Track book loans with fine calculation
- **Search & Filter**: Advanced search capabilities across books and authors
- **Status Tracking**: Real-time book availability status

### 🔧 Technical Features

- **Clean Architecture**: Separation of concerns with dependency injection
- **Database Migrations**: Automatic schema management with Entity Framework
- **Seed Data**: Auto-populated database with sample data in development
- **Error Handling**: Global exception handling
- **API Documentation**: Interactive Swagger/OpenAPI documentation
- **Docker Support**: Containerized deployment

## 📚 Tech Stack

### Backend

- **.NET 10.0** - Latest .NET framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM for database operations
- **Microsoft.Extensions.AI** - AI integration framework

### Database

- **PostgreSQL 17** - Robust relational database
- **Docker Compose** - Database containerization

## 📦 Setup and Installation

### Prerequisites

- **.NET 10.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Docker & Docker Compose** - [Install Docker](https://docs.docker.com/get-docker/)

### 🐳 Quick Start with Docker

1. **Clone the repository**

   ```bash
   git clone https://github.com/tbernalz/ih-library-system.git
   cd ih-library-system
   ```

2. **Configure environment variables**

   ```bash
   cp .env.example .env
   # Edit .env with your PostgreSQL password
   ```

3. **Start the database**

   ```bash
   docker compose up -d
   ```

4. **Run the application**

   ```bash
   dotnet run --project IH.LibrarySystem.Api
   ```

5. **Access the application**
   - API: `http://localhost:5192`
   - Swagger UI: `http://localhost:5192/swagger`

## 📁 Project Structure

```
IHLibrarySystem/
├── IH.LibrarySystem.Api/           # Web API layer
├── IH.LibrarySystem.Application/   # Business logic layer
├── IH.LibrarySystem.Domain/        # Core domain layer
├── IH.LibrarySystem.Infrastructure/ # Data access layer
├── docker-compose.yml              # Database container setup
├── Dockerfile                      # Application containerization
└── README.md                       # This file
```

## 🔧 API Documentation

Once the application is running, visit `http://localhost:5192/swagger` for interactive API documentation.

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

## ✉️ Contact

**Tomas Bernal**  
Software Developer

📧 **Email**: [tbernalz@eafit.edu.co]  
🐙 **GitHub**: [tbernalz](https://github.com/tbernalz)  
💼 **LinkedIn**: [tbernalz](https://www.linkedin.com/in/tbernalz/)
