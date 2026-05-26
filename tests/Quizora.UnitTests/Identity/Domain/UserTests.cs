using FluentAssertions;
using Identity.Domain.Entities;
using Identity.Domain.Events;
using Identity.Domain.ValueObjects;

namespace Quizora.UnitTests.Identity.Domain;

public class UserTests
{
    private static Email ValidEmail() => Email.Create("user@test.com").Value;

    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        var result = User.Create(ValidEmail(), "testuser", "hashedpw");
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Value.Should().Be("user@test.com");
        result.Value.UserName.Should().Be("testuser");
    }

    [Fact]
    public void Create_RaisesUserRegisteredEvent()
    {
        var result = User.Create(ValidEmail(), "testuser", "hashedpw");
        result.Value.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserRegisteredEvent>();
    }

    [Fact]
    public void Create_WithEmptyUserName_Fails()
    {
        var result = User.Create(ValidEmail(), "", "hashedpw");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void RecordLogin_SetsLastLoginAt()
    {
        var user = User.Create(ValidEmail(), "u", "pw").Value;
        user.ClearDomainEvents();
        user.RecordLogin();
        user.LastLoginAt.Should().NotBeNull();
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserLoggedInEvent>();
    }

    [Fact]
    public void AddRefreshToken_ThenFindActive_ReturnsToken()
    {
        var user = User.Create(ValidEmail(), "u", "pw").Value;
        user.AddRefreshToken("tok123", DateTime.UtcNow.AddDays(7));
        var found = user.FindActiveRefreshToken("tok123");
        found.Should().NotBeNull();
        found!.IsActive.Should().BeTrue();
    }

    [Fact]
    public void RevokeRefreshToken_MakesTokenInactive()
    {
        var user = User.Create(ValidEmail(), "u", "pw").Value;
        user.AddRefreshToken("tok123", DateTime.UtcNow.AddDays(7));
        user.RevokeRefreshToken("tok123");
        var found = user.FindActiveRefreshToken("tok123");
        found.Should().BeNull();
    }

    [Fact]
    public void AssignCreatorRole_SetsIsCreatorTrue()
    {
        var user = User.Create(ValidEmail(), "u", "pw").Value;
        user.AssignCreatorRole();
        user.IsCreator.Should().BeTrue();
    }

    [Fact]
    public void RevokeCreatorRole_SetsIsCreatorFalse()
    {
        var user = User.Create(ValidEmail(), "u", "pw").Value;
        user.AssignCreatorRole();
        user.RevokeCreatorRole();
        user.IsCreator.Should().BeFalse();
    }
}
