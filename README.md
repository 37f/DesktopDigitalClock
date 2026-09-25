# 桌面电子时钟

一个轻量的 Windows 桌面悬浮时钟，使用 C# WinForms 编写，不依赖第三方库或额外字体。

## 功能

- 默认显示在主屏幕右上角，时间精确到秒
- 电子手表/七段数码管风格，显示 `HH:MM:SS`
- 无边框、默认始终置顶、半透明深色背景
- 按住鼠标左键可拖动窗口
- 右键菜单可切换 24/12 小时制、置顶、透明效果、恢复右上角、隐藏到任务栏或退出
- 12 小时制会显示 `AM` / `PM`
- EXE 内置与界面配色一致的多尺寸 Windows 图标

## 直接运行

双击 `DesktopDigitalClock.exe`。软件不会修改系统设置，也不需要管理员权限。

GitHub 仓库的 `download` 目录提供 Windows x64 单文件 EXE 和 SHA-256 校验值。

当前 EXE 没有商业代码签名证书；部分电脑首次运行时可能显示“未知发布者”或 SmartScreen 提示，可先核对交付目录中的 SHA-256 校验值。

退出方法：在时钟上单击鼠标右键，然后选择“退出”。

暂时不想显示时钟时，右键选择“隐藏到任务栏”。时钟会最小化并在 Windows 任务栏显示按钮；单击该按钮即可恢复悬浮时钟。隐藏和恢复不会重置位置或时间制式。

## 从源码构建

需要 Windows 和 .NET 6 SDK（或可编译 `net6.0-windows` 的更高版本 SDK）。

可以双击 `build-exe.cmd`，也可以在源码目录打开 PowerShell，运行：

```powershell
dotnet build .\DesktopDigitalClock.csproj -c Release
dotnet publish .\DesktopDigitalClock.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=None -p:DebugSymbols=false -o .\publish\win-x64
```

生成的文件位于：

```text
publish\win-x64\DesktopDigitalClock.exe
```

这是 Windows x64 自包含单文件版本，目标电脑无需预先安装 .NET。

## 图标源文件

应用图标位于 `assets\DesktopDigitalClock.ico`，预览图为 `assets\DesktopDigitalClock-icon.png`。图标由 `tools\IconBuilder` 中的代码确定性生成，可按需重新生成。

## 功能检查

在源码目录运行以下命令，可检查隐藏、恢复及状态保持：

```powershell
dotnet run --project .\tools\ClockLifecycleChecks\ClockLifecycleChecks.csproj -c Release
```
