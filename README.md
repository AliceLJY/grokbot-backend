# GrokBot 后端 API

这是 GrokBot 应用的后端 API。它充当前端和 Grok AI API 之间的代理。

## 部署到 Render.com (使用Docker)

1. 在 Render.com 上创建一个新的 Web 服务
2. 连接您的 GitHub 仓库
3. 选择适当的分支
4. 配置服务：
   - 名称：`grokbot-backend`
   - **环境：`Docker`**
   - 不需要额外的构建命令（Dockerfile已包含所有需要的构建步骤）
5. 添加下列环境变量（如需覆盖Dockerfile中的默认值）：
   - 键：`GrokApi__ApiKey`
   - 值：您的 Grok API 密钥

## 环境变量

- `GrokApi__ApiKey`: 您的 Grok API 密钥（已设置在Dockerfile中，但可以通过环境变量覆盖）
- `ASPNETCORE_URLS`: 应用程序监听的URL（默认为http://+:80）

## 本地开发

### 使用 .NET CLI

1. 克隆仓库
2. 导航到项目目录
3. 运行 `dotnet restore`
4. 运行 `dotnet run`

### 使用 Docker

1. 克隆仓库
2. 导航到项目目录
3. 构建Docker镜像：`docker build -t grokbot-backend .`
4. 运行容器：`docker run -p 8080:80 grokbot-backend`

API 将在 `http://localhost:8080` 上可用。