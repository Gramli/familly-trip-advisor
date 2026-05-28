import { Component, input } from '@angular/core';
import { TripPlanDto } from '../../models/trip-plan.model';
import { TripSectionTabs } from '../trip-section-tabs/trip-section-tabs';

@Component({
  selector: 'app-trip-plan',
  imports: [TripSectionTabs],
  templateUrl: './trip-plan.html',
  styleUrl: './trip-plan.scss'
})
export class TripPlan {
  readonly plan = input.required<TripPlanDto>();
}
