using Dapper;
using Quizora.SharedKernel;

namespace AITutor.Application.Services;

public sealed class UserContextBuilder(IDbConnectionFactory db) : IUserContextBuilder
{
    public async Task<string> BuildSystemPromptAsync(Guid userId, string? pageContext, CancellationToken ct)
    {
        using var conn = db.CreateConnection();

        var stats = await conn.QueryAsync<dynamic>("""
            SELECT TOP 10 q.Title, s.NormalizedScore, s.CorrectCount, s.WrongCount,
                   s.SkippedCount, s.TotalQuestions, s.CompletedAt, s.Type
            FROM exam.ExamSessions s
            JOIN quiz.Quizzes q ON q.Id = s.QuizId
            WHERE s.UserId = @UserId AND s.Status IN ('Completed','TimedOut')
            ORDER BY s.CompletedAt DESC
            """, new { UserId = userId });

        var weakAreas = await conn.QueryAsync<dynamic>("""
            SELECT TOP 5 q.Text AS QuestionText, c.Name AS CategoryName,
                   COUNT(*) AS WrongCount
            FROM exam.SessionAnswers sa
            JOIN exam.ExamSessions s ON s.Id = sa.SessionId
            JOIN quiz.Questions q ON q.Id = sa.QuestionId
            JOIN quiz.Quizzes qz ON qz.Id = s.QuizId
            JOIN quiz.Categories c ON c.Id = qz.CategoryId
            WHERE s.UserId = @UserId AND sa.IsCorrect = 0
            GROUP BY q.Text, c.Name
            ORDER BY COUNT(*) DESC
            """, new { UserId = userId });

        var statsText = string.Join("\n", stats.Select(s =>
            $"- {s.Title}: {s.NormalizedScore:F1}/100 ({s.CorrectCount}✓ {s.WrongCount}✗ {s.SkippedCount}skip) — {s.Type}"));

        var weakText = string.Join("\n", weakAreas.Select(w =>
            $"- [{w.CategoryName}] {w.QuestionText} (sbagliata {w.WrongCount} volte)"));

        return $"""
            Sei un tutor educativo personale su Quizora. Rispondi sempre in italiano.

            PROFILO STUDENTE:
            Ultimi esami:
            {(statsText.Length > 0 ? statsText : "Nessun esame ancora.")}

            Aree di debolezza:
            {(weakText.Length > 0 ? weakText : "Nessuna area debole identificata.")}

            {(pageContext != null ? $"Contesto pagina corrente: {pageContext}" : "")}

            Comportati come un tutor paziente e motivante. Adatta le tue spiegazioni al livello
            dello studente. Se vedi aree deboli, proponi esercizi mirati su quelle.
            Non rivelare mai le risposte corrette dei quiz direttamente — guida lo studente.
            """;
    }
}
