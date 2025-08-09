# Use .NET 8 ASP.NET runtime for final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use .NET 8 SDK to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the csproj and restore dependencies
COPY ["VolunteerScheduler.csproj", "./"]
RUN dotnet restore "VolunteerScheduler.csproj"

# Copy the rest of the source and build
COPY . .
RUN dotnet build -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VolunteerScheduler.dll"]
