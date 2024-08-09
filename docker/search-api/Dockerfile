# Get the dotnet Build Environment:
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# COPY Project Files:
COPY ../src/ElasticsearchFulltextExample.Api/*.csproj ./ElasticsearchFulltextExample.Api/
COPY ../src/ElasticsearchFulltextExample.Database/*.csproj ./ElasticsearchFulltextExample.Database/
COPY ../src/ElasticsearchFulltextExample.Shared/*.csproj ./ElasticsearchFulltextExample.Shared/

# And restore the NuGet Packages:
RUN dotnet restore "ElasticsearchFulltextExample.Api/ElasticsearchFulltextExample.Api.csproj"

# COPY 
COPY ../src/ElasticsearchFulltextExample.Api/. ./src/ElasticsearchFulltextExample.Api/
COPY ../src/ElasticsearchFulltextExample.Database/. ./src/ElasticsearchFulltextExample.Database/
COPY ../src/ElasticsearchFulltextExample.Shared/. ./src/ElasticsearchFulltextExample.Shared/

RUN dotnet publish ./src/ElasticsearchFulltextExample.Api/ElasticsearchFulltextExample.Api.csproj -c release -o /app

# Build the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Copy Artifacts from Build:
WORKDIR /app
COPY --from=build /app ./

# Install Git
RUN apt-get -y update
RUN apt-get -y install git

# Start the Kestrel Server:
ENTRYPOINT ["dotnet", "ElasticsearchFulltextExample.Api.dll"]