FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Services/Reviews/Reviews-Api/Reviews-Api.csproj", "Services/Reviews/Reviews-Api/"]
COPY ["Services/Reviews/Reviews-Infrastructure/Reviews-Infrastructure.csproj", "Services/Reviews/Reviews-Infrastructure/"]
COPY ["Services/Reviews/Reviews-Domain/Reviews-Domain.csproj", "Services/Reviews/Reviews-Domain/"]
RUN dotnet restore "Services/Reviews/Reviews-Api/Reviews-Api.csproj"
COPY . .
WORKDIR "/src/Services/Reviews/Reviews-Api"
RUN dotnet build "Reviews-Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Reviews-Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Reviews-Api.dll"]
