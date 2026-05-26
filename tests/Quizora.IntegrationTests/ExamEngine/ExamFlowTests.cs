using System.Net;
using System.Net.Http.Json;
using ExamEngine.Application.DTOs;
using FluentAssertions;
using Identity.Application.Commands.Register;
using Identity.Application.DTOs;
using Quizora.IntegrationTests.Fixtures;

namespace Quizora.IntegrationTests.ExamEngine;

public sealed class ExamFlowTests(QuizoraWebFactory factory) : IClassFixture<QuizoraWebFactory>
{
    [Fact]
    public async Task StartExam_WithNonExistentQuiz_Returns404()
    {
        var client = await factory.GetClientWithMigratedDbAsync();
        var reg = new RegisterCommand("examtest@test.com", "examtest", "Password1!");
        var auth = await (await client.PostAsJsonAsync("/api/v1/auth/register", reg))
            .Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        var response = await client.PostAsJsonAsync("/api/v1/exams/start",
            new { QuizId = Guid.NewGuid(), SessionType = "Study" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetHistory_Authenticated_Returns200WithEmptyList()
    {
        var client = await factory.GetClientWithMigratedDbAsync();
        var reg = new RegisterCommand("histtest@test.com", "histtest", "Password1!");
        var auth = await (await client.PostAsJsonAsync("/api/v1/auth/register", reg))
            .Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        var response = await client.GetAsync("/api/v1/exams/history");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<IReadOnlyList<ExamHistoryDto>>();
        body.Should().BeEmpty();
    }
}
