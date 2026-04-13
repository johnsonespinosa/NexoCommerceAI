FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/NexoCommerceAI.API/NexoCommerceAI.API.csproj", "src/NexoCommerceAI.API/"]
COPY ["src/NexoCommerceAI.Application/NexoCommerceAI.Application.csproj", "src/NexoCommerceAI.Application/"]
COPY ["src/NexoCommerceAI.Domain/NexoCommerceAI.Domain.csproj", "src/NexoCommerceAI.Domain/"]
COPY ["src/NexoCommerceAI.Infrastructure/NexoCommerceAI.Infrastructure.csproj", "src/NexoCommerceAI.Infrastructure/"]

RUN dotnet restore "src/NexoCommerceAI.API/NexoCommerceAI.API.csproj"

COPY . .
WORKDIR "/src/src/NexoCommerceAI.API"
RUN dotnet publish "NexoCommerceAI.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "NexoCommerceAI.API.dll"]
