FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Manabi.sln .
COPY src/Manabi.Shared/Manabi.Shared.csproj src/Manabi.Shared/
COPY src/Manabi.Client/Manabi.Client.csproj src/Manabi.Client/
COPY src/Manabi.Api/Manabi.Api.csproj src/Manabi.Api/
RUN dotnet restore

COPY . .

RUN dotnet publish src/Manabi.Client/Manabi.Client.csproj -c Release -o /app/client
RUN dotnet publish src/Manabi.Api/Manabi.Api.csproj -c Release -o /app/api

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/api .
COPY --from=build /app/client/wwwroot ./wwwroot
EXPOSE 8080
ENTRYPOINT ["dotnet", "Manabi.Api.dll"]
