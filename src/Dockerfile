# Use Microsoft's official build .NET image.
# https://hub.docker.com/_/microsoft-dotnet-core-sdk/
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build

# We'll need this for for front-end stuff (which will the .NET app will execute)
RUN apk add --update npm

WORKDIR /app

# Copy local code (from dist during deploy) to the container image.
COPY . ./
WORKDIR /app

# Install production dependencies.
# Copy csproj and restore as distinct layers.
COPY *.sln ./
RUN dotnet restore ./App.sln


# Build a release artifact (inside the container)
RUN dotnet publish -c Release -o out


# Use Microsoft's official runtime .NET image.
# https://hub.docker.com/_/microsoft-dotnet-core-aspnet/
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Run the web service on container startup.
ENTRYPOINT ["dotnet", "App.dll"]