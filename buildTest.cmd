@echo off

dotnet build .\src\Nova.sln /nologo
dotnet test .\src\Nova.Tests\Nova.Tests.csproj