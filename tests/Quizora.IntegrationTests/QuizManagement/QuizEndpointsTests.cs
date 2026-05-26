using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Identity.Application.Commands.Register;
using Identity.Application.DTOs;
using QuizManagement.Application.Commands.CreateQuiz;
using QuizManagement.Application.DTOs;
using Quizora.IntegrationTests.Fixtures;

namespace Quizora.IntegrationTests.QuizManagement;

public sealed class QuizEndpointsTests(QuizoraWebFactory factory) : IClassFixture<QuizoraWebFactory>
{
    private async Task<HttpClient> GetAuthenticatedClientAsync(string email, string username)
    {
        var client = await factory.GetClientWithMigratedDbAsync();
        var reg = new RegisterCommand(email, username, "Password1!");
        var resp = await client.PostAsJsonAsync("/api/v1/auth/register", reg);
        var auth = await resp.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        return client;
    }

    [Fact]
    public async Task GetQuizzes_Authenticated_Returns200()
    {
        var client = await GetAuthenticatedClientAsync("quizlist@test.com", "quizlist");

        var response = await client.GetAsync("/api/v1/quizzes");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<IReadOnlyList<QuizSummaryDto>>();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task GetQuizzes_Unauthenticated_Returns401()
    {
        var client = await factory.GetClientWithMigratedDbAsync();

        var response = await client.GetAsync("/api/v1/quizzes");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCategories_Authenticated_Returns200()
    {
        var client = await GetAuthenticatedClientAsync("catlist@test.com", "catlist");

        var response = await client.GetAsync("/api/v1/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateQuiz_WithoutCreatorRole_Returns403()
    {
        var client = await GetAuthenticatedClientAsync("notrole@test.com", "notrole");

        var cmd = new CreateQuizCommand(Guid.Empty, "T", "t", Guid.NewGuid(), null, 60, 1.0, -0.2, 0, null, false, false);
        var response = await client.PostAsJsonAsync("/api/v1/quizzes", cmd);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
