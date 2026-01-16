// <copyright file="Program.cs" company="CommandMan">
// Copyright (c) CommandMan. All rights reserved.
// </copyright>

using CommandMan.Core.Interfaces;
using CommandMan.Infrastructure.Services;
using CommandMan.Web.Components;
using CommandMan.Web.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

const string OutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} | {Level:u3} | {LogSource} | {SourceContext} | {Message:lj}{NewLine}{Exception}";

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Properties.TryGetValue("LogSource", out var p) && p.ToString().Contains("Frontend"))
        .WriteTo.File("logs/commandman-frontend.log", outputTemplate: OutputTemplate, rollingInterval: RollingInterval.Day, shared: true))
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Properties.TryGetValue("LogSource", out var p) && p.ToString().Contains("Backend"))
        .WriteTo.File("logs/commandman-backend.log", outputTemplate: OutputTemplate, rollingInterval: RollingInterval.Day, shared: true))
    .WriteTo.File("logs/commandman-combined.log", outputTemplate: OutputTemplate, rollingInterval: RollingInterval.Day, shared: true));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Helper to decorate services
static T Decorate<T>(IServiceProvider provider, T implementation)
    where T : class
{
    var logger = provider.GetRequiredService<ILogger<T>>();
    return LoggingDecorator<T>.Create(implementation, logger);
}

builder.Services.AddScoped<IFileSystemService>(p =>
    Decorate<IFileSystemService>(p, ActivatorUtilities.CreateInstance<LocalFileSystemService>(p)));

builder.Services.AddScoped<IAppState>(p =>
    Decorate<IAppState>(p, ActivatorUtilities.CreateInstance<AppState>(p)));

builder.Services.AddScoped<IProgressService>(p =>
    Decorate<IProgressService>(p, ActivatorUtilities.CreateInstance<ProgressService>(p)));

builder.Services.AddScoped<IFavoritesService>(p =>
    Decorate<IFavoritesService>(p, ActivatorUtilities.CreateInstance<FavoritesService>(p)));

builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<IClipboardService>(p =>
    Decorate<IClipboardService>(p, ActivatorUtilities.CreateInstance<ClipboardService>(p)));

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
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
