using AITutor.Application.Services;
using FluentAssertions;

namespace Quizora.UnitTests.AITutor.Application;

public class JsonResponseSanitizerTests
{
    [Fact]
    public void Extract_CleanJson_ReturnsUnchanged()
    {
        var raw = """{"priorityAreas":["Math"]}""";
        JsonResponseSanitizer.Extract(raw).Should().Be("""{"priorityAreas":["Math"]}""");
    }

    [Fact]
    public void Extract_MarkdownFence_StripsFence()
    {
        var raw = "```json\n{\"priorityAreas\":[\"Math\"]}\n```";
        JsonResponseSanitizer.Extract(raw).Should().Be("{\"priorityAreas\":[\"Math\"]}");
    }

    [Fact]
    public void Extract_TextAroundJson_ExtractsObject()
    {
        var raw = "Ecco il tuo piano:\n{\"priorityAreas\":[\"Math\"]}\nSpero ti aiuti!";
        JsonResponseSanitizer.Extract(raw).Should().Be("{\"priorityAreas\":[\"Math\"]}");
    }

    [Fact]
    public void Extract_NestedObject_KeepsOuterObject()
    {
        var raw = "text {\"a\":{\"b\":1},\"c\":2} more";
        JsonResponseSanitizer.Extract(raw).Should().Be("{\"a\":{\"b\":1},\"c\":2}");
    }

    [Fact]
    public void Extract_NoJsonObject_ReturnsTrimmedRaw()
    {
        var raw = "  nessun json qui  ";
        JsonResponseSanitizer.Extract(raw).Should().Be("nessun json qui");
    }
}
