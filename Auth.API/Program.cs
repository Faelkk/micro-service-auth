using System.Text;
using Auth.API.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Auth.API.Services;
using System.Security.Claims;
using Auth.API.Context;
using Auth.API.Data;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<DatabaseContext>();
builder.Services.AddScoped<IDatabaseContext, DatabaseContext>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<TokenGenerator>();



builder.Services.Configure<Auth.API.Services.TokenOptions>(
    builder.Configuration.GetSection(Auth.API.Services.TokenOptions.Token)
);


var tokenOptions = builder.Configuration.GetSection(Auth.API.Services.TokenOptions.Token);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = builder.Configuration["Token:Secret"] ?? Environment.GetEnvironmentVariable("JWT_KEY");

    if (string.IsNullOrEmpty(key))
    {
        throw new Exception("JWT key is missing");
    }

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireClaim(ClaimTypes.Role, "Admin"));
    options.AddPolicy("Authenticated", policy =>
        policy.RequireAuthenticatedUser());
});

builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Micro-serice-auth-api",
        Version = "v1",
        Description = "Micro-Service-Auth API - Authentication service using JWT and RabbitMQ notifications.",
        Contact = new OpenApiContact
        {
            Name = "Rafael Achtenberg",
            Email = "achtenberg.rafa@gmail.com",
            Url = new Uri("https://github.com/Faelkk")
        }
    });
});

var port = builder.Configuration["APIPORT"];
builder.WebHost.UseUrls($"http://*:{port}");

var app = builder.Build();

app.UseCors(builder =>
{
    builder
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod();
});


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library Management API v1");
});


DatabaseSeeder.ApplyMigrationsAndSeed(app.Services);






app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
