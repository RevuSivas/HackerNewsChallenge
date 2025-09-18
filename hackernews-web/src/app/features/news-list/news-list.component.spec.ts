import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { of, Subject, throwError } from 'rxjs';
import { NewsListComponent } from './news-list.component';
import { NewsService } from '../../services/news.service';
import { NewsStory, PagedResponse } from '../../models/news.model';

describe('NewsListComponent', () => {
  let fixture: ComponentFixture<NewsListComponent>;
  let component: NewsListComponent;

  let getLatestSpy: jasmine.Spy;

  const makePage = (items: Partial<NewsStory>[], total = items.length, page = 1, pageSize = 20): PagedResponse<NewsStory> => ({
    items: items.map(i => ({
      id: 1,
      title: 'Hello',
      url: '#',
      discussionUrl: '#',
      score: 1,
      time: new Date().toISOString(),
      comments: 0,
      ...i
    })) as NewsStory[],
    total, page, pageSize
  });

  beforeEach(async () => {
    const stub = {
      getLatest: (..._args: any[]) => of(makePage([{ id: 1, title: 'Hello' }]))
    };
    getLatestSpy = spyOn(stub, 'getLatest').and.callThrough();

    await TestBed.configureTestingModule({
      declarations: [NewsListComponent],
      imports: [ReactiveFormsModule],
      providers: [{ provide: NewsService, useValue: stub }]
    }).compileComponents();

    fixture = TestBed.createComponent(NewsListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create and load first page', () => {
    expect(component).toBeTruthy();
    expect(getLatestSpy).toHaveBeenCalledWith(1, 20, '');
    expect(component.stories.length).toBe(1);

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Hello');
  });

  it('should disable Prev on first page and enable Next when there are more pages', () => {
    // simulate total > pageSize so next is enabled
    component.total = 100;
    component.pageSize = 20;
    component.totalPages = 5;
    fixture.detectChanges();

    const buttons = fixture.debugElement.queryAll(By.css('button'));
    const prevBtn = buttons[0].nativeElement as HTMLButtonElement;
    const nextBtn = buttons[1].nativeElement as HTMLButtonElement;

    expect(prevBtn.disabled).toBeTrue();
    expect(nextBtn.disabled).toBeFalse();
  });

  it('should call service again when clicking Next', () => {
    const stub = TestBed.inject(NewsService) as any;
    getLatestSpy.calls.reset();

    // set multiple pages
    component.total = 100;
    component.pageSize = 20;
    component.totalPages = 5;
    fixture.detectChanges();

    const nextBtn = fixture.debugElement.queryAll(By.css('button'))[1].nativeElement as HTMLButtonElement;
    nextBtn.click();

    expect(component.page).toBe(2);
    expect(getLatestSpy).toHaveBeenCalledWith(2, 20, '');
  });

  it('should reset to page 1 and query when typing in the search box (debounced)', fakeAsync(() => {
    const input = fixture.debugElement.query(By.css('input')).nativeElement as HTMLInputElement;
    getLatestSpy.calls.reset();

    input.value = 'angular';
    input.dispatchEvent(new Event('input'));

    // debounceTime(300)
    tick(300);
    fixture.detectChanges();

    expect(component.page).toBe(1);
    expect(getLatestSpy).toHaveBeenCalledWith(1, 20, 'angular');
  }));

  it('should change page size via onPageSizeChange', () => {
    getLatestSpy.calls.reset();

    const select = fixture.debugElement.query(By.css('select')).nativeElement as HTMLSelectElement;
    select.value = '50';
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();

    expect(component.pageSize).toBe(50);
    expect(component.page).toBe(1);
    expect(getLatestSpy).toHaveBeenCalledWith(1, 50, '');
  });

  it('should show "No stories found." when list is empty', () => {
    // Force empty state
    component.stories = [];
    component.loading = false;
    component.error = null;
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('No stories found.');
  });

  it('should show error message and not crash when service errors', () => {
    // Replace the stub on the fly for this test
    const svc = TestBed.inject(NewsService) as any;
    (svc.getLatest as jasmine.Spy).and.returnValue(throwError(() => new Error('boom')));

    component.load();
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Failed to load stories. Try again.');
  });
});
