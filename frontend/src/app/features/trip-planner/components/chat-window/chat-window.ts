import {
  afterNextRender,
  Component,
  ElementRef,
  inject,
  Injector,
  viewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TripPlannerService } from '../../services/trip-planner.service';
import { ChatStateService } from '../../services/chat-state.service';
import { LoadingSpinner } from '../../../../shared/components/loading-spinner/loading-spinner';
import { TripPlan } from '../trip-plan/trip-plan';
import { RadiusPicker } from '../radius-picker/radius-picker';
import { SuggestedPrompts } from '../suggested-prompts/suggested-prompts';

@Component({
  selector: 'app-chat-window',
  imports: [CommonModule, FormsModule, LoadingSpinner, TripPlan, RadiusPicker, SuggestedPrompts],
  templateUrl: './chat-window.html',
  styleUrl: './chat-window.scss',
})
export class ChatWindow {
  private readonly tripPlannerService = inject(TripPlannerService);
  private readonly state = inject(ChatStateService);
  private readonly injector = inject(Injector);
  private readonly messagesRef = viewChild<ElementRef<HTMLElement>>('messages');

  readonly chatHistory = this.state.chatHistory;
  readonly isLoading = this.state.isLoading;
  readonly selectedRadius = this.state.selectedRadius;

  currentPrompt = '';

  sendPrompt(): void {
    const prompt = this.currentPrompt.trim();
    if (!prompt || this.isLoading()) {
      return;
    }

    this.scrollToBottom();

    const startTime = Date.now();
    this.state.chatHistory.update((h) => [
      ...h,
      { prompt, plan: null, error: null, loading: true },
    ]);
    this.currentPrompt = '';
    this.state.isLoading.set(true);

    this.tripPlannerService
      .generatePlan({
        prompt,
        sessionId: this.state.sessionId() ?? undefined,
        radiusInMeters: this.selectedRadius(),
      })
      .subscribe({
        next: (response) => {
          const durationMs = Date.now() - startTime;
          if (response.errors?.length) {
            this.state.updateLastTurn({
              loading: false,
              error: response.errors.join(', '),
              plan: null,
              durationMs,
            });
          } else if (response.data) {
            this.state.sessionId.set(response.data.sessionId);
            this.state.updateLastTurn({
              loading: false,
              plan: response.data,
              error: null,
              durationMs,
            });
          } else {
            this.state.updateLastTurn({
              loading: false,
              error: 'No plan returned. Please try again.',
              plan: null,
              durationMs,
            });
          }
          this.state.isLoading.set(false);
          this.scrollToBottom();
        },
        error: () => {
          const durationMs = Date.now() - startTime;
          this.state.updateLastTurn({
            loading: false,
            error: 'Failed to connect to the server. Please try again.',
            plan: null,
            durationMs,
          });
          this.state.isLoading.set(false);
        },
      });
  }

  clearSession(): void {
    this.state.clearSession();
  }

  fillPrompt(prompt: string): void {
    this.currentPrompt = prompt;
  }

  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendPrompt();
    }
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

