HackerNews Challenge
Full-stack solution using ASP.NET Core Web API and Angular.

Run Backend (API)
cd HackerNews.Api
dotnet run

Runs at:
http://localhost:5206/swagger

Run Frontend (Angular)
cd hackernews-web
npm install
npm start

Runs at:
http://localhost:4200

NewsService → verifies API calls
NewsListComponent → verifies rendering, search, pagination

Tests
Backend (xUnit):
dotnet test HackerNews.Tests

Frontend (Jasmine/Karma):
cd hackernews-web
ng test --watch=false

Features
List newest Hacker News stories
Each story shows title, link (or fallback discussion link)
Search by title
Pagination with page size
Backend caching + DI
Automated tests (backend & frontend)
