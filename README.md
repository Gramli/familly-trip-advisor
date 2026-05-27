# 🗺️ Family Trip Advisor

[![Backend Build](https://github.com/Gramli/familly-trip-advisor/actions/workflows/backend-build.yml/badge.svg)](https://github.com/Gramli/familly-trip-advisor/actions/workflows/backend-build.yml)
[![Frontend Build](https://github.com/Gramli/familly-trip-advisor/actions/workflows/frontend-build.yml/badge.svg)](https://github.com/Gramli/familly-trip-advisor/actions/workflows/frontend-build.yml)
![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![Angular](https://img.shields.io/badge/Angular-21-DD0031?logo=angular)

> Tell us where you'd like to go — and we'll plan the perfect day for your family.

Family Trip Advisor is an AI-powered trip planning assistant that runs entirely on your own computer. Just describe your trip in plain language, and the app will suggest activities, restaurants, and parking — tailored to the weather and your preferences.

---

## How It Works

1. **You describe your trip** — type something like *"We want to visit Vienna next Saturday with two kids"*
2. **The app understands your intent** — it extracts the destination, date, and preferences
3. **Weather is checked** — the forecast for that day is fetched automatically
4. **Places are found** — nearby activities, restaurants, and parking are discovered
5. **A full plan is generated** — the AI puts it all together into a ready-to-follow itinerary

Everything runs locally on your machine. No data is sent to any external AI service.

---

## Screenshots

> _Add screenshots here once the app is running_

---

## Tech at a Glance

| Layer | Technology |
|---|---|
| Frontend | Angular 21 |
| Backend | .NET 10 Minimal API |
| AI Model | Gemma 4 (via Ollama — runs locally) |
| Places data | Geoapify API |
| Weather data | Weatherbit via RapidAPI |

---

## Getting Started

- **[How to Run the App](HOW-TO-RUN.md)** — step-by-step setup guide for all skill levels, including installing all prerequisites and API keys
- **[Architecture Overview](ARCHITECTURE.md)** — how the codebase is structured (for developers)

---

## Project Structure

```
familly-trip-advisor/
├── backend/        .NET 10 API — handles AI, weather, and places logic
├── frontend/       Angular app — the chat interface you interact with
├── HOW-TO-RUN.md   Setup and run instructions
├── ARCHITECTURE.md Technical architecture overview
└── start-app.ps1   One-command app launcher (PowerShell)
```

---
