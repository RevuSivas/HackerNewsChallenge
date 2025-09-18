import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { NewsStory, PagedResponse } from '../models/news.model';


@Injectable({ providedIn: 'root' })
export class NewsService {
  private readonly base = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  getLatest(page = 1, pageSize = 20, search = ''): Observable<PagedResponse<NewsStory>> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    if (search) params = params.set('q', search);

    return this.http.get<PagedResponse<NewsStory>>(`${this.base}/api/news/latest`, { params });
  }

  getById(id: number): Observable<NewsStory> {
    return this.http.get<NewsStory>(`${this.base}/api/news/${id}`);
  }
}
