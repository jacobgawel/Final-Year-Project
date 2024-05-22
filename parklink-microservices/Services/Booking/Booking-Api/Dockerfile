#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Services/Booking/Booking-Api/Booking-Api.csproj", "Services/Booking/Booking-Api/"]
COPY ["Services/Booking/Booking-Domain/Booking-Domain.csproj", "Services/Booking/Booking-Domain/"]
COPY ["Services/Booking/Booking-Infrastructure/Booking-Infrastructure.csproj", "Services/Booking/Booking-Infrastructure/"]
RUN dotnet restore "./Services/Booking/Booking-Api/Booking-Api.csproj"
COPY . .
WORKDIR "/src/Services/Booking/Booking-Api"
RUN dotnet build "./Booking-Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Booking-Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Copy AWS credentials
COPY ["Services/aws", "/root/.aws"]

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Booking-Api.dll"]