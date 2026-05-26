using FluentAssertions;
using Identity.Application.Commands.Login;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;
using NSubstitute;

namespace Quizora.UnitTests.Identity.Application;

public class LoginCommandHandlerTests
{
    private readonly IIdentityRepository _repo = Substitute.For<IIdentityRepository>();
    private readonly IJwtService _jwt = Substitute.For<IJwtService>();

    private LoginCommandHandler CreateHandler() => new(_repo, _jwt);

    private static User BuildUser()
    {
        var email = Email.Create("login@test.com").Value;
        return User.Create(email, "loginuser", "hashed_pw").Value;
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsSuccess()
    {
        var user = BuildUser();
        _repo.GetByEmailAsync("login@test.com", default).Returns(user);
        _repo.CheckPasswordAsync(user, "Password1!").Returns(true);
        _jwt.GenerateAccessToken(user).Returns("token");
        _jwt.GenerateRefreshToken().Returns("refresh");
        _jwt.GetAccessTokenExpiry().Returns(DateTime.UtcNow.AddMinutes(15));

        var cmd = new LoginCommand("login@test.com", "Password1!");
        var result = await CreateHandler().Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("token");
    }

    [Fact]
    public async Task Handle_WithUnknownEmail_ReturnsUnauthorized()
    {
        _repo.GetByEmailAsync("unknown@test.com", default).Returns((User?)null);

        var cmd = new LoginCommand("unknown@test.com", "Password1!");
        var result = await CreateHandler().Handle(cmd, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Unauthorized");
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ReturnsUnauthorized()
    {
        var user = BuildUser();
        _repo.GetByEmailAsync("login@test.com", default).Returns(user);
        _repo.CheckPasswordAsync(user, "WrongPass").Returns(false);

        var cmd = new LoginCommand("login@test.com", "WrongPass");
        var result = await CreateHandler().Handle(cmd, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Unauthorized");
    }
}
