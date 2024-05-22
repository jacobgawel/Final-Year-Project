FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/IdentityServer/Identity-Server/Identity-Server.csproj", "Services/IdentityServer/Identity-Server/"]
RUN dotnet restore "Services/IdentityServer/Identity-Server/Identity-Server.csproj"
COPY . .
WORKDIR "/src/Services/IdentityServer/Identity-Server"
RUN dotnet build "Identity-Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Identity-Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Identity-Server.dll"]
