FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["poll-api.csproj", "./"]
RUN dotnet restore "./poll-api.csproj"
COPY . .
RUN dotnet build "poll-api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "poll-api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "poll-api.dll"]
