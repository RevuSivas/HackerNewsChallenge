import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { NewsService } from '../../services/news.service';
import { NewsStory } from '../../models/news.model';

@Component({
  selector: 'app-news-list',
  templateUrl: './news-list.component.html',
  styleUrls: ['./news-list.component.scss']
})
export class NewsListComponent implements OnInit {
  stories: NewsStory[] = [];
  total = 0;
  page = 1;
  pageSize = 20;
  totalPages = 1;

  // Keep this as string control (safer for HTML input binding)
  search = new FormControl('', { nonNullable: true });

  loading = false;
  error: string | null = null;

  constructor(private api: NewsService) {}

  ngOnInit(): void {
    this.load();

    this.search.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe(() => {
        this.page = 1; // reset to first page on search
        this.load();
      });
  }

  load(): void {
    this.loading = true;
    this.error = null;

    this.api.getLatest(this.page, this.pageSize, this.search.value).subscribe({
      next: (r) => {
        this.stories = r.items;
        this.total = r.total;
        this.totalPages = Math.max(1, Math.ceil(this.total / this.pageSize));
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load stories. Try again.';
        console.error(err);
        this.loading = false;
      }
    });
  }

  prev(): void {
    if (this.page > 1) {
      this.page--;
      this.load();
    }
  }

  next(): void {
    if (this.page < this.totalPages) {
      this.page++;
      this.load();
    }
  }

  onPageSizeChange(e: Event): void {
    const target = e.target as HTMLSelectElement | null;
    if (!target) return;
    const val = Number(target.value);
    if (Number.isFinite(val) && val > 0) {
      this.setPageSize(val);
    }
  }

  setPageSize(sz: number): void {
    this.pageSize = sz;
    this.page = 1;
    this.load();
  }
}
