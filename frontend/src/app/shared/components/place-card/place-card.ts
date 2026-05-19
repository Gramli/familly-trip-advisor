import { Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-place-card',
  templateUrl: './place-card.html',
  styleUrl: './place-card.scss'
})
export class PlaceCard {
  readonly name = input<string | null>(null);
  readonly address = input<string | null>(null);
  readonly distanceMeters = input<number | null>(null);
  readonly latitude = input<number>(0);
  readonly longitude = input<number>(0);
  readonly badge = input<string | null>(null);
  readonly icon = input<string>('📍');

  readonly distance = computed(() => {
    const d = this.distanceMeters();
    if (d === null || d === undefined) return null;
    return d >= 1000 ? `${(d / 1000).toFixed(1)} km` : `${Math.round(d)} m`;
  });

  readonly mapsUrl = computed(() =>
    `https://www.google.com/maps/search/?api=1&query=${this.latitude()},${this.longitude()}`
  );
}
