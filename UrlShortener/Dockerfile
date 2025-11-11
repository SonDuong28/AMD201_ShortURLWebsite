# --- Build ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file solution and project
COPY *.sln .
COPY UrlShortener.Api/*.csproj UrlShortener.Api/

# install package NuGet
RUN dotnet restore

# Copy code
COPY . .

WORKDIR "/src/UrlShortener.Api"
RUN dotnet publish "UrlShortener.Api.csproj" -c Release -o /app/publish

# --- Final (Create final image) ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Open port 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Command to run the application
ENTRYPOINT ["dotnet", "UrlShortener.Api.dll"]