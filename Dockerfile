FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 10000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "src/Desbravadores.Gestao.Api/Desbravadores.Gestao.Api.csproj"
RUN dotnet publish "src/Desbravadores.Gestao.Api/Desbravadores.Gestao.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Desbravadores.Gestao.Api.dll"]