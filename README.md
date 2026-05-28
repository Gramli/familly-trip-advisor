# Family Trip Advisor

[![Backend Build](https://github.com/Gramli/familly-trip-advisor/actions/workflows/backend-build.yml/badge.svg)](https://github.com/Gramli/familly-trip-advisor/actions/workflows/backend-build.yml)
[![Frontend Build](https://github.com/Gramli/familly-trip-advisor/actions/workflows/frontend-build.yml/badge.svg)](https://github.com/Gramli/familly-trip-advisor/actions/workflows/frontend-build.yml)
![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![Angular](https://img.shields.io/badge/Angular-21-DD0031?logo=angular)
![Ollama](https://img.shields.io/badge/Ollama-local_AI-black?logo=ollama)

> Describe your trip in plain language — get a full day plan, tailored to the weather and your family's preferences.

Family Trip Advisor is a full-stack AI application that runs entirely on your local machine. You type a natural language request, and the backend orchestrates multiple steps — intent extraction, weather lookup, place discovery, and plan generation — to produce a structured, ready-to-follow itinerary.

**Highlights:**

- No cloud AI — the language model runs locally via [Ollama](https://ollama.com)
- Weather-aware recommendations: indoor vs outdoor activities are resolved by rule-based logic
- Activities, restaurants, and parking are discovered in parallel from the Geoapify API
- AI outputs are validated and retried automatically when they are incomplete
- Clean Vertical Slice Architecture across both frontend and backend

---

## Features

| Feature | Description |
|---|---|
| Natural language trip planning | Describe your trip in plain text; the model extracts destination, date, and preferences |
| Weather-aware suggestions | Forecasts are fetched automatically; indoor/outdoor activities are selected based on temperature, cloud cover, and wind |
| Activity discovery | Nearby activities are fetched from Geoapify and filtered by type |
| Restaurant recommendations | Relevant restaurants near the destination are included in the plan |
| Parking suggestions | Nearby parking options are included in every itinerary |
| AI validation & retry | Backend checks model output for completeness and retries if a plan is missing key sections |
| Local AI via Ollama | Uses Gemma 4 (`gemma4:e2b`) running on your machine — no data leaves your computer |
| Angular chat interface | Conversational SPA with session continuity and a radius selector |
| ASP.NET Core backend | .NET 10 Minimal API with a clean vertical slice structure |
| Input sanitization | Request validator guards against prompt injection and off-topic content |

---

## Architecture Overview

The application follows **Vertical Slice Architecture** on both sides of the stack. Each feature owns its endpoint, handler, services, models, and prompt builders. Infrastructure clients (Ollama, Geoapify, Weatherbit) are shared across slices.

**Backend request flow:**

```
POST /trip/v1/plan
    │
    ▼
Validate & sanitize input
    │
    ▼
Extract trip intent (LLM → TripIntentionDto)
    │
    ├── Fetch weather forecast
    │       └── Resolve activity type (Indoor / Outdoor / Both) from weather rules
    │
    ├── Fetch places in parallel (activities + restaurants + parking)
    │
    └── Generate trip plan (LLM → TripPlanDto)
                └── Validate output → retry if incomplete
```

For full structural details, see [ARCHITECTURE.md](ARCHITECTURE.md).

---

## AI Strategy

The language model is used for two distinct tasks:

1. **Intent extraction** — parse the user's prompt into a structured object (destination, date, activity preference)
2. **Plan generation** — select the best places from the discovered list and write a human-readable summary

Because smaller local models can produce inconsistent output, the backend does not trust the model blindly:

- Output is deserialized and validated after every model call
- If a required field is missing (e.g. no parking suggestion, no summary), the request is retried automatically
- Prompt construction is handled by dedicated builder classes, keeping prompts focused and short — which improves reliability with smaller models

This approach keeps the AI integration practical without requiring a large cloud-hosted model.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Angular 21 (standalone components, signals) |
| Backend | ASP.NET Core — .NET 10 Minimal API |
| AI runtime | [Ollama](https://ollama.com) (local) |
| AI model | Gemma 4 (`gemma4:e2b`) |
| Places API | [Geoapify](https://www.geoapify.com) |
| Weather API | [Weatherbit](https://rapidapi.com) via RapidAPI |
| Architecture | Vertical Slice Architecture |
| Validation | FluentValidation |
| Resilience | Retry pipeline with output validation |

---

## Quick Start

**Prerequisites:** Ollama, .NET 10 SDK, Node.js, and two free API keys (Geoapify + Weatherbit via RapidAPI).

For full installation instructions, see [HOW-TO-RUN.md](HOW-TO-RUN.md).

Once prerequisites are in place:

```powershell
# Clone the repository
git clone https://github.com/Gramli/familly-trip-advisor.git
cd familly-trip-advisor

# Start both backend and frontend
.\start-app.ps1
```

Then open your browser at `http://localhost:4200`.

**Script options:**

| Argument | Effect |
|---|---|
| `-Backend` | Start the backend only |
| `-Frontend` | Start the frontend only |
| `-Expose` | Bind to `0.0.0.0` for local network access |

---

## Project Structure

```
familly-trip-advisor/
├── backend/
│   ├── Features/TripPlanner/   Core slice: endpoint, handler, AI planning, weather, places
│   ├── Infrastructure/         Shared HTTP clients (Ollama, Geoapify, Weatherbit)
│   ├── Shared/                 Utilities: geo calculator, JSON extractor, result extensions
│   └── Tests/                  Unit tests
│
├── frontend/
│   └── src/app/
│       ├── features/trip-planner/   Chat window, trip plan display, place lists
│       └── shared/                  Reusable UI components
│
├── ARCHITECTURE.md             Detailed structural documentation
├── HOW-TO-RUN.md               Step-by-step setup guide
└── start-app.ps1               One-command launcher (PowerShell)
```

---

## Example Workflow

**User input:**
> "We want to visit Vienna next Saturday with two kids."

**What happens:**

1. The model extracts: destination = Vienna, date = next Saturday, activity preference = family-friendly
2. The weather forecast for that date is fetched from Weatherbit
3. Weather rules determine activity type (e.g. partly cloudy → outdoor activities preferred)
4. Geoapify is queried in parallel for activities, restaurants, and parking near Vienna
5. The model receives the filtered place list and generates a structured day plan
6. The plan is validated — if anything is missing, the step retries automatically
7. A complete itinerary is returned to the Angular frontend and displayed in the chat

---

## Documentation

| Document | Description |
|---|---|
| [HOW-TO-RUN.md](HOW-TO-RUN.md) | Full setup guide: prerequisites, API keys, running the app, troubleshooting |
| [ARCHITECTURE.md](ARCHITECTURE.md) | Codebase structure, request flow, component responsibilities |

---

## Limitations

- **Latency** — local model inference is slower than cloud APIs, especially on the first request when the model loads
- **Non-determinism** — the language model may occasionally produce different outputs for the same input; the retry pipeline mitigates this but does not eliminate it
- **External API dependency** — place and weather data require active API keys with valid free-tier quotas
- **Local model trade-offs** — smaller models are more practical to run locally but are less capable than large cloud-hosted models; prompt design matters significantly

---

## Roadmap

Planned improvements tracked in [TODO.md](TODO.md):

- Cache weather and places responses to reduce external API calls
- Fan out weather and places fetches in parallel to reduce overall latency
- Replace in-process session cache with a distributed cache for better scalability
- Implement request deduplication using an idempotency key
- Strengthen prompt injection detection with a text normalisation step

---

## Contributing

Contributions are welcome. To get started:

1. Fork the repository and create a feature branch
2. Make your changes and ensure existing tests pass
3. Open a pull request with a clear description of the change

For larger changes, opening an issue first to discuss the approach is appreciated.

---
