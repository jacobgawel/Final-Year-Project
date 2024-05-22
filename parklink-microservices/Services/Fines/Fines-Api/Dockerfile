FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Services/Fines/Fines-Api/Fines-Api.csproj", "Services/Fines/Fines-Api/"]
COPY ["Services/Fines/Fines-Infrastructure/Fines-Infrastructure.csproj", "Services/Fines/Fines-Infrastructure/"]
COPY ["Services/Fines/Fines-Domain/Fines-Domain.csproj", "Services/Fines/Fines-Domain/"]
RUN dotnet restore "Services/Fines/Fines-Api/Fines-Api.csproj"
COPY . .
WORKDIR "/src/Services/Fines/Fines-Api"
RUN dotnet build "Fines-Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Fines-Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fines-Api.dll"]
