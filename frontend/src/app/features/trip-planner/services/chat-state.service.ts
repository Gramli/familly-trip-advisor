import { Injectable, signal } from '@angular/core';
import { ChatTurn } from '../models/trip-plan.model';

@Injectable({ providedIn: 'root' })
export class ChatStateService {
  readonly chatHistory = signal<ChatTurn[]>([]);
  readonly sessionId = signal<string | null>(null);
  readonly isLoading = signal(false);
  readonly selectedRadius = signal(10000);

  clearSession(): void {
    this.chatHistory.set([]);
    this.sessionId.set(null);
    // selectedRadius is intentionally preserved — it is a user preference
  }

  updateLastTurn(patch: Partial<ChatTurn>): void {
    this.chatHistory.update((h) => {
      const copy = [...h];
      copy[copy.length - 1] = { ...copy[copy.length - 1], ...patch };
      return copy;
    });
  }
}
