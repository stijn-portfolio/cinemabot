# Cinema Ticket Booking Bot

**Challenge 2 - Transactional Chatbot**
**Course:** AI.NET - Thomas More
**Tech Stack:** .NET, OpenAI, Semantic Kernel, Bot Framework, Microsoft Teams

---

## Overview

A transactional chatbot for cinema ticket booking that runs in Microsoft Teams. Users can browse movies, check showtimes, and book tickets through natural language conversation with adaptive card visualizations.

## Features

- Browse movies with TMDb poster images
- Check showtimes by movie and date
- Book tickets with customer details
- Update and cancel bookings
- Adaptive cards for rich visual experience in Teams

## Project Structure

```
CinemaBot/
├── AzureBot-solution/       # Bot Framework EchoBot (Teams integration)
├── Bot-solution/            # Semantic Kernel backend
│   ├── Bot.API/            # ASP.NET Core Web API
│   └── Bot.Core/           # Class Library with CinemaPlugin
└── API-solution/           # Database API
    └── Cinema.API/         # ASP.NET Core Web API with EF Core
```

## Requirements

- .NET 8.0 SDK
- OpenAI API key
- TMDb API key (free)
- Microsoft Teams account
- Ngrok (for local development)

## Database Schema

- **Movies** (Id, Title, Description, Genre, Duration, Rating, PosterUrl)
- **Screenings** (Id, MovieId, DateTime, Room, TotalSeats, AvailableSeats, Price)
- **Bookings** (Id, ScreeningId, CustomerName, Email, Phone, Seats, BookingCode, BookedAt)

## Setup Instructions

### 1. Clone Repository

```bash
git clone <repo-url>
cd CinemaBot
```

### 2. Configure API Keys

Create `appsettings.json` files based on `appsettings.Example.json` in each project:

**Cinema.API/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=cinema.db"
  },
  "TMDb": {
    "ApiKey": "your_tmdb_api_key",
    "ImageBaseUrl": "https://image.tmdb.org/t/p/w500"
  }
}
```

**Bot.API/appsettings.json:**
```json
{
  "OpenAI": {
    "ApiKey": "sk-proj-...",
    "Model": "gpt-4o-mini"
  },
  "CinemaAPI": {
    "BaseUrl": "http://localhost:5001"
  }
}
```

### 3. Run Cinema.API

```bash
cd API-solution/Cinema.API
dotnet restore
dotnet ef database update
dotnet run
```

API will be available at `http://localhost:5001`

### 4. Run Bot.API

```bash
cd Bot-solution/Bot.API
dotnet restore
dotnet run
```

API will be available at `http://localhost:5000`

### 5. Setup Ngrok

```bash
ngrok http 5000
```

Copy the HTTPS URL for Teams bot configuration.

### 6. Run Teams Bot

```bash
cd AzureBot-solution/CinemaBot
dotnet restore
dotnet run
```

Configure bot messaging endpoint in Azure Bot registration with ngrok URL.

## Development Workflow

This project follows a feature branch workflow:

```bash
# Start new phase
git checkout -b feature/phase1-cinema-api

# Make changes and commit
git add .
git commit -m "Add Movie model with EF Core configuration"

# Complete phase
git checkout main
git merge feature/phase1-cinema-api
git tag -a phase1-complete -m "Phase 1: Cinema.API completed"
git push origin main --tags
```

## Implementation Phases

- **Phase 1:** Cinema.API - Database API with TMDb integration ✅
- **Phase 2:** Bot.Core - CinemaPlugin with 7 Semantic Kernel functions ✅
- **Phase 3:** Bot.API - KernelService and AdaptiveCardBuilders
- **Phase 4:** Teams Integration - EchoBot + Ngrok
- **Phase 5:** Polish & Demo - Documentation and presentation

## Technologies

- **Backend:** ASP.NET Core 8.0, Entity Framework Core
- **AI:** OpenAI API (gpt-4o-mini), Microsoft Semantic Kernel
- **Bot:** Bot Framework SDK, Microsoft Teams
- **Database:** SQLite
- **External APIs:** TMDb API (movie data and posters)
- **Hosting:** Local development with Ngrok tunneling

## License

Educational project for Thomas More AI.NET course.
