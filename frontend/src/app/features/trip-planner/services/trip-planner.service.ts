import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CreateTripPlanCommand, DataResponse, TripPlanDto } from '../models/trip-plan.model';

@Injectable({ providedIn: 'root' })
export class TripPlannerService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/trip/v1/plan`;

  generatePlan(command: CreateTripPlanCommand): Observable<DataResponse<TripPlanDto>> {
    return this.http.post<DataResponse<TripPlanDto>>(this.apiUrl, command);
  }
}
