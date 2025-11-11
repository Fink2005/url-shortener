FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["UrlService/UrlService.Api/UrlService.Api.csproj", "UrlService/UrlService.Api/"]
COPY ["UrlService/UrlService.Application/UrlService.Application.csproj", "UrlService/UrlService.Application/"]
COPY ["UrlService/UrlService.Domain/UrlService.Domain.csproj", "UrlService/UrlService.Domain/"]
COPY ["UrlService/UrlService.Infrastructure/UrlService.Infrastructure.csproj", "UrlService/UrlService.Infrastructure/"]
COPY ["Libs/Contracts/Contracts.csproj", "Libs/Contracts/"]
COPY ["Libs/Validators/Validators.csproj", "Libs/Validators/"]

RUN dotnet restore "UrlService/UrlService.Api/UrlService.Api.csproj"

COPY . .
WORKDIR "/src/UrlService/UrlService.Api"
RUN dotnet build "UrlService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UrlService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "UrlService.Api.dll"]
