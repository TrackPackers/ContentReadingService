FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ContentReaderService.csproj", "./"]
RUN dotnet restore "ContentReaderService.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ContentReaderService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ContentReaderService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ContentReaderService.dll"]