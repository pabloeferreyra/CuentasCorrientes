FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["CuentasCorrientes.csproj", "./"]
RUN dotnet restore "CuentasCorrientes.csproj"

COPY . .
RUN dotnet publish "CuentasCorrientes.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CuentasCorrientes.dll"]
