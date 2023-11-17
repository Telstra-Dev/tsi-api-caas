
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish WCA.Consumer.API -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0-jammy
WORKDIR /App
COPY --from=build-env /App/out .
ENV PORT=8080
EXPOSE $PORT
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ./WCA.Consumer.API --urls "http://0.0.0.0:$PORT"