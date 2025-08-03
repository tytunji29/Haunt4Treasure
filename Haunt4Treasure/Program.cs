using System;
using System.Text;
using CloudinaryDotNet;
using Haunt4Treasure.Helpers;
using Haunt4Treasure.Models;
using Haunt4Treasure.RegistrationFlow;
using Haunt4Treasure.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Haunt4Treasure API",
        Version = "v1"
    });

    // Enable JWT auth in Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token.\nExample: Bearer abc123token"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
var jwtKey = builder.Configuration["Jwt:Key"]; // Set this in appsettings.json or Render env
var jwtIssuer = builder.Configuration["Jwt:Issuer"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // for dev only
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// 3️⃣ Cloudinary Configuration
var account = new Account(
    builder.Configuration["appSettings:CloudinaryUsername"],
    builder.Configuration["appSettings:CloudinaryApiKey"],
    builder.Configuration["appSettings:CloudinarySecreteKey"]
);
var cloudinary = new Cloudinary(account) { Api = { Secure = true } };
builder.Services.AddSingleton(cloudinary);

builder.Services.AddScoped<IUploadFileService, UploadFileService>();
builder.Services.AddScoped<IAllRepository, AllRepository>();
builder.Services.AddScoped<IAllService, AllService>();

//builder.Services.AddDbContext<HauntDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<HauntDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient();

// Allow all CORS (for local HTML or frontend tests)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Enable Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Haunt4Treasure API v1");

    // This makes Swagger UI show at "/"
    c.RoutePrefix = string.Empty;
});

// Middleware
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication(); // ?? This MUST come before UseAuthorization
app.UseAuthorization();
app.MapControllers();

app.Run();
