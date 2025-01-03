using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Serilog;
using SS14.Weaver.Services;
using SU_Replays.Components;
using SU.Replays.Configuration;
using SU.Replays.Database;
using SU.Replays.Helpers;
using SU.Replays.Services;
using SU.Replays.Services.ReplayParticipantsProvider;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddYamlFile("appsettings.yaml", false, true);
builder.Configuration.AddYamlFile($"appsettings.{builder.Environment.EnvironmentName}.yaml", true, true);
builder.Configuration.AddYamlFile("appsettings.Secret.yaml", true, true);

var serverConfiguration = new ServerConfiguration();
builder.Configuration.Bind(ServerConfiguration.Name, serverConfiguration);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));
builder.Logging.AddSerilog();

builder.Services.AddAntiforgery();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(serverConfiguration.CorsOrigins.ToArray());
        policy.AllowCredentials();
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(serverConfiguration.ConnectionString);

    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
});

builder.Host.UseSystemd();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();

builder.Services.AddSingleton<ReplayUpdater>();
builder.Services.AddHostedService<BackgroundServiceStarter<ReplayUpdater>>();

builder.Services.AddScoped<ReplayHelper>();

#if DEBUG
builder.Services.AddScoped<IReplayParticipantsProvider, DebugReplayParticipantsProvider>();
#else
builder.Services.AddScoped<IReplayParticipantsProvider, ReplayParticipantsProvider>();
#endif

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options =>
    {
        options.Authority = serverConfiguration.OicdAuthority;
        options.ClientId = serverConfiguration.OicdClientId;
        options.ClientSecret = serverConfiguration.OicdClientSecret;

        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.ResponseType = OpenIdConnectResponseType.Code;

        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;

        options.MapInboundClaims = false;
        options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
        options.TokenValidationParameters.RoleClaimType = "roles";
    });

var app = builder.Build();

var logger = Log.ForContext<Program>();

{
    using var dbScope = app.Services.CreateScope();
    logger.Information("Migrating database...");
    var sw = Stopwatch.StartNew();
    var db = dbScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    sw.Stop();
    logger.Information("Database migrated in {Time}", sw.Elapsed);
}

logger.Information("Environment: {Environment}", builder.Environment.EnvironmentName);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

if (serverConfiguration.PathBase != null)
{
    app.UsePathBase(serverConfiguration.PathBase);
}

if (serverConfiguration.UseForwardedHeaders)
{
    app.UseForwardedHeaders();
}

if ((app.Environment.IsProduction() || app.Environment.IsStaging()) && serverConfiguration.UseHttps)
{
    app.UseHttpsRedirection();
    app.UseHsts();
}


app.UseSerilogRequestLogging();

app.UseRouting();
app.MapStaticAssets();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.UseCors();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapControllers();

app.Run();