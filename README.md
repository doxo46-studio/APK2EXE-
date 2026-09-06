# APK2EXE 一键打包工具 / APK2EXE One-Click Packager

## 中文说明

把 Android APK 文件打包成 Windows 可执行程序（EXE）。

### 特点
- **一键打包**：图形化界面，选 APK → 点按钮 → 生成 EXE
- **体积小巧**：生成的 EXE 只含启动器和 APK，不包含运行时
- **自动下载运行时**：首次运行自动下载 Android 运行时到 C 盘，多个应用共享
- **中英文界面**：支持简体中文和 English 切换
- **自包含**：工具本身不需要安装 .NET 运行时

### 使用方法
1. 双击运行 `APK2EXE.exe`
2. 选择要打包的 APK 文件
3. 输入应用名称
4. （可选）修改运行时下载地址和启动命令
5. 点击"一键生成 EXE"
6. 完成！输出目录里会生成 `应用名.exe`

### 生成的 EXE 怎么用
- 双击即可运行
- 首次运行自动下载 Android 运行时到 `C:\ProgramData\APK2EXE\Runtime\`（约 1GB）
- 下载完成后自动启动应用
- 以后运行（包括其他用本工具打包的应用）直接启动，无需重新下载

### 运行时
默认使用 [WSABuilds](https://github.com/WSABuilds/WSABuilds)（Windows Subsystem for Android 社区版），支持 Windows 10/11。
也可以换成其他 Android 运行时的 ZIP 包（如模拟器便携版）。

### 系统要求
- Windows 10 或 Windows 11（64位）
- CPU 支持虚拟化（VT-x / AMD-V）
- 首次运行需要联网

### 项目结构
```
src/
├── APK2EXE.GUI/          # 图形化打包工具（WinForms）
│   ├── Program.cs
│   └── MainForm.cs
└── APK2EXE.Launcher/     # 启动器（运行时自动下载运行时并启动应用）
    └── Program.cs
```

### 编译
需要 .NET 8 SDK：
```bash
dotnet publish src/APK2EXE.GUI/APK2EXE.GUI.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o output
dotnet publish src/APK2EXE.Launcher/APK2EXE.Launcher.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o output/launcher
```

### 许可证
MIT License

---

## English

Package Android APK files into Windows executable (EXE).

### Features
- **One-click packaging**: GUI, select APK → click button → generate EXE
- **Small size**: Generated EXE contains only launcher and APK, no runtime
- **Auto-download runtime**: First run auto-downloads Android runtime to C: drive, shared by all apps
- **Bilingual UI**: Supports Simplified Chinese and English
- **Self-contained**: Tool itself doesn't require .NET runtime

### Usage
1. Double-click `APK2EXE.exe`
2. Select the APK file to package
3. Enter app name
4. (Optional) Modify runtime download URL and start command
5. Click "Generate EXE"
6. Done! `AppName.exe` will be generated in the output folder

### How to use the generated EXE
- Double-click to run
- First run auto-downloads Android runtime to `C:\ProgramData\APK2EXE\Runtime\` (~1GB)
- Auto-launches the app after download
- Subsequent runs (including other apps packaged with this tool) launch directly without re-download

### Runtime
Default uses [WSABuilds](https://github.com/WSABuilds/WSABuilds) (Windows Subsystem for Android community build), supports Windows 10/11.
Can also use other Android runtime ZIP packages (like portable emulators).

### System Requirements
- Windows 10 or Windows 11 (64-bit)
- CPU with virtualization support (VT-x / AMD-V)
- Internet connection required for first run

### License
MIT License
