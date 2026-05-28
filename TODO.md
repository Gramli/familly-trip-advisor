# TODO

- [x] Extend intent generation to support Geoapify place categories
- [x] Apply hard rules for determining whether a place is indoor, outdoor, or both
- [ ] When the user continues the conversation or suggests a different place, exclude the current place selection from the context
- [x] **`PlanningService.ResolveCategories` — expand sparse category selections**: if the model selects fewer than 3 categories, automatically supplement the selection with additional relevant categories from the activity map before querying Geoapify
- [x] **`PlacesService` — retry with broader categories on empty results**: when Geoapify returns zero activity results, broaden the category set (e.g. fall back to parent/sibling categories) and retry the request instead of surfacing an empty plan
- [x] **`PlanningService` — validate and retry incomplete model output**: after deserializing the trip plan, assert that the response includes at minimum a parking suggestion and a non-empty summary; if either is missing, treat the result as failed and let the existing retry pipeline request a new response from the model

### Architecture & Performance Improvements

- [ ] **Performance (`GeoapifyHttpClient`, `WeatherbitHttpClient`)**: Introduce `IMemoryCache` (or `IDistributedCache`) for external API calls to avoid re-fetching data.
- [ ] **Latency (`GenerateTripHandler.cs`)**: Fan out weather and places fetches in parallel with `Task.WhenAll` to reduce network round-trips.
- [ ] **Scalability (`OllamaClient.cs`)**: Replace in-process session cache with `IDistributedCache` and abstract behind an `ISessionHistoryStore` interface for session continuity.
- [ ] **Reliability (`GenerateTripEndpoint.cs`)**: Implement request deduplication using an `idempotencyKey` field on `CreateTripPlanCommand` with a short-lived cache.
- [ ] **Latency (`PlacesService.cs`)**: Fire both user-category and fallback-category queries in parallel using `Task.WhenAny` instead of a sequential retry.
- [ ] **Architecture (`PlanningService.cs`)**: Replace isolated static `RetryPipeline<T>` instances with a single non-generic `ResiliencePipeline` registered via DI.
- [ ] **Security (`GetTripPlanQueryValidator.cs`)**: Strengthen prompt injection detection by adding a text normalisation step before pattern matching.
