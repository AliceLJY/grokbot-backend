# GrokBot 后端 API

这是 GrokBot 应用的后端 API。它充当前端和 Grok AI API 之间的代理。

## 部署到 Render.com

1. 在 Render.com 上创建一个新的 Web 服务
2. 连接您的 GitHub 仓库
3. 选择适当的分支
4. 配置服务：
   - 名称：`grokbot-backend`
   - 环境：`.NET`
   - 构建命令：`dotnet publish -c Release -o out`
   - 启动命令：`dotnet out/GrokBot.Api.dll`
5. 添加下列环境变量：
   - 键：`GrokApi__ApiKey`
   - 值：您的 Grok API 密钥

## 环境变量

- `GrokApi__ApiKey`: 您的 Grok API 密钥

## 本地开发

1. 克隆仓库
2. 导航到项目目录
3. 运行 `dotnet restore`
4. 运行 `dotnet run`

API 将在 `https://localhost:7001` 和 `http://localhost:5000` 上可用。