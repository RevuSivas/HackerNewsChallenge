export interface NewsStory {
  id: number;
  title: string;
  url: string;
  discussionUrl: string;
  by?: string;
  score: number;
  time: string;
  comments: number;
}

export interface PagedResponse<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}
