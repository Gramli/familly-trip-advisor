import { Component, output } from '@angular/core';

@Component({
  selector: 'app-suggested-prompts',
  templateUrl: './suggested-prompts.html',
  styleUrl: './suggested-prompts.scss',
})
export class SuggestedPrompts {
  readonly promptSelected = output<string>();

  readonly examples = [
    'Family day in Prague with kids who love museums',
    'Weekend trip to Vienna with a 5-year-old',
    'Outdoor activities near Budapest for teens',
    'Rainy day in Berlin with two young children',
  ];
}
