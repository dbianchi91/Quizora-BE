using FluentAssertions;
using Identity.Domain.ValueObjects;

namespace Quizora.UnitTests.Identity.Domain;

public class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_Succeeds()
    {
        var result = Email.Create("test@example.com");
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Create_NormalizesToLowercase()
    {
        var result = Email.Create("TEST@EXAMPLE.COM");
        result.Value.Value.Should().Be("test@example.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("notanemail")]
    public void Create_WithInvalidEmail_Fails(string? value)
    {
        var result = Email.Create(value);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void TwoEmailsWithSameValue_AreEqual()
    {
        var e1 = Email.Create("a@b.com").Value;
        var e2 = Email.Create("a@b.com").Value;
        e1.Should().Be(e2);
    }
}
