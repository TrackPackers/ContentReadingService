FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["newPostsFeed.csproj", "./"]
RUN dotnet restore "newPostsFeed.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "newPostsFeed.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "newPostsFeed.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "newPostsFeed.dll"]