using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Identity.Application.Commands.Login;
using Identity.Application.Commands.Register;
using Identity.Application.DTOs;
using Quizora.IntegrationTests.Fixtures;

namespace Quizora.IntegrationTests.Identity;

public sealed class AuthEndpointsTests(QuizoraWebFactory factory)
    : IClassFixture<QuizoraWebFactory>
{
    [Fact]
    public async Task Register_WithValidData_Returns200()
    {
        var client = await factory.GetClientWithMigratedDbAsync();
        var cmd = new RegisterCommand("inttest@test.com", "intuser", "Password1!");

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", cmd);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        body!.AccessToken.Should().NotBeNullOrEmpty();
        body.User.Email.Should().Be("inttest@test.com");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409()
    {
        var client = await factory.GetClientWithMigratedDbAsync();
        var cmd = new RegisterCommand("dup@test.com", "dupuser", "Password1!");
        await client.PostAsJsonAsync("/api/v1/auth/register", cmd);

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", cmd);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200()
    {
        var client = await factory.GetClientWithMigratedDbAsync();
        var registerCmd = new RegisterCommand("login@test.com", "loginuser", "Password1!");
        await client.PostAsJsonAsync("/api/v1/auth/register", registerCmd);

        var loginCmd = new LoginCommand("login@test.com", "Password1!");
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginCmd);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        body!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var client = await factory.GetClientWithMigratedDbAsync();
        var cmd = new LoginCommand("bad@test.com", "WrongPass1!");

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", cmd);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_WithValidToken_Returns200()
    {
        var client = await factory.GetClientWithMigratedDbAsync();
        var cmd = new RegisterCommand("me@test.com", "meuser", "Password1!");
        var regResp = await client.PostAsJsonAsync("/api/v1/auth/register", cmd);
        var auth = await regResp.Content.ReadFromJsonAsync<AuthResponseDto>();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        var response = await client.GetAsync("/api/v1/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
