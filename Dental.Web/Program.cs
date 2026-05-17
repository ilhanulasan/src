using System.IO;
using System.Text.Json.Serialization;
using Dental.Web.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    var logsDir = Path.Combine(AppContext.BaseDirectory, "Logs");
    Directory.CreateDirectory(logsDir);

    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            Path.Combine(logsDir, "dental-.log"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 31);
});

var connectionString = builder.Configuration.GetConnectionString("PostGreConnection")
                       ?? builder.Configuration["PostGreConnection"];

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllersWithViews()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var corsOrigin = builder.Configuration["Cors:FrontendOrigin"] ?? "http://localhost:4200";
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Frontend",
        policy => policy.WithOrigins(corsOrigin.TrimEnd('/'))
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

try
{
    Log.Information("Dental.Web started");

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseCors("Frontend");
    app.UseAuthorization();

    app.MapStaticAssets();
    app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    app.MapControllers();

    if (app.Configuration.GetValue("Database:ApplyMigrations", true))
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
        }
    }

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
