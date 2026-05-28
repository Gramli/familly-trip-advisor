import { Component, computed, input, signal } from '@angular/core';
import { TripPlanDto } from '../../models/trip-plan.model';
import { ActivityList } from '../activity-list/activity-list';
import { RestaurantList } from '../restaurant-list/restaurant-list';
import { ParkingList } from '../parking-list/parking-list';

type TabId = 'activities' | 'restaurants' | 'parking';

interface Tab {
  id: TabId;
  label: string;
  icon: string;
  count: number;
}

@Component({
  selector: 'app-trip-section-tabs',
  imports: [ActivityList, RestaurantList, ParkingList],
  templateUrl: './trip-section-tabs.html',
  styleUrl: './trip-section-tabs.scss',
})
export class TripSectionTabs {
  readonly plan = input.required<TripPlanDto>();
  readonly activeTab = signal<TabId>('activities');

  readonly tabs = computed<Tab[]>(() => {
    const p = this.plan();
    const all: Tab[] = [
      { id: 'activities',  label: 'Activities',  icon: '🎯', count: p.suggestedActivities.length },
      { id: 'restaurants', label: 'Eat',          icon: '🍽️', count: p.suggestedRestaurants.length },
      { id: 'parking',     label: 'Parking',      icon: '🅿️', count: p.suggestedParking.length },
    ];
    const visible = all.filter((t) => t.count > 0);
    // Ensure the active tab is always valid when plan data changes
    if (visible.length && !visible.find((t) => t.id === this.activeTab())) {
      this.activeTab.set(visible[0].id);
    }
    return visible;
  });

  setTab(id: TabId): void {
    this.activeTab.set(id);
  }
}
