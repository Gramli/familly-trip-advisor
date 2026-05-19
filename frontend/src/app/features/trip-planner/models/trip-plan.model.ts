export interface PlaceDto {
  name: string | null;
  address: string | null;
  distanceMeters: number | null;
  latitude: number;
  longitude: number;
  placeId: string | null;
}

export interface ActivityPlaceDto extends PlaceDto {
  activityType: number;
  category: string | null;
}

export interface RestaurantDto extends PlaceDto {
  categories: string[];
}

export interface ParkingDto extends PlaceDto {
  parkingType: string | null;
}

export interface TripPlanDto {
  sessionId: string;
  destination: string | null;
  date: string;
  activityType: string | null;
  suggestedActivities: ActivityPlaceDto[];
  suggestedRestaurants: RestaurantDto[];
  suggestedParking: ParkingDto[];
  planSummary: string | null;
}

export interface CreateTripPlanCommand {
  prompt: string;
  sessionId?: string;
  radiusInMeters?: number;
}

export interface DataResponse<T> {
  data: T | null;
  errors: string[];
}

export interface ChatTurn {
  prompt: string;
  plan: TripPlanDto | null;
  error: string | null;
  loading: boolean;
  durationMs?: number;
}
