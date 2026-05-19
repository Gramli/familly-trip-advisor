# Vertical Slice Architecture — Family Trip Advisor

The application is organized around **features (vertical slices)** rather than technical layers. Each slice owns everything it needs — from the HTTP endpoint down to the data model — so changes to a feature stay contained within its folder.

---

## Overview

```
familly-trip-advisor/
├── backend/                        .NET 10 Minimal API
│   ├── Features/                   ← vertical slices live here
│   │   └── TripPlanner/            one slice = one feature
│   ├── Infrastructure/             shared HTTP clients (cross-cutting)
│   ├── Shared/                     shared utilities (cross-cutting)
│   └── Program.cs                  composition root
│
└── frontend/                       Angular 21 SPA
    └── src/app/
        ├── features/               ← vertical slices live here
        │   └── trip-planner/       one slice = one feature
        └── shared/                 reusable UI primitives (cross-cutting)
```

---

## Backend — .NET 10 Minimal API

### Feature: `TripPlanner`

The single slice that handles the full trip-planning flow. Every class related to this feature lives under `backend/Features/TripPlanner/`.

```
Features/TripPlanner/
│
├── GenerateTripEndpoint.cs         ← HTTP surface + DI registrations
│       Maps  POST /trip/v1/plan
│       Registers all feature services into the DI container
│
├── GenerateTripHandler.cs          ← feature orchestrator (the "handler")
│       1. Validates the incoming command
│       2. Calls PlanningService → extracts trip intention from the prompt
│       3. Calls WeatherService  → fetches forecast for the destination date
│       4. Calls PlanningService → determines Indoor / Outdoor activity
│       5. Calls PlacesService   → fetches activity, restaurant & parking places
│       6. Calls PlanningService → generates the final AI trip plan
│
├── GetTripPlanQueryValidator.cs    ← input validation + prompt-injection guard
│       Validates length, allowed characters, injection patterns, off-topic content
│
├── Models/                         ← all DTOs & value objects owned by this slice
│   ├── CreateTripPlanCommand.cs    input command  (prompt, sessionId, radiusInMeters)
│   ├── GenerateTripPlanCommand.cs  internal command passed to PlanningService
│   ├── TripIntentionDto.cs         AI-extracted intent (date, coords, activity pref)
│   ├── ActivityPlacesRequest.cs    request for place lookup
│   ├── TripPlacesDto.cs            container: activities + restaurants + parking
│   ├── TripPlanDto.cs              API output (selected places + AI summary)
│   ├── PlaceDto.cs                 base place (name, address, coords, distanceMeters)
│   ├── ActivityPlaceDto.cs         extends PlaceDto → activityType, category
│   ├── RestaurantDto.cs            extends PlaceDto → categories[]
│   ├── ParkingDto.cs               extends PlaceDto → parkingType
│   ├── PlanningActivityType.cs     enum: Indoor | Outdoor | Both
│   └── TripPlannerOptions.cs       config options (home location, etc.)
│
├── Places/                         ← place-lookup sub-slice
│   ├── PlacesService.cs            fetches & maps places from Geoapify in parallel
│   │       GetTripPlacesAsync → runs 3 parallel HTTP calls (activities, restaurants, parking)
│   │       Maps raw Geoapify data to PlaceDto hierarchy
│   └── PlaceCategoryActivityMap.cs static maps: Geoapify category → Activity type
│
├── Planning/                       ← AI planning sub-slice
│   ├── PlanningService.cs          drives all LLM interactions via OllamaClient
│   │       ExtractIntentionAsync    → parses natural language prompt to TripIntentionDto
│   │       GetActivityByWeatherAsync → asks LLM: Indoor or Outdoor?
│   │       GenerateTripPlanAsync    → asks LLM to select places + write summary
│   └── PromptBuilders/             each builder owns its own prompt template
│       ├── IntentionPromptBuilder.cs   "extract trip intent as JSON"
│       ├── ActivityPromptBuilder.cs    "pick Indoor or Outdoor based on forecast"
│       └── TripPlanPromptBuilder.cs    "select best places and summarise the day"
│
└── Weather/                        ← weather sub-slice
    └── WeatherService.cs           wraps WeatherbitClient; finds the forecast for a specific date
```

### Infrastructure — Cross-cutting HTTP Clients

Shared clients consumed by multiple features (currently only `TripPlanner`, but designed to be reusable).

```
Infrastructure/
├── GeoapifyClient/
│   ├── GeoapifyHttpClient.cs       Places API: activities, restaurants, parking
│   ├── PlacesDataModel.cs          raw API response models
│   ├── GeoapifyOptions.cs          config (base URL, API key)
│   └── ContainerConfigurationExtension.cs   registers client into DI
│
├── OllamaClient/
│   ├── OllamaClient.cs             local LLM: GetResponseAsync / GetPreservedResponseAsync (with session)
│   ├── OllamaOptions.cs            config (base URL, model name)
│   └── ContainerConfigurationExtension.cs
│
└── WeatherbitClient/
    ├── WeatherbitHttpClient.cs     16-day forecast + current weather
    ├── WeatherDataModel.cs         ForecastWeatherDto, CurrentWeatherDto, etc.
    ├── WeatherbitOptions.cs        config (base URL, RapidAPI key/host)
    └── ContainerConfigurationExtension.cs
```

### Shared — Cross-cutting Utilities

```
Shared/
├── DateTimeProvider.cs     abstraction over DateTime.Today (testable)
├── GeoCalculator.cs        haversine distance calculations
├── GpsCoordinatesDto.cs    GPS coordinate value object
└── JsonExtractor.cs        extracts raw JSON block from LLM text responses
```

### Request Flow (backend)

```
HTTP POST /trip/v1/plan
    │
    ▼
GenerateTripEndpoint          maps route → handler
    │
    ▼
GenerateTripHandler           orchestrates the slice
    ├─► GetTripPlanQueryValidator      validate & sanitize input
    ├─► PlanningService.ExtractIntentionAsync
    │       └─► IntentionPromptBuilder → OllamaClient
    ├─► WeatherService.GetWeatherForecastAsync
    │       └─► WeatherbitHttpClient
    ├─► PlanningService.GetActivityByWeatherAsync
    │       └─► ActivityPromptBuilder → OllamaClient
    ├─► PlacesService.GetTripPlacesAsync
    │       └─► GeoapifyHttpClient  (3 parallel calls)
    └─► PlanningService.GenerateTripPlanAsync
            └─► TripPlanPromptBuilder → OllamaClient
                    │
                    ▼
            TripPlanDto  →  HTTP 200
```

---

## Frontend — Angular 21

### Feature: `trip-planner`

The single slice that mirrors the backend feature. Everything lives under `src/app/features/trip-planner/`.

```
features/trip-planner/
│
├── models/
│   └── trip-plan.model.ts          TypeScript interfaces that mirror backend DTOs
│           PlaceDto, ActivityPlaceDto, RestaurantDto, ParkingDto
│           TripPlanDto             (the API response shape)
│           CreateTripPlanCommand   (the API request shape)
│           ChatTurn                (UI chat history entry)
│           DataResponse<T>         generic API envelope
│
├── services/
│   └── trip-planner.service.ts     HTTP client for POST /trip/v1/plan
│           generatePlan(command)   → Observable<DataResponse<TripPlanDto>>
│
└── components/
    │
    ├── chat-window/                ← feature entry point (smart component)
    │       chat-window.ts          manages: chatHistory signal, sessionId signal,
    │                               isLoading signal, prompt input, radius selector
    │                               calls TripPlannerService and updates chat turns
    │       chat-window.html
    │       chat-window.scss
    │
    ├── trip-plan/                  ← renders one complete AI trip plan response
    │       trip-plan.ts            input: TripPlanDto
    │       trip-plan.html
    │
    ├── activity-list/              ← renders the suggested activities
    │       activity-list.ts        input: ActivityPlaceDto[]
    │                               maps category strings → emoji icons
    │       activity-list.html
    │
    ├── restaurant-list/            ← renders the suggested restaurants
    │       restaurant-list.ts      input: RestaurantDto[]
    │                               derives cuisine label from Geoapify categories
    │       restaurant-list.html
    │
    └── parking-list/               ← renders the suggested parking spots
            parking-list.ts         input: ParkingDto[]
                                    maps parking type → emoji icon
            parking-list.html
```

### Shared — Cross-cutting UI Primitives

Reusable UI building blocks, independent of any feature.

```
shared/
└── components/
    ├── place-card/         generic card used by activity-list, restaurant-list & parking-list
    └── loading-spinner/    spinner shown while a plan is loading
```

### UI Data Flow (frontend)

```
User types prompt
    │
    ▼
ChatWindow (smart component)
    ├─ appends a loading ChatTurn to chatHistory signal
    ├─ calls TripPlannerService.generatePlan()
    │       └─► HTTP POST /trip/v1/plan  {prompt, sessionId, radiusInMeters}
    │
    └─ on response:
        ├─ sets sessionId signal (enables follow-up conversation)
        ├─ patches last ChatTurn with plan | error + durationMs
        └─ scrolls messages list to bottom
               │
               ▼
           TripPlan
               ├─► ActivityList  →  PlaceCard (×N)
               ├─► RestaurantList → PlaceCard (×N)
               └─► ParkingList   →  PlaceCard (×N)
```

---

## Slice Boundary Rules

| Rule | Rationale |
|---|---|
| Feature code lives exclusively inside its feature folder | Changes to one feature do not break others |
| Infrastructure clients are injected via interfaces | Features are decoupled from transport/external API details |
| DTOs are defined inside the feature, not in Infrastructure | The feature owns its contract; Infrastructure owns raw API models |
| Shared utilities have no knowledge of any feature | Keeps cross-cutting code stable and independently testable |
| Each `PromptBuilder` owns exactly one prompt template | Prompt changes are isolated; builders are independently replaceable |
| Frontend models mirror backend DTOs explicitly | Contract drift is immediately visible at compile time |
