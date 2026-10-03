# 1. Build Phase
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Copy project file and restore dependencies
COPY ["NmazgyCloudPhotos.csproj", "./"]
RUN dotnet restore "NmazgyCloudPhotos.csproj"
# Copy the rest of the code and build the release
COPY . .
RUN dotnet publish "NmazgyCloudPhotos.csproj" -c Release -o /app/publish

# 2. Run Phase
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# Expose port 8080 for web traffic
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "NmazgyCloudPhotos.dll"]