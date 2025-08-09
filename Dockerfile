# --- Base Image ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# --- Build Stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install EF CLI
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy and restore
COPY ["VolunteerScheduler.csproj", "./"]
RUN dotnet restore "VolunteerScheduler.csproj"

# Copy everything and build
COPY . .
RUN dotnet build -c Release -o /app/build

# --- Publish Stage ---
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# --- Final Stage ---
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VolunteerScheduler.dll"]
