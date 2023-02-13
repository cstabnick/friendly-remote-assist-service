FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["FriendlyRemoteAssistantService/FriendlyRemoteAssistantService.csproj", "FriendlyRemoteAssistantService/"]
RUN dotnet restore "FriendlyRemoteAssistantService/FriendlyRemoteAssistantService.csproj"
COPY . .
WORKDIR "/src/FriendlyRemoteAssistantService"
RUN dotnet build "FriendlyRemoteAssistantService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FriendlyRemoteAssistantService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FriendlyRemoteAssistantService.dll"]
