FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore
COPY ["Zlebuh.MinTacToe.API/Zlebuh.MinTacToe.API.csproj", "Zlebuh.MinTacToe.API/"]
COPY ["Zlebuh.MinTacToe/Zlebuh.MinTacToe.csproj", "Zlebuh.MinTacToe/"]
RUN dotnet restore "Zlebuh.MinTacToe.API/Zlebuh.MinTacToe.API.csproj"

# Copy all source files
COPY . .

WORKDIR "/src/Zlebuh.MinTacToe.API"
RUN dotnet publish "Zlebuh.MinTacToe.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Zlebuh.MinTacToe.API.dll"]