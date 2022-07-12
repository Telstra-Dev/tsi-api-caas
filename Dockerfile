FROM mcr.microsoft.com/dotnet/sdk:5.0-focal AS build
WORKDIR /app

COPY . /app

# RUN dotnet restore
RUN dotnet publish Telstra.Core.Api -c release -o /telstra-core --nologo

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app

COPY --from=build /telstra-core /app

HEALTHCHECK CMD curl --fail http://localhost/api/health || exit

ENV MYAPP_ports__http=80
ENV MYAPP_ports__https=0

ENTRYPOINT ["dotnet", "Telstra.Core.Api.dll"]
