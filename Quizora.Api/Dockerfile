FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY Quizora-be.sln .
COPY Quizora-be/Quizora-be.csproj Quizora-be/
COPY src/Quizora.SharedKernel/Quizora.SharedKernel.csproj src/Quizora.SharedKernel/
COPY src/Modules/Identity/Identity.Domain/Identity.Domain.csproj src/Modules/Identity/Identity.Domain/
COPY src/Modules/Identity/Identity.Application/Identity.Application.csproj src/Modules/Identity/Identity.Application/
COPY src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure.csproj src/Modules/Identity/Identity.Infrastructure/
COPY src/Modules/QuizManagement/QuizManagement.Domain/QuizManagement.Domain.csproj src/Modules/QuizManagement/QuizManagement.Domain/
COPY src/Modules/QuizManagement/QuizManagement.Application/QuizManagement.Application.csproj src/Modules/QuizManagement/QuizManagement.Application/
COPY src/Modules/QuizManagement/QuizManagement.Infrastructure/QuizManagement.Infrastructure.csproj src/Modules/QuizManagement/QuizManagement.Infrastructure/
COPY src/Modules/ExamEngine/ExamEngine.Domain/ExamEngine.Domain.csproj src/Modules/ExamEngine/ExamEngine.Domain/
COPY src/Modules/ExamEngine/ExamEngine.Application/ExamEngine.Application.csproj src/Modules/ExamEngine/ExamEngine.Application/
COPY src/Modules/ExamEngine/ExamEngine.Infrastructure/ExamEngine.Infrastructure.csproj src/Modules/ExamEngine/ExamEngine.Infrastructure/
COPY src/Modules/AITutor/AITutor.Domain/AITutor.Domain.csproj src/Modules/AITutor/AITutor.Domain/
COPY src/Modules/AITutor/AITutor.Application/AITutor.Application.csproj src/Modules/AITutor/AITutor.Application/
COPY src/Modules/AITutor/AITutor.Infrastructure/AITutor.Infrastructure.csproj src/Modules/AITutor/AITutor.Infrastructure/
RUN dotnet restore

COPY . .
RUN dotnet publish Quizora-be/Quizora-be.csproj -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Quizora.API.dll"]
