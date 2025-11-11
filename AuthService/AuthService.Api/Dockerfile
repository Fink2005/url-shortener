FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["AuthService/AuthService.Api/AuthService.Api.csproj", "AuthService/AuthService.Api/"]
COPY ["AuthService/AuthService.Application/AuthService.Application.csproj", "AuthService/AuthService.Application/"]
COPY ["AuthService/AuthService.Domain/AuthService.Domain.csproj", "AuthService/AuthService.Domain/"]
COPY ["AuthService/AuthService.Infrastructure/AuthService.Infrastructure.csproj", "AuthService/AuthService.Infrastructure/"]
COPY ["Libs/Contracts/Contracts.csproj", "Libs/Contracts/"]
COPY ["Libs/Validators/Validators.csproj", "Libs/Validators/"]

RUN dotnet restore "AuthService/AuthService.Api/AuthService.Api.csproj"

COPY . .
WORKDIR "/src/AuthService/AuthService.Api"
RUN dotnet build "AuthService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "AuthService.Api.dll"]
