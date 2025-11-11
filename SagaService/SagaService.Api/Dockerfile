FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["SagaService/SagaService.Api/SagaService.Api.csproj", "SagaService/SagaService.Api/"]
COPY ["SagaService/SagaService.Domain/SagaService.Domain.csproj", "SagaService/SagaService.Domain/"]
COPY ["SagaService/SagaService.Infrastructure/SagaService.Infrastructure.csproj", "SagaService/SagaService.Infrastructure/"]
COPY ["Libs/Contracts/Contracts.csproj", "Libs/Contracts/"]

RUN dotnet restore "SagaService/SagaService.Api/SagaService.Api.csproj"

COPY . .
WORKDIR "/src/SagaService/SagaService.Api"
RUN dotnet build "SagaService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SagaService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app/publish .
RUN rm -f appsettings.Development.json
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "SagaService.Api.dll"]
