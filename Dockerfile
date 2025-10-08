# Stage 1: Build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY ProjectManagement/*.csproj ./ProjectManagement/
RUN dotnet restore ProjectManagement/*.csproj

# Copy everything else and build
COPY ProjectManagement/. ./ProjectManagement/
WORKDIR /app/ProjectManagement
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Run the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProjectManagement.dll"]
