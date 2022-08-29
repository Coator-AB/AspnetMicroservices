#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["services/ordering/Ordering.API/Ordering.API.csproj", "services/ordering/Ordering.API/"]
COPY ["services/ordering/Ordering.Application/Ordering.Application.csproj", "services/ordering/Ordering.Application/"]
COPY ["services/ordering/Ordering.Domain/Ordering.Domain.csproj", "services/ordering/Ordering.Domain/"]
COPY ["services/ordering/Ordering.Infrastructure/Ordering.Infrastructure.csproj", "services/ordering/Ordering.Infrastructure/"]
RUN dotnet restore "services/ordering/Ordering.API/Ordering.API.csproj"
COPY . .
WORKDIR "/src/services/ordering/Ordering.API"
RUN dotnet build "Ordering.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ordering.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ordering.API.dll"]
