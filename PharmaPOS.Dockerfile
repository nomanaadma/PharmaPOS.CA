FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 as build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PharmaPOS.Razor/PharmaPOS.Razor.csproj", "PharmaPOS.Razor/"]
COPY ["PharmaPOS.Application/PharmaPOS.Application.csproj", "PharmaPOS.Application/"]
COPY ["PharmaPOS.Contracts/PharmaPOS.Contracts.csproj", "PharmaPOS.Contracts/"]
COPY ["PharmaPOS.Domain/PharmaPOS.Domain.csproj", "PharmaPOS.Domain/"]
COPY ["PharmaPOS.Infrastructure/PharmaPOS.Infrastructure.csproj", "PharmaPOS.Infrastructure/"]
COPY ["PharmaPOS.Shared/PharmaPOS.Shared.csproj", "PharmaPOS.Shared/"]
RUN dotnet restore "PharmaPOS.Razor/PharmaPOS.Razor.csproj"
COPY . .
WORKDIR "/src/PharmaPOS.Razor"
RUN dotnet build "PharmaPOS.Razor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PharmaPOS.Razor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "PharmaPOS.Razor.dll" ]