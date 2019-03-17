FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /app

COPY Sawmill/. ./sawmill/
WORKDIR /app/sawmill
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/runtime:2.2 AS runtime
#RUN touch /var/log/access.log
RUN touch /tmp/access.log
WORKDIR /app
COPY --from=build /app/sawmill/out ./
ENTRYPOINT ["dotnet", "Sawmill.dll"]
