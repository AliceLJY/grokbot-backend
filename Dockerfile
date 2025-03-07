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

# 为环境变量创建默认值，但允许在运行时覆盖
ENV ASPNETCORE_URLS=http://+:80
ENV GrokApi__ApiKey=xai-5xUXPInpGKyQZBFH0EsBz5tjW6TJOUmByMeXhzYhw0BPpFNGw0fOoXmyJ2zA9slmZx4I8O5Uj7sT5DLP

ENTRYPOINT ["dotnet", "GrokBot.Api.dll"]