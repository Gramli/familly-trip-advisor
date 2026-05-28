import { Component, input } from '@angular/core';
import { ActivityPlaceDto } from '../../models/trip-plan.model';
import { PlaceCard } from '../../../../shared/components/place-card/place-card';
import { ActivityIconPipe } from '../../../../shared/pipes/activity-icon.pipe';

@Component({
  selector: 'app-activity-list',
  imports: [PlaceCard, ActivityIconPipe],
  templateUrl: './activity-list.html'
})
export class ActivityList {
  readonly activities = input.required<ActivityPlaceDto[]>();
}
