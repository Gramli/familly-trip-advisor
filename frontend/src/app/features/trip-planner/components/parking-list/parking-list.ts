import { Component, input } from '@angular/core';
import { ParkingDto } from '../../models/trip-plan.model';
import { PlaceCard } from '../../../../shared/components/place-card/place-card';

@Component({
  selector: 'app-parking-list',
  imports: [PlaceCard],
  templateUrl: './parking-list.html'
})
export class ParkingList {
  readonly parking = input.required<ParkingDto[]>();

  parkingIcon(type: string | null): string {
    if (!type) return '🅿️';
    if (type.includes('underground')) return '🏗️';
    if (type.includes('multistorey')) return '🏢';
    return '🅿️';
  }
}
