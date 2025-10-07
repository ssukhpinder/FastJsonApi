# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./src/FastJsonApi ./src/FastJsonApi
RUN dotnet publish ./src/FastJsonApi/FastJsonApi.csproj -c Release -o /app/publish

# Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT [ "dotnet", "FastJsonApi.dll" ]
