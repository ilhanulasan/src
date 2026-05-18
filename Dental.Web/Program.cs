using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dental.Web.Data;
using Dental.Web.Models;
using Dental.Web.Services;
using Dental.Web.Services.OpenDental;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                 ?? throw new InvalidOperationException("Jwt configuration section is missing.");

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

builder.Services.Configure<OpenDentalOptions>(builder.Configuration.GetSection(OpenDentalOptions.SectionName));
builder.Services.AddHttpClient<IOpenDentalApiClient, OpenDentalApiClient>((sp, client) =>
{
    var od = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenDentalOptions>>().Value;
    client.BaseAddress = new Uri(od.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromMinutes(2);
});

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

builder.Services.AddControllersWithViews()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var corsOrigin = builder.Configuration["Cors:FrontendOrigin"] ?? "http://localhost:4300";
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
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapStaticAssets();
    app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    app.MapControllers();

    if (app.Configuration.GetValue("Database:ApplyMigrations", true))
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
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
