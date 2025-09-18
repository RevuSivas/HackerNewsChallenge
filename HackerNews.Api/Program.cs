using System.Net.Http.Headers;
using HackerNews.Api.ApiClients;
using HackerNews.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// services
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IHackerNewsClient, HackerNewsClient>(c =>
{
    c.BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/");
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddSingleton<INewsService, NewsService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for local Angular (adjust for Azure later)
builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(p => p
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();

// minimal API endpoints
app.MapGet("/api/news/latest", async (
    int page,
    int pageSize,
    string? q,
    INewsService newsService,
    CancellationToken ct) =>
{
    // allow callers to omit page/pageSize
    page = page <= 0 ? 1 : page;
    pageSize = pageSize <= 0 ? 20 : pageSize;

    var result = await newsService.GetLatestAsync(page, pageSize, q, ct);
    return Results.Ok(result);
});

app.MapGet("/api/news/{id:long}", async (long id, INewsService newsService, CancellationToken ct) =>
{
    var story = await newsService.GetByIdAsync(id, ct);
    return story is null ? Results.NotFound() : Results.Ok(story);
});

app.Run();
