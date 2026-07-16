> **[Archived / offline]** This repository is retained as a historical artifact. The hosted AI proxy is being retired, the chat endpoint now returns HTTP 410, and no API credential belongs in this repository or image.

# GrokBot 后端 API

这是 GrokBot 早期演示的后端源码。它曾充当前端和 Grok AI API 之间的代理，现在不再提供在线 AI 转发能力。

## 当前状态

- `POST /api/grok/chat` 固定返回 HTTP 410，不会调用任何模型供应商。
- `GET /health` 和 `GET /api/grok/health` 只报告归档状态。
- Docker 镜像和 tracked 配置不再包含 API key。
- 不要把本仓重新部署为公开、无认证的模型代理；需要类似能力时，应另建带认证、限流、费用上限和最小 CORS allowlist 的服务。

## 环境变量

- `ASPNETCORE_URLS`: 应用程序监听的URL（默认为http://+:80）
- `Cors__AllowedOrigins`: 逗号分隔的允许来源；默认只允许本地开发来源。

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

归档状态 API 将在 `http://localhost:8080` 上可用；聊天端点不会转发请求。
