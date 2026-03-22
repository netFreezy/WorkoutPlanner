using ApexCharts;
using BlazorApp2.Components;
using BlazorApp2.Data;
using BlazorApp2.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<MaterializationService>();
builder.Services.AddScoped<SchedulingService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddScoped<PRDetectionService>();

builder.Services.AddApexCharts(e =>
{
    e.GlobalOptions = new ApexChartBaseOptions
    {
        Theme = new Theme { Mode = Mode.Dark },
        Chart = new Chart { Background = "transparent" }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
