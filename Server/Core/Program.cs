using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using BattleSimulator.Server;
using BattleSimulator.Server.Auth;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Workers;
using BattleSimulator.Server.Database;
using BattleSimulator.Server.Hubs.EventHandling;

const string CorsPolicyName = "CorsDefault";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDistributedMemoryCache();

// Database services
builder.Services.AddScoped<IJsonSerializerWrapper, JsonSerializerWrapper>();
builder.Services.AddSingleton<IGameDbConverter, GameDbConverter>();
builder.Services.AddSingleton<IGameDb, GameDb>();

// state
builder.Services.AddSingleton<IGameHubState, GameHubState>();
builder.Services.AddSingleton<IBattleCollection, BattleCollection>();
builder.Services.AddSingleton<IMovementIntentionCollection, MovementIntentionCollection>();
builder.Services.AddSingleton<IConnectionMapping, ConnectionMapping>();
builder.Services.AddSingleton<IBattleRequestCollection, BattleRequestCollection>();
builder.Services.AddSingleton<IAttacksRequestedList, AttacksRequestedList>();

// handlers
builder.Services.AddSingleton<IBattleEventsHandler, BattleEventsHandler>();
builder.Services.AddSingleton<IRequestsHandler, RequestsHandler>();
builder.Services.AddSingleton<IBattleHandler, BattleHandler>();
builder.Services.AddSingleton<IGameEngine, GameEngine>();

// other services
builder.Services.AddSingleton<ISkillProvider, SkillProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IServerConfig, ServerConfig>();

var config = new ServerConfig(builder.Configuration);

builder.Services.AddSignalR();

builder.Services.AddCors(options => {
    options.AddPolicy(name: CorsPolicyName, policy => {
        Console.WriteLine($"Allowed origin: {config.AllowedOrigin}");
        policy.WithOrigins(config.AllowedOrigin)
            .WithMethods("POST", "PUT", "GET", "OPTIONS")
            .AllowAnyHeader();
    });
});

builder.Services.AddAuthorization(options => {
    options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireClaim(config.ClaimTypeName);
    });
});


builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            IssuerSigningKey = new SymmetricSecurityKey(config.SecretKey),
            LifetimeValidator = (before, expires, token, parameters) => expires > DateTime.UtcNow,
            ValidIssuer = config.Issuer,
            ValidAudience = config.Audience,
            NameClaimType = config.ClaimTypeName
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = context.Request.Query["access_token"];
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddHostedService<MovementIntentionsWorker>();
builder.Services.AddHostedService<AttacksHandlerWorker>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors(CorsPolicyName);

app.MapHub<GameHub>("/hubs/game");
app.MapControllers();

app.Run();
