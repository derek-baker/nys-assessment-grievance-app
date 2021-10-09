# https://hub.docker.com/_/microsoft-dotnet-core-sdk/
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-focal AS build

# We'll need this for for front-end stuff (which will the .NET release build will execute)
RUN apt update && apt install npm -y

# Copy local code (work dir is /dist during deploy) to the container image.
WORKDIR /app
COPY . ./

# Install production dependencies.
# Copy and restore as distinct layers.
COPY *.sln ./
RUN dotnet restore ./App.sln

# Build a release artifact (inside the container)
RUN dotnet publish -c Release -o out

# Use Microsoft's official runtime .NET image.
# https://hub.docker.com/_/microsoft-dotnet-core-aspnet/
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-focal AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Run the web service on container startup.
ENTRYPOINT ["dotnet", "App.dll"]