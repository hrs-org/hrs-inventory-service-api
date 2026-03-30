using System.Security.Claims;
using FluentValidation;
using HRS.API.Filters;
using HRS.API.Middleware;
using HRS.API.Services;
using HRS.API.Services.Interfaces;
using HRS.API.Validators.Item;
using HRS.Domain.Interfaces;
using HRS.Infrastructure;
using HRS.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using HRS.Shared.Core.Interfaces;

using HRS.Shared.Core.Authorization;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// MongoDB Configuration
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDb")
    ?? "mongodb://localhost:27017";
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoConnectionString));
builder.Services.AddScoped<MongoContext>();

builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IPackageService, PackageService>();
builder.Services.AddScoped(typeof(ICrudRepository<>), typeof(CrudRepository<>));
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IPackageRepository, PackageRepository>();
builder.Services.AddScoped<IAppConfiguration, AppConfiguration>();
builder.Services.AddHttpContextAccessor();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers(options => { options.Filters.Add<ValidationFilter>(); });

builder.Services.AddValidatorsFromAssemblyContaining<AddItemRequestDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateItemRequestDtoValidator>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HRS API", Version = "v1" });

    // 🔑 Enable JWT Bearer in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your JWT token.\n\nExample: **Bearer eyJhbGciOi...**"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

builder.Services.AddAutoMapper(cfg => { }, typeof(Program));

var auth0Domain = builder.Configuration["Auth0:Domain"]!;
var auth0Audience = builder.Configuration["Auth0:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{auth0Domain}/";
        options.Audience = auth0Audience;
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = $"https://{auth0Domain}/",
            ValidAudience = auth0Audience,
            NameClaimType = "sub"
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var claims = context.Principal?.Claims.ToList() ?? new List<Claim>();
                var subClaim = claims.FirstOrDefault(c => c.Type == "sub");
                var claimsIdentity = (ClaimsIdentity)context.Principal?.Identity!;

                if (subClaim != null && !claims.Any(c => c.Type == ClaimTypes.NameIdentifier))
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, subClaim.Value));
                }

                if (!claims.Any(c => c.Type == "userId"))
                {
                    var userIdClaim = claims.FirstOrDefault(c =>
                        c.Type == "https://hrs-api/userId" || c.Type == "https://hrs-api/user_id");

                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                    {
                        claimsIdentity.AddClaim(new Claim("userId", userId.ToString()));
                    }
                }

                if (!claims.Any(c => c.Type == "storeId"))
                {
                    var storeIdClaim = claims.FirstOrDefault(c =>
                        c.Type == "https://hrs-api/storeId" || c.Type == "https://hrs-api/store_id");

                    if (storeIdClaim != null && int.TryParse(storeIdClaim.Value, out var storeId))
                    {
                        claimsIdentity.AddClaim(new Claim("storeId", storeId.ToString()));
                    }
                }

                await Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Item service scopes
    options.AddPolicy("read:item", policy =>
        policy.Requirements.Add(new PermissionRequirement("read:item")));
    options.AddPolicy("write:item", policy =>
        policy.Requirements.Add(new PermissionRequirement("write:item")));
    options.AddPolicy("update:item", policy =>
        policy.Requirements.Add(new PermissionRequirement("update:item")));
    options.AddPolicy("delete:item", policy =>
        policy.Requirements.Add(new PermissionRequirement("delete:item")));

    // Package service scopes
    options.AddPolicy("read:packages", policy =>
        policy.Requirements.Add(new PermissionRequirement("read:packages")));
    options.AddPolicy("write:packages", policy =>
        policy.Requirements.Add(new PermissionRequirement("write:packages")));
    options.AddPolicy("update:packages", policy =>
        policy.Requirements.Add(new PermissionRequirement("update:packages")));
    options.AddPolicy("delete:packages", policy =>
        policy.Requirements.Add(new PermissionRequirement("delete:packages")));
});

builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebClient", policy =>
        policy.WithOrigins(
                builder.Configuration["AllowedOrigins"]?.Split(',') ?? Array.Empty<string>()
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
if (app.Environment.IsDevelopment()) app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("AllowWebClient");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
