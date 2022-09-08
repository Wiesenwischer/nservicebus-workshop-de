#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Microservices-Templates/Stock.Inventory/Stock.Inventory.csproj", "Microservices-Templates/Stock.Inventory/"]
RUN dotnet restore "Microservices-Templates/Stock.Inventory/Stock.Inventory.csproj"
COPY . .
WORKDIR "/src/Microservices-Templates/Stock.Inventory"
RUN dotnet build "Stock.Inventory.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Stock.Inventory.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Stock.Inventory.dll"]