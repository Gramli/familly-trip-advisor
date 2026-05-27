# TODO

- [x] Extend intent generation to support Geoapify place categories
- [x] Apply hard rules for determining whether a place is indoor, outdoor, or both
- [ ] When the user continues the conversation or suggests a different model, exclude the current place selection from the context
- [x] **`PlanningService.ResolveCategories` — expand sparse category selections**: if the model selects fewer than 3 categories, automatically supplement the selection with additional relevant categories from the activity map before querying Geoapify
- [x] **`PlacesService` — retry with broader categories on empty results**: when Geoapify returns zero activity results, broaden the category set (e.g. fall back to parent/sibling categories) and retry the request instead of surfacing an empty plan
- [x] **`PlanningService` — validate and retry incomplete model output**: after deserializing the trip plan, assert that the response includes at minimum a parking suggestion and a non-empty summary; if either is missing, treat the result as failed and let the existing retry pipeline request a new response from the model
