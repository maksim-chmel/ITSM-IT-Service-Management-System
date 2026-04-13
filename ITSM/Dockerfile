FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ITSM/ITSM.csproj", "ITSM/"]
RUN dotnet restore "ITSM/ITSM.csproj"

COPY . .
WORKDIR /src/ITSM
RUN dotnet publish "ITSM.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /src

ENV PATH="${PATH}:/root/.dotnet/tools"
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /src /src
RUN dotnet tool install --global dotnet-ef --version 9.0.0
RUN chmod +x /src/ITSM/scripts/docker-entrypoint.sh

ENTRYPOINT ["/src/ITSM/scripts/docker-entrypoint.sh"]
