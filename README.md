# OLO API Desktop Tool

OLO API Windows 桌面工具，支持测试 OLO API 的 GET、POST 和 PUT 请求。

## 本地安装与使用

本项目提供可直接运行的 Windows 程序，不需要安装 .NET 或其他开发环境。

1. 在 GitHub 仓库的文件列表中下载 `OloApiDesktopTool-Setup.zip`。
2. 将下载的 ZIP 文件解压到任意目录。
3. 打开解压后的文件夹，进入 `app` 子文件夹，双击其中的 `OloApiDesktopTool.exe` 文件。
4. 程序启动后即可使用。

> 如果 Windows 弹出安全提示，请确认文件来源可信后选择“运行”。建议先完整解压 ZIP，再运行 EXE 文件。

## 功能说明

- **Restaurant JSON**：输入 Domain、Key、Secret 和 Restaurant ID，获取餐厅信息。
- **Custom Request**：向自定义域名和路径发送签名请求，可选填写 JSON 请求体。
- **Restaurant XML Export**：输入相关信息，下载并保存餐厅菜单 XML。

程序默认使用 `https://ordering.api.olo.com` 作为 API 域名；每个功能页都可以单独修改 Domain。Secret 输入框会自动隐藏敏感内容。

## 从源码运行（开发者）

如果需要修改源码，请安装 .NET 8 SDK，并在项目目录运行：

```powershell
dotnet run --project .\OloApiDesktopTool.csproj
```
