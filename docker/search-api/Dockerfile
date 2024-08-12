# Get the dotnet Build Environment:
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source


# Copy SQL Files
COPY ../sql ./sql

# COPY Project Files
COPY ../src/SqliteFulltextSearch.Api/*.csproj ./SqliteFulltextSearch.Api/
COPY ../src/SqliteFulltextSearch.Database/*.csproj ./SqliteFulltextSearch.Database/
COPY ../src/SqliteFulltextSearch.Shared/*.csproj ./SqliteFulltextSearch.Shared/

# Restore NuGet Packages
RUN dotnet restore "SqliteFulltextSearch.Api/SqliteFulltextSearch.Api.csproj"

# COPY Projects
COPY ../src/SqliteFulltextSearch.Api/. ./src/SqliteFulltextSearch.Api/
COPY ../src/SqliteFulltextSearch.Database/. ./src/SqliteFulltextSearch.Database/
COPY ../src/SqliteFulltextSearch.Shared/. ./src/SqliteFulltextSearch.Shared/

RUN dotnet publish ./src/SqliteFulltextSearch.Api/SqliteFulltextSearch.Api.csproj -c release -o /app

# Build the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Copy Artifacts from Build:
WORKDIR /app
COPY --from=build /app ./

# Start the Kestrel Server:
ENTRYPOINT ["dotnet", "SqliteFulltextSearch.Api.dll"]