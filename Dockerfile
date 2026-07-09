FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ItAssets.csproj", "./"]
RUN dotnet restore "ItAssets.csproj"
COPY . .
RUN dotnet build "ItAssets.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ItAssets.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ItAssets.dll"]
