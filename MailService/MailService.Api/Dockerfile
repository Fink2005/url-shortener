FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MailService/MailService.Api/MailService.Api.csproj", "MailService/MailService.Api/"]
COPY ["MailService/MailService.Application/MailService.Application.csproj", "MailService/MailService.Application/"]
COPY ["MailService/MailService.Domain/MailService.Domain.csproj", "MailService/MailService.Domain/"]
COPY ["MailService/MailService.Infrastructure/MailService.Infrastructure.csproj", "MailService/MailService.Infrastructure/"]
COPY ["Libs/Contracts/Contracts.csproj", "Libs/Contracts/"]

RUN dotnet restore "MailService/MailService.Api/MailService.Api.csproj"

COPY . .
WORKDIR "/src/MailService/MailService.Api"
RUN dotnet build "MailService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MailService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["MailService/MailService.Api/appsettings.json", "."]
COPY ["MailService/MailService.Api/appsettings.Development.json", "."]
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "MailService.Api.dll"]
