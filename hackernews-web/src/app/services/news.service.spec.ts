// src/app/services/news.service.spec.ts
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { NewsService } from './news.service';
import { PagedResponse, NewsStory } from '../models/news.model';

describe('NewsService (functional providers)', () => {
  let service: NewsService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),         
        provideHttpClientTesting(),  
        NewsService
      ]
    });

    service = TestBed.inject(NewsService);
    http = TestBed.inject(HttpTestingController); 
  });

  afterEach(() => http.verify());

  it('should request latest stories with page, pageSize and q', () => {
    service.getLatest(2, 10, 'angular').subscribe();

    const req = http.expectOne(r =>
      r.url.includes('/api/news/latest') &&
      r.params.get('page') === '2' &&
      r.params.get('pageSize') === '10' &&
      r.params.get('q') === 'angular'
    );
    expect(req.request.method).toBe('GET');
    req.flush({ items: [], total: 0, page: 2, pageSize: 10 } as PagedResponse<NewsStory>);
  });

  it('should omit q when empty', () => {
    service.getLatest(1, 20, '').subscribe();

    const req = http.expectOne(r =>
      r.url.includes('/api/news/latest') &&
      r.params.get('page') === '1' &&
      r.params.get('pageSize') === '20' &&
      !r.params.has('q')
    );
    expect(req.request.method).toBe('GET');
    req.flush({ items: [], total: 0, page: 1, pageSize: 20 } as PagedResponse<NewsStory>);
  });

  it('should request single story by id', () => {
    service.getById(123).subscribe();

    const req = http.expectOne(r => r.url.includes('/api/news/123'));
    expect(req.request.method).toBe('GET');
    req.flush({
      id: 123,
      title: 'Sample',
      url: '#',
      discussionUrl: '#',
      score: 10,
      time: new Date().toISOString(),
      comments: 0
    } as NewsStory);
  });
});
