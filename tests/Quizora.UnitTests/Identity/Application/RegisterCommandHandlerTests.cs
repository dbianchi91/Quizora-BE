using FluentAssertions;
using Identity.Application.Commands.Register;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using NSubstitute;

namespace Quizora.UnitTests.Identity.Application;

public class RegisterCommandHandlerTests
{
    private readonly IIdentityRepository _repo = Substitute.For<IIdentityRepository>();
    private readonly IJwtService _jwt = Substitute.For<IJwtService>();

    private RegisterCommandHandler CreateHandler() => new(_repo, _jwt);

    [Fact]
    public async Task Handle_WithNewEmail_ReturnsSuccess()
    {
        _repo.EmailExistsAsync("new@test.com", default).Returns(false);
        _repo.HashPasswordAsync("Password1!").Returns("hashed");
        _jwt.GenerateAccessToken(Arg.Any<User>()).Returns("access_token");
        _jwt.GenerateRefreshToken().Returns("refresh_token");
        _jwt.GetAccessTokenExpiry().Returns(DateTime.UtcNow.AddMinutes(15));

        var cmd = new RegisterCommand("new@test.com", "newuser", "Password1!");
        var result = await CreateHandler().Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access_token");
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ReturnsConflict()
    {
        _repo.EmailExistsAsync("existing@test.com", default).Returns(true);

        var cmd = new RegisterCommand("existing@test.com", "user", "Password1!");
        var result = await CreateHandler().Handle(cmd, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Conflict");
    }
}
