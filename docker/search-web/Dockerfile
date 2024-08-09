# Get the dotnet Build Environment:
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# COPY Project Files:
COPY ../src/ElasticsearchFulltextExample.Web.Client/*.csproj ./ElasticsearchFulltextExample.Web.Client/
COPY ../src/ElasticsearchFulltextExample.Web.Server/*.csproj ./ElasticsearchFulltextExample.Web.Server/
COPY ../src/ElasticsearchFulltextExample.Shared/*.csproj ./ElasticsearchFulltextExample.Shared/

# And restore the NuGet Packages:
RUN dotnet restore "ElasticsearchFulltextExample.Web.Server/ElasticsearchFulltextExample.Web.Server.csproj"

# COPY 
COPY ../src/ElasticsearchFulltextExample.Web.Client/. ./src/ElasticsearchFulltextExample.Web.Client/
COPY ../src/ElasticsearchFulltextExample.Web.Server/. ./src/ElasticsearchFulltextExample.Web.Server/
COPY ../src/ElasticsearchFulltextExample.Shared/. ./src/ElasticsearchFulltextExample.Shared/

RUN dotnet publish ./src/ElasticsearchFulltextExample.Web.Server/ElasticsearchFulltextExample.Web.Server.csproj -c Release -o /app

# Build the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Copy Artifacts from Build:
WORKDIR /app
COPY --from=build /app ./

# Start the Kestrel Server:
ENTRYPOINT ["dotnet", "ElasticsearchFulltextExample.Web.Server.dll"]