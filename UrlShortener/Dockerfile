# --- Giai đoạn 1: Build (Xây dựng) ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file solution và project
COPY *.sln .
# SỬA Ở ĐÂY: Bỏ 'src/' ở đường dẫn copy
COPY UrlShortener.Api/*.csproj UrlShortener.Api/

# Chạy restore để tải các package NuGet
RUN dotnet restore

# Copy toàn bộ source code
COPY . .

# SỬA Ở ĐÂY: Sửa WORKDIR và đường dẫn publish
WORKDIR "/src/UrlShortener.Api"
RUN dotnet publish "UrlShortener.Api.csproj" -c Release -o /app/publish

# --- Giai đoạn 2: Final (Tạo image cuối cùng) ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Mở port 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Lệnh để chạy ứng dụng
ENTRYPOINT ["dotnet", "UrlShortener.Api.dll"]