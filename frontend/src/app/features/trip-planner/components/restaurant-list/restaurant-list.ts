import { Component, input } from '@angular/core';
import { RestaurantDto } from '../../models/trip-plan.model';
import { PlaceCard } from '../../../../shared/components/place-card/place-card';

@Component({
  selector: 'app-restaurant-list',
  imports: [PlaceCard],
  templateUrl: './restaurant-list.html'
})
export class RestaurantList {
  readonly restaurants = input.required<RestaurantDto[]>();

  cuisine(categories: string[]): string | null {
    const food = categories.find(c => c.startsWith('catering.'));
    if (!food) return null;
    return food.replace('catering.', '').replace(/[._]/g, ' ').trim();
  }
}
