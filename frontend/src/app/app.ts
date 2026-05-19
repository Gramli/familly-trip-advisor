import { Component } from '@angular/core';
import { ChatWindow } from './features/trip-planner/components/chat-window/chat-window';

@Component({
  selector: 'app-root',
  imports: [ChatWindow],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {}
