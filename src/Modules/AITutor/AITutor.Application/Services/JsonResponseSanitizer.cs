namespace AITutor.Application.Services;

public static class JsonResponseSanitizer
{
    public static string Extract(string raw)
    {
        var text = (raw ?? string.Empty).Trim();

        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');

        if (start < 0 || end < 0 || end <= start)
            return text;

        return text.Substring(start, end - start + 1);
    }
}
