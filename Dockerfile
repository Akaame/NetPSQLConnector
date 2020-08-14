FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY NPSQLConnector/NPSQLConnector.csproj ./NPSQLConnector/
COPY NPSQLConnector.Sample/NPSQLConnector.Sample.csproj ./NPSQLConnector.Sample/
RUN dotnet restore NPSQLConnector.Sample/NPSQLConnector.Sample.csproj

# Copy everything else and build
COPY NPSQLConnector/ ./NPSQLConnector
COPY NPSQLConnector.Sample/ ./NPSQLConnector.Sample

RUN find -type d -name bin -prune -exec rm -rf {} \; && find -type d -name obj -prune -exec rm -rf {} \;
RUN dotnet publish NPSQLConnector.Sample/NPSQLConnector.Sample.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .
COPY ./start.sh .
COPY ./wait-for-it.sh .

ENTRYPOINT ["./start.sh"]