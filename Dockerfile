FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
EXPOSE 443

# Archived image: no credentials are baked into the image.
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "GrokBot.Api.dll"]
