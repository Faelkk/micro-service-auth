using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Auth.API.DTO;
using Auth.API.Models;
using AuthApi.Test;
using System;
using Auth.API.Context;
using Auth.API.Repository;
using Auth.API.Services;

public class IntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    public HttpClient _clientTest;
    private readonly WebApplicationFactory<Program> factory; // Injecao do factory


    public IntegrationTest(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
        _clientTest = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<DatabaseContext>)
                );

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ContextTest>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryTestDatabase");
                });

                services.AddScoped<IDatabaseContext, DatabaseContext>();
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<INotificationService, NotificationService>();
                services.AddScoped<IPasswordService, PasswordService>();

                var sp = services.BuildServiceProvider();

                using (var scope = sp.CreateScope())
                {
                    var appContext = scope.ServiceProvider.GetRequiredService<ContextTest>();
                    appContext.Database.EnsureDeleted();
                    appContext.Database.EnsureCreated();

                    appContext.Users.AddRange(
                        new User { UserId = 1, Name = "Ana", Email = "ana@trybehotel.com", Password = "Senha1", Role = "admin" },
                        new User { UserId = 2, Name = "Beatriz", Email = "beatriz@trybehotel.com", Password = "Senha2", Role = "client" },
                        new User { UserId = 3, Name = "Laura", Email = "laura@trybehotel.com", Password = "Senha3", Role = "client" }
                    );

                    appContext.PasswordResetTokens.AddRange(
                        new PasswordResetToken { UserId = 2, Token = "test-token", ExpirationDate = DateTime.UtcNow.AddHours(1) }
                    );

                    appContext.SaveChanges();
                }
            });
        }).CreateClient();
    }

    [Trait("User", "GET /user")]
    [Fact(DisplayName = "Test GET /user returns all users")]
    public async Task TestGetUsers()
    {
        var response = await _clientTest.GetAsync("/user");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var users = JsonConvert.DeserializeObject<List<UserDto>>(json);

        Assert.NotNull(users);
        Assert.Equal(3, users.Count);
        Assert.Contains(users, u => u.name == "Ana");
        Assert.Contains(users, u => u.name == "Beatriz");
        Assert.Contains(users, u => u.name == "Laura");
    }

    [Trait("User", "POST /user")]
    [Fact(DisplayName = "Test POST /user creates new user")]
    public async Task TestPostUser()
    {
        var user = new UserInsertDto
        {
            name = "Test User",
            email = "testuser@example.com",
            password = "StrongPassword123"
        };

        var json = JsonConvert.SerializeObject(user);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _clientTest.PostAsync("/user", content);
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var getAllResponse = await _clientTest.GetAsync("/user");
        var getAllJson = await getAllResponse.Content.ReadAsStringAsync();
        var allUsers = JsonConvert.DeserializeObject<List<UserDto>>(getAllJson);

        Assert.NotNull(allUsers);
        Assert.Contains(allUsers, u => u.email == "testuser@example.com");
    }

    [Trait("User", "GET /user/{userId}")]
    [Fact(DisplayName = "Test GET /user/{userId} returns correct user")]
    public async Task TestGetUserById()
    {
        var response = await _clientTest.GetAsync("/user/1");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var user = JsonConvert.DeserializeObject<UserResponseDto>(json);

        Assert.NotNull(user);
        Assert.Equal(1, user.UserId);
        Assert.Equal("Ana", user.Name);
        Assert.Equal("ana@trybehotel.com", user.Email);
    }

    [Trait("User", "POST /login")]
    [Fact(DisplayName = "Test post login return tokens for valid user")]
    public async Task TestLoginSuccess()
    {
        var login = new LoginDto
        {
            email = "beatriz@trybehotel.com",
            password = "Senha2"
        };

        var json = JsonConvert.SerializeObject(login);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _clientTest.PostAsync("/login", content);
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("token", body);
    }

    [Trait("User", "POST /login")]
    [Fact(DisplayName = "Test POST /login fails with invalid credentials")]
    public async Task TestLoginFail()
    {
        var login = new LoginDto
        {
            email = "wrong@example.com",
            password = "wrongpassword"
        };
        var json = JsonConvert.SerializeObject(login);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _clientTest.PostAsync("/login", content);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains("Not found any user for this E-Mail", body);
    }

    [Trait("User", "PATCH /user/recover-password")]
    [Fact(DisplayName = "Test PATCH /user/recover-password initiates password recovery")]
    public async Task TestRecoverPassword()
    {
        var recoveryDto = new UserRecoveryPasswordDto
        {
            email = "beatriz@trybehotel.com"
        };
        var json = JsonConvert.SerializeObject(recoveryDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _clientTest.PatchAsync("/user/recover-password", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("If an account with that email exists, we've sent a password reset link.", body);

    }

    [Trait("User", "PATCH /user/reset-password")]
    [Fact(DisplayName = "Test PATCH /user/reset-password resets user password with valid token")]
    public async Task TestResetPasswordSuccess()
    {
        var resetDto = new UserResetPasswordDto
        {
            password = "NewSecurePassword",
        };
        var token = "test-token";
        var json = JsonConvert.SerializeObject(resetDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _clientTest.PatchAsync($"/user/reset-password?token={token}", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Password reset successfully.", body);


        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ContextTest>();
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "beatriz@trybehotel.com");
            Assert.NotNull(user);
        }
    }

    [Trait("User", "PATCH /user/reset-password")]
    [Fact(DisplayName = "Test PATCH /user/reset-password fails with invalid token")]
    public async Task TestResetPasswordInvalidToken()
    {
        var resetDto = new UserResetPasswordDto
        {
            password = "NewPassword",
        };
        var invalidToken = "invalid-token";
        var json = JsonConvert.SerializeObject(resetDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _clientTest.PatchAsync($"/user/reset-password?token={invalidToken}", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid or expired reset token.", body);
    }

    [Trait("User", "PATCH /user/reset-password")]
    [Fact(DisplayName = "Test PATCH /user/reset-password fails without token")]
    public async Task TestResetPasswordNoToken()
    {
        var resetDto = new UserResetPasswordDto
        {
            password = "NewPassword",
        };
        var json = JsonConvert.SerializeObject(resetDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _clientTest.PatchAsync("/user/reset-password", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Token de redefinição de senha é obrigatório.", body);
    }
}