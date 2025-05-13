using System.Text;
using Auth.API.Repository;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Auth.API.Models;
using Auth.API.Services;
using System.Security.Claims;
using Auth.API.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;




var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<DatabaseContext>();
builder.Services.AddScoped<IDatabaseContext, DatabaseContext>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<TokenGenerator>();



builder.Services.Configure<Auth.API.Services.TokenOptions>(
    builder.Configuration.GetSection(Auth.API.Services.TokenOptions.Token)
);


var tokenOptions = builder.Configuration.GetSection(Auth.API.Services.TokenOptions.Token);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenOptions.GetValue<string>("Secret")))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireClaim(ClaimTypes.Role, "Admin"));

});


builder.Services.AddOpenApi();

var port = builder.Configuration["APIPORT"];
builder.WebHost.UseUrls($"http://*:{port}");



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    db.Database.Migrate();


    var adminExists = db.Users.Any(u => u.Email == "admin@example.com");
    if (!adminExists)
    {
        var admin = new User
        {
            Name = "Admin",
            Email = "admin@example.com",
            Role = "Admin"
        };

        var hasher = new PasswordHasher<User>();
        admin.Password = hasher.HashPassword(admin, "admin123");

        db.Users.Add(admin);
        db.SaveChanges();
    }
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
