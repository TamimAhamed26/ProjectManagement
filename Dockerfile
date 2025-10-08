# Use the official .NET 9 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Copy the published files
COPY ./bin/Release/net9.0/publish/ ./

# Set the entry point
ENTRYPOINT ["dotnet", "ProjectManagement.dll"]
