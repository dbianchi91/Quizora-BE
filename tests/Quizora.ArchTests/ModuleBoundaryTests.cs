using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using AITutor.Application.Commands.GenerateStudyPlan;
using AITutor.Domain.Entities;
using AITutor.Infrastructure.Persistence;
using ExamEngine.Application.Commands.StartExam;
using ExamEngine.Domain.Entities;
using ExamEngine.Infrastructure.Persistence;
using Identity.Application.Commands.Register;
using Identity.Domain.Entities;
using Identity.Infrastructure.Persistence;
using QuizManagement.Application.Commands.CreateQuiz;
using QuizManagement.Domain.Entities;
using QuizManagement.Infrastructure.Persistence;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Quizora.ArchTests;

public class ModuleBoundaryTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(Quizora.SharedKernel.BaseEntity<>).Assembly,
            // Identity
            typeof(User).Assembly,
            typeof(RegisterCommand).Assembly,
            typeof(IdentityDbContext).Assembly,
            // QuizManagement
            typeof(Quiz).Assembly,
            typeof(CreateQuizCommand).Assembly,
            typeof(QuizManagementDbContext).Assembly,
            // ExamEngine
            typeof(ExamSession).Assembly,
            typeof(StartExamCommand).Assembly,
            typeof(ExamEngineDbContext).Assembly,
            // AITutor
            typeof(ChatSession).Assembly,
            typeof(GenerateStudyPlanCommand).Assembly,
            typeof(AITutorDbContext).Assembly)
        .Build();

    // ─── Identity ───────────────────────────────────────────────────────────

    [Fact]
    public void Identity_Domain_ShouldNotDependOn_Application()
    {
        Types().That().ResideInAssembly(typeof(User).Assembly)
            .Should().NotDependOnAny(
                Types().That().ResideInAssembly(typeof(RegisterCommand).Assembly))
            .Check(Architecture);
    }

    [Fact]
    public void Identity_Domain_ShouldNotDependOn_Infrastructure()
    {
        Types().That().ResideInAssembly(typeof(User).Assembly)
            .Should().NotDependOnAny(
                Types().That().ResideInAssembly(typeof(IdentityDbContext).Assembly))
            .Check(Architecture);
    }

    [Fact]
    public void Identity_Application_ShouldNotDependOn_Infrastructure()
    {
        Types().That().ResideInAssembly(typeof(RegisterCommand).Assembly)
            .Should().NotDependOnAny(
                Types().That().ResideInAssembly(typeof(IdentityDbContext).Assembly))
            .Check(Architecture);
    }

    // ─── QuizManagement ─────────────────────────────────────────────────────

    [Fact]
    public void QuizManagement_Domain_ShouldNotDependOn_Application()
    {
        Types().That().ResideInAssembly(typeof(Quiz).Assembly)
            .Should().NotDependOnAny(
                Types().That().ResideInAssembly(typeof(CreateQuizCommand).Assembly))
            .Check(Architecture);
    }

    [Fact]
    public void QuizManagement_Domain_ShouldNotDependOn_Infrastructure()
    {
        Types().That().ResideInAssembly(typeof(Quiz).Assembly)
            .Should().NotDependOnAny(
                Types().That().ResideInAssembly(typeof(QuizManagementDbContext).Assembly))
            .Check(Architecture);
    }

    [Fact]
    public void QuizManagement_Application_ShouldNotDependOn_Infrastructure()
    {
        Types().That().ResideInAssembly(typeof(CreateQuizCommand).Assembly)
            .Should().NotDependOnAny(
                Types().That().ResideInAssembly(typeof(QuizManagementDbContext).Assembly))
            .Check(Architecture);
    }

    // ─── ExamEngine ─────────────────────────────────────────────────────────

    [Fact]
    public void ExamEngine_Domain_ShouldNotDependOn_Application()
    {
        Types().That().ResideInAssembly(typeof(ExamSession).Assembly)
            .Should().NotDependOnAny(
                Types().That().ResideInAssembly(typeof(StartExamCommand).Assembly))
            .Check(Architecture);
    }

    [Fact]
    public void ExamEngine_Domain_ShouldNotDependOn_Infrastructure()
    {
        Types().That().ResideInAssembly(typeof(ExamSession).Assembly)
            .Should().NotDependOnAny(
                Types().That().ResideInAssembly(typeof(ExamEngineDbContext).Assembly))
            .Check(Architecture);
    }

    [Fact]
    public void ExamEngine_Application_ShouldNotDependOn_Infrastructure()
    {
        Types().That().ResideInAssembly(typeof(StartExamCommand).Assembly)
            .Should().NotDependOnAny(
                Types().That().ResideInAssembly(typeof(ExamEngineDbContext).Assembly))
            .Check(Architecture);
    }

    // ─── AITutor ────────────────────────────────────────────────────────────

    [Fact]
    public void AITutor_Domain_ShouldNotDependOn_Application()
    {
        Types().That().ResideInAssembly(typeof(ChatSession).Assembly)
            .Should().NotDependOnAny(
                Types().That().ResideInAssembly(typeof(GenerateStudyPlanCommand).Assembly))
            .Check(Architecture);
    }

    [Fact]
    public void AITutor_Domain_ShouldNotDependOn_Infrastructure()
    {
        Types().That().ResideInAssembly(typeof(ChatSession).Assembly)
            .Should().NotDependOnAny(
                Types().That().ResideInAssembly(typeof(AITutorDbContext).Assembly))
            .Check(Architecture);
    }

    [Fact]
    public void AITutor_Application_ShouldNotDependOn_Infrastructure()
    {
        Types().That().ResideInAssembly(typeof(GenerateStudyPlanCommand).Assembly)
            .Should().NotDependOnAny(
                Types().That().ResideInAssembly(typeof(AITutorDbContext).Assembly))
            .Check(Architecture);
    }

    // ─── Cross-module isolation ──────────────────────────────────────────────

    [Fact]
    public void QuizManagement_ShouldNotDependOn_ExamEngine()
    {
        var quizAssemblies = new[]
        {
            typeof(Quiz).Assembly,
            typeof(CreateQuizCommand).Assembly,
            typeof(QuizManagementDbContext).Assembly
        };
        var examAssemblies = new[]
        {
            typeof(ExamSession).Assembly,
            typeof(StartExamCommand).Assembly,
            typeof(ExamEngineDbContext).Assembly
        };

        foreach (var quizAsm in quizAssemblies)
        {
            foreach (var examAsm in examAssemblies)
            {
                Types().That().ResideInAssembly(quizAsm)
                    .Should().NotDependOnAny(Types().That().ResideInAssembly(examAsm))
                    .Check(Architecture);
            }
        }
    }

    [Fact]
    public void ExamEngine_ShouldNotDependOn_QuizManagement_Infrastructure()
    {
        Types().That().ResideInAssembly(typeof(ExamSession).Assembly)
            .Should().NotDependOnAny(
                Types().That().ResideInAssembly(typeof(QuizManagementDbContext).Assembly))
            .Check(Architecture);
    }
}
