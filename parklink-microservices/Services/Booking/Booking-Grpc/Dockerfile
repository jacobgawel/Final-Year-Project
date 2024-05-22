FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Services/Booking/Booking-Grpc/Booking-Grpc.csproj", "Services/Booking/Booking-Grpc/"]
COPY ["Services/Booking/Booking-Infrastructure/Booking-Infrastructure.csproj", "Services/Booking/Booking-Infrastructure/"]
COPY ["Services/Booking/Booking-Domain/Booking-Domain.csproj", "Services/Booking/Booking-Domain/"]
RUN dotnet restore "Services/Booking/Booking-Grpc/Booking-Grpc.csproj"
COPY . .
WORKDIR "/src/Services/Booking/Booking-Grpc"
RUN dotnet build "Booking-Grpc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Booking-Grpc.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Booking-Grpc.dll"]
