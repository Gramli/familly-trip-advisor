import { Component, input } from '@angular/core';
import { ActivityPlaceDto } from '../../models/trip-plan.model';
import { PlaceCard } from '../../../../shared/components/place-card/place-card';

@Component({
  selector: 'app-activity-list',
  imports: [PlaceCard],
  templateUrl: './activity-list.html'
})
export class ActivityList {
  readonly activities = input.required<ActivityPlaceDto[]>();

  categoryIcon(category: string | null): string {
    if (!category) return '🎯';
    if (category.includes('museum')) return '🏛️';
    if (category.includes('zoo')) return '🦁';
    if (category.includes('aquarium')) return '🐠';
    if (category.includes('theme') || category.includes('amusement')) return '🎢';
    if (category.includes('sport') || category.includes('pool')) return '🏊';
    if (category.includes('cinema') || category.includes('theatre')) return '🎭';
    if (category.includes('park') || category.includes('nature')) return '🌳';
    return '🎯';
  }
}
