import {
  afterNextRender,
  Component,
  ElementRef,
  inject,
  Injector,
  signal,
  viewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TripPlannerService } from '../../services/trip-planner.service';
import { ChatTurn } from '../../models/trip-plan.model';
import { LoadingSpinner } from '../../../../shared/components/loading-spinner/loading-spinner';
import { TripPlan } from '../trip-plan/trip-plan';

@Component({
  selector: 'app-chat-window',
  imports: [CommonModule, FormsModule, LoadingSpinner, TripPlan],
  templateUrl: './chat-window.html',
  styleUrl: './chat-window.scss',
})
export class ChatWindow {
  private readonly tripPlannerService = inject(TripPlannerService);
  private readonly injector = inject(Injector);
  private readonly messagesRef = viewChild<ElementRef<HTMLElement>>('messages');

  readonly chatHistory = signal<ChatTurn[]>([]);
  readonly sessionId = signal<string | null>(null);
  readonly isLoading = signal(false);

  currentPrompt = '';

  readonly radiuses = Array.from({ length: 10 }, (_, i) => (i + 1) * 5000);
  selectedRadius = 5000;

  sendPrompt(): void {
    const prompt = this.currentPrompt.trim();
    if (!prompt || this.isLoading()) {
      return;
    }

    this.scrollToBottom();

    const startTime = Date.now();
    this.chatHistory.update((h) => [...h, { prompt, plan: null, error: null, loading: true }]);
    this.currentPrompt = '';
    this.isLoading.set(true);

    this.tripPlannerService
      .generatePlan({
        prompt,
        sessionId: this.sessionId() ?? undefined,
        radiusInMeters: this.selectedRadius,
      })
      .subscribe({
        next: (response) => {
          const durationMs = Date.now() - startTime;
          if (response.errors?.length) {
            this.updateLastTurn({
              loading: false,
              error: response.errors.join(', '),
              plan: null,
              durationMs,
            });
          } else if (response.data) {
            this.sessionId.set(response.data.sessionId);
            this.updateLastTurn({ loading: false, plan: response.data, error: null, durationMs });
          } else {
            this.updateLastTurn({
              loading: false,
              error: 'No plan returned. Please try again.',
              plan: null,
              durationMs,
            });
          }
          this.isLoading.set(false);
          this.scrollToBottom();
        },
        error: () => {
          const durationMs = Date.now() - startTime;
          this.updateLastTurn({
            loading: false,
            error: 'Failed to connect to the server. Please try again.',
            plan: null,
            durationMs,
          });
          this.isLoading.set(false);
        },
      });
  }

  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendPrompt();
    }
  }

  private updateLastTurn(patch: Partial<ChatTurn>): void {
    this.chatHistory.update((h) => {
      const copy = [...h];
      copy[copy.length - 1] = { ...copy[copy.length - 1], ...patch };
      return copy;
    });
  }

  private scrollToBottom(): void {
    afterNextRender(
      () => {
        const el = this.messagesRef()?.nativeElement;
        if (el) el.scrollTop = el.scrollHeight;
      },
      { injector: this.injector },
    );
  }
}
