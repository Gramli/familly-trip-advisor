import { Component, input, model } from '@angular/core';

@Component({
  selector: 'app-radius-picker',
  templateUrl: './radius-picker.html',
  styleUrl: './radius-picker.scss',
})
export class RadiusPicker {
  readonly radii = [10000, 20000, 30000, 50000, 75000, 100000];
  readonly selectedRadius = model<number>(10000);
  readonly disabled = input(false);

  select(r: number): void {
    if (!this.disabled()) {
      this.selectedRadius.set(r);
    }
  }

  label(r: number): string {
    return `${r / 1000} km radius`;
  }
}
