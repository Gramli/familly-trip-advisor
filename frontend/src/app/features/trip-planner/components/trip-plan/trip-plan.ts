import { Component, input } from '@angular/core';
import { TripPlanDto } from '../../models/trip-plan.model';
import { ActivityList } from '../activity-list/activity-list';
import { RestaurantList } from '../restaurant-list/restaurant-list';
import { ParkingList } from '../parking-list/parking-list';

@Component({
  selector: 'app-trip-plan',
  imports: [ActivityList, RestaurantList, ParkingList],
  templateUrl: './trip-plan.html',
  styleUrl: './trip-plan.scss'
})
export class TripPlan {
  readonly plan = input.required<TripPlanDto>();
}
