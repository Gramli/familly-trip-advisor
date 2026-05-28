import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'activityIcon', pure: true, standalone: true })
export class ActivityIconPipe implements PipeTransform {
  transform(category: string | null): string {
    if (!category) return '🎯';
    const c = category.toLowerCase();
    if (c.includes('museum'))                         return '🏛️';
    if (c.includes('zoo'))                            return '🦁';
    if (c.includes('aquarium'))                       return '🐠';
    if (c.includes('theme') || c.includes('amusement')) return '🎢';
    if (c.includes('sport') || c.includes('pool'))   return '🏊';
    if (c.includes('cinema') || c.includes('theatre')) return '🎭';
    if (c.includes('park') || c.includes('nature'))  return '🌳';
    return '🎯';
  }
}
