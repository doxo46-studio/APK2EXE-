using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace APK2EXE.GUI
{
    public enum UILanguage { Chinese, English }

    public class MainForm : Form
    {
        private TextBox txtApkPath;
        private TextBox txtAppName;
        private TextBox txtRuntimeUrl;
        private TextBox txtStartCommand;
        private TextBox txtOutputDir;
        private TextBox txtLog;
        private Button btnGenerate;
        private Button btnBrowseApk;
        private Button btnBrowseOutput;
        private ComboBox cmbLanguage;
                private Label lblUrlHint;

        private UILanguage _lang = UILanguage.Chinese;

        private static readonly string DotNetPath = @"C:\Users\Administrator\dotnet\dotnet.exe";
        private const string Magic = "AP2E";

        // 多语言文本字典
        private readonly Dictionary<string, string> _texts = new Dictionary<string, string>();

        public MainForm()
        {
            LoadTexts();
            InitializeComponent();
            ApplyLanguage();
        }

        private void LoadTexts()
        {
            // 中文（默认）
            _texts["zh_Title"] = "APK2EXE 一键打包工具";
            _texts["zh_ApkLabel"] = "APK 文件：";
            _texts["zh_Browse"] = "浏览...";
            _texts["zh_AppName"] = "应用名称：";
            _texts["zh_RuntimeUrl"] = "运行时下载地址：";
            _texts["zh_RuntimeHint"] = "（首次运行时自动下载安装到 C 盘，所有应用共享；可换其他 Android 运行时 ZIP）";
            _texts["zh_StartCmd"] = "启动命令：";
            _texts["zh_OutputDir"] = "输出目录：";
            _texts["zh_Generate"] = "一键生成 EXE";
            _texts["zh_Generating"] = "生成中...";
            _texts["zh_Log"] = "日志：";
            _texts["zh_Language"] = "语言：";
            _texts["zh_Ready"] = "APK2EXE 一键打包工具已就绪。";
            _texts["zh_Desc1"] = "说明：生成的 EXE 体积很小（只含 APK），首次运行时自动下载 Android 运行时到 C 盘。";
            _texts["zh_Desc2"] = "运行时下载地址默认使用 WSABuilds（WSA 社区版），你也可以换成其他 Android 运行时 ZIP。";
            _texts["zh_ErrNoApk"] = "请选择有效的 APK 文件！";
            _texts["zh_ErrNoName"] = "请输入应用名称！";
            _texts["zh_ErrNoOutput"] = "请选择输出目录！";
            _texts["zh_ErrTitle"] = "错误";
            _texts["zh_Success"] = "生成成功！\n\n输出文件：";
            _texts["zh_SuccessTitle"] = "完成";
            _texts["zh_FilterApk"] = "APK 文件 (*.apk)|*.apk|所有文件 (*.*)|*.*";
            _texts["zh_FilterTitle"] = "选择 APK 文件";
            _texts["zh_OutputTitle"] = "选择输出目录";

            // 英文
            _texts["en_Title"] = "APK2EXE One-Click Packager";
            _texts["en_ApkLabel"] = "APK File:";
            _texts["en_Browse"] = "Browse...";
            _texts["en_AppName"] = "App Name:";
            _texts["en_RuntimeUrl"] = "Runtime Download URL:";
            _texts["en_RuntimeHint"] = "(Auto-downloads to C: drive on first run, shared by all apps; can use other Android runtime ZIPs)";
            _texts["en_StartCmd"] = "Start Command:";
            _texts["en_OutputDir"] = "Output Folder:";
            _texts["en_Generate"] = "Generate EXE";
            _texts["en_Generating"] = "Generating...";
            _texts["en_Log"] = "Log:";
            _texts["en_Language"] = "Language:";
            _texts["en_Ready"] = "APK2EXE One-Click Packager is ready.";
            _texts["en_Desc1"] = "Note: Generated EXE is small (APK only). Android runtime auto-downloads to C: drive on first run.";
            _texts["en_Desc2"] = "Default runtime URL uses WSABuilds (WSA community build). You can replace with other Android runtime ZIPs.";
            _texts["en_ErrNoApk"] = "Please select a valid APK file!";
            _texts["en_ErrNoName"] = "Please enter an app name!";
            _texts["en_ErrNoOutput"] = "Please select an output folder!";
            _texts["en_ErrTitle"] = "Error";
            _texts["en_Success"] = "Generated successfully!\n\nOutput file: ";
            _texts["en_SuccessTitle"] = "Done";
            _texts["en_FilterApk"] = "APK Files (*.apk)|*.apk|All Files (*.*)|*.*";
            _texts["en_FilterTitle"] = "Select APK File";
            _texts["en_OutputTitle"] = "Select Output Folder";
        }

        private string T(string key)
        {
            string prefix = _lang == UILanguage.Chinese ? "zh" : "en";
            string fullKey = prefix + "_" + key;
            return _texts.ContainsKey(fullKey) ? _texts[fullKey] : key;
        }

        private void InitializeComponent()
        {
            this.Text = "APK2EXE";
            this.Size = new Size(740, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Microsoft YaHei UI", 9F);
            this.MinimumSize = new Size(700, 640);

            int y = 50;
            int labelW = 120;
            int inputX = 140;
            int inputW = 455;
            int btnW = 80;
            int rowH = 30;

            // --- 语言选择（右上角）---
            var lblLang = new Label { Name = "lblLang", Left = 540, Top = 12, Width = 55, TextAlign = ContentAlignment.MiddleRight };
            cmbLanguage = new ComboBox
            {
                Left = 600,
                Top = 10,
                Width = 110,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbLanguage.Items.AddRange(new object[] { "简体中文", "English" });
            cmbLanguage.SelectedIndex = 0;
            cmbLanguage.SelectedIndexChanged += (s, e) =>
            {
                _lang = cmbLanguage.SelectedIndex == 0 ? UILanguage.Chinese : UILanguage.English;
                ApplyLanguage();
            };
            
            this.Controls.AddRange(new Control[] { lblLang, cmbLanguage });

            // --- APK 文件 ---
            var lblApk = new Label { Name = "lblApk", Left = 15, Top = y + 5, Width = labelW, TextAlign = ContentAlignment.MiddleRight };
            txtApkPath = new TextBox { Left = inputX, Top = y, Width = inputW, ReadOnly = true };
            btnBrowseApk = new Button { Name = "btnBrowseApk", Left = inputX + inputW + 5, Top = y - 1, Width = btnW };
            btnBrowseApk.Click += (s, e) => BrowseApk();
            this.Controls.AddRange(new Control[] { lblApk, txtApkPath, btnBrowseApk });
            y += rowH + 8;

            // --- 应用名 ---
            var lblName = new Label { Name = "lblName", Left = 15, Top = y + 5, Width = labelW, TextAlign = ContentAlignment.MiddleRight };
            txtAppName = new TextBox { Left = inputX, Top = y, Width = inputW + btnW + 5 };
            this.Controls.AddRange(new Control[] { lblName, txtAppName });
            y += rowH + 8;

            // --- 运行时下载 URL ---
            var lblUrl = new Label { Name = "lblUrl", Left = 15, Top = y + 5, Width = labelW, TextAlign = ContentAlignment.MiddleRight };
            txtRuntimeUrl = new TextBox { Left = inputX, Top = y, Width = inputW + btnW + 5 };
            txtRuntimeUrl.Text = "https://github.com/WSABuilds/WSABuilds/releases/download/v2407.40000.4.0_v2/WSA_2407.40000.4.0_x64_Release-Nightly.zip";
            lblUrlHint = new Label
            {
                Name = "lblUrlHint",
                Left = inputX, Top = y + 22, Width = inputW + btnW + 5,
                Font = new Font("Microsoft YaHei UI", 8F),
                ForeColor = Color.Gray
            };
            this.Controls.AddRange(new Control[] { lblUrl, txtRuntimeUrl, lblUrlHint });
            y += 50;

            // --- 启动命令 ---
            var lblCmd = new Label { Name = "lblCmd", Left = 15, Top = y + 5, Width = labelW, TextAlign = ContentAlignment.TopRight };
            txtStartCommand = new TextBox
            {
                Left = inputX,
                Top = y,
                Width = inputW + btnW + 5,
                Height = 110,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.AddRange(new Control[] { lblCmd, txtStartCommand });
            y += 120;

            // --- 输出目录 ---
            var lblOut = new Label { Name = "lblOut", Left = 15, Top = y + 5, Width = labelW, TextAlign = ContentAlignment.MiddleRight };
            txtOutputDir = new TextBox { Left = inputX, Top = y, Width = inputW };
            txtOutputDir.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            btnBrowseOutput = new Button { Name = "btnBrowseOutput", Left = inputX + inputW + 5, Top = y - 1, Width = btnW };
            btnBrowseOutput.Click += (s, e) => BrowseOutput();
            this.Controls.AddRange(new Control[] { lblOut, txtOutputDir, btnBrowseOutput });
            y += rowH + 15;

            // --- 生成按钮 ---
            btnGenerate = new Button
            {
                Name = "btnGenerate",
                Left = inputX,
                Top = y,
                Width = 200,
                Height = 40,
                Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold),
                BackColor = Color.FromArgb(88, 101, 242),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnGenerate.Click += async (s, e) => await Generate();
            this.Controls.Add(btnGenerate);
            y += 55;

            // --- 日志 ---
            var lblLog = new Label { Name = "lblLog", Left = 15, Top = y, Width = labelW };
            txtLog = new TextBox
            {
                Left = 15,
                Top = y + 22,
                Width = this.ClientSize.Width - 30,
                Height = this.ClientSize.Height - y - 40,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9F),
                BackColor = Color.FromArgb(30, 31, 38),
                ForeColor = Color.FromArgb(200, 200, 210),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.AddRange(new Control[] { lblLog, txtLog });
        }

        private void ApplyLanguage()
        {
            this.Text = T("Title");

            // 更新所有标签和按钮
            SetControlText("lblLang", T("Language"));
            
            SetControlText("lblApk", T("ApkLabel"));
            SetControlText("btnBrowseApk", T("Browse"));
            SetControlText("lblName", T("AppName"));
            SetControlText("lblUrl", T("RuntimeUrl"));
            SetControlText("lblUrlHint", T("RuntimeHint"));
            SetControlText("lblCmd", T("StartCmd"));
            SetControlText("lblOut", T("OutputDir"));
            SetControlText("btnBrowseOutput", T("Browse"));
            SetControlText("btnGenerate", T("Generate"));
            SetControlText("lblLog", T("Log"));

            // 更新启动命令默认内容（如果用户没改过）
            UpdateDefaultStartCommand();

            // 重新写日志头部
            txtLog.Clear();
            Log(T("Ready"));
            Log(T("Desc1"));
            Log(T("Desc2"));
        }

        private void SetControlText(string name, string text)
        {
            foreach (Control c in this.Controls)
            {
                if (c.Name == name)
                {
                    c.Text = text;
                    return;
                }
            }
        }

        private void UpdateDefaultStartCommand()
        {
            // 只有当内容是默认模板时才更新，避免覆盖用户修改
            string zhDefault = GetDefaultStartCommand(UILanguage.Chinese);
            string enDefault = GetDefaultStartCommand(UILanguage.English);
            string current = txtStartCommand.Text;

            if (string.IsNullOrWhiteSpace(current) || current == zhDefault || current == enDefault)
            {
                txtStartCommand.Text = GetDefaultStartCommand(_lang);
            }
        }

        private string GetDefaultStartCommand(UILanguage lang)
        {
            if (lang == UILanguage.English)
            {
                return "@echo off\r\n" +
                       "chcp 65001 >nul\r\n" +
                       "echo ========================================\r\n" +
                       "echo   Starting Android Runtime...\r\n" +
                       "echo ========================================\r\n" +
                       "echo.\r\n" +
                       "rem Runtime installed at C:\\ProgramData\\APK2EXE\\Runtime\\\r\n" +
                       "rem APK file in current folder: %~dp0app.apk\r\n" +
                       "rem Write commands to start runtime, install APK, launch app here\r\n" +
                       "echo.\r\n" +
                       "echo Runtime folder: C:\\ProgramData\\APK2EXE\\Runtime\r\n" +
                       "echo APK file: %~dp0app.apk\r\n" +
                       "echo.\r\n" +
                       "pause";
            }
            else
            {
                return "@echo off\r\n" +
                       "chcp 65001 >nul\r\n" +
                       "echo ========================================\r\n" +
                       "echo   正在启动 Android 运行时...\r\n" +
                       "echo ========================================\r\n" +
                       "echo.\r\n" +
                       "rem 运行时安装在 C:\\ProgramData\\APK2EXE\\Runtime\\\r\n" +
                       "rem APK 文件在当前目录：%~dp0app.apk\r\n" +
                       "rem 在这里写启动运行时、安装APK、启动应用的命令\r\n" +
                       "echo.\r\n" +
                       "echo 运行时目录：C:\\ProgramData\\APK2EXE\\Runtime\r\n" +
                       "echo APK 文件：%~dp0app.apk\r\n" +
                       "echo.\r\n" +
                       "pause";
            }
        }

        private void Log(string msg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(Log), msg);
                return;
            }
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\r\n");
            txtLog.ScrollToCaret();
        }

        private void BrowseApk()
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = T("FilterApk");
                dlg.Title = T("FilterTitle");
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtApkPath.Text = dlg.FileName;
                    if (string.IsNullOrWhiteSpace(txtAppName.Text))
                    {
                        txtAppName.Text = Path.GetFileNameWithoutExtension(dlg.FileName);
                    }
                }
            }
        }

        private void BrowseOutput()
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = T("OutputTitle");
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtOutputDir.Text = dlg.SelectedPath;
                }
            }
        }

        private async Task Generate()
        {
            string apkPath = txtApkPath.Text.Trim();
            string appName = txtAppName.Text.Trim();
            string runtimeUrl = txtRuntimeUrl.Text.Trim();
            string startCmd = txtStartCommand.Text;
            string outputDir = txtOutputDir.Text.Trim();

            if (string.IsNullOrEmpty(apkPath) || !File.Exists(apkPath))
            {
                MessageBox.Show(T("ErrNoApk"), T("ErrTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(appName))
            {
                MessageBox.Show(T("ErrNoName"), T("ErrTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(outputDir))
            {
                MessageBox.Show(T("ErrNoOutput"), T("ErrTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnGenerate.Enabled = false;
            btnGenerate.Text = T("Generating");

            try
            {
                await Task.Run(() => DoGenerate(apkPath, appName, runtimeUrl, startCmd, outputDir));
                MessageBox.Show(T("Success") + Path.Combine(outputDir, appName + ".exe"),
                    T("SuccessTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
                Log(ex.StackTrace);
                MessageBox.Show($"{T("ErrTitle")}: {ex.Message}", T("ErrTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnGenerate.Enabled = true;
                btnGenerate.Text = T("Generate");
            }
        }

        private void DoGenerate(string apkPath, string appName, string runtimeUrl, string startCmd, string outputDir)
        {
            string tempRoot = Path.Combine(Path.GetTempPath(), "APK2EXE_build_" + Guid.NewGuid().ToString("N"));
            string workDir = Path.Combine(tempRoot, "payload");

            try
            {
                Log("=== Start packaging ===");
                Log($"App: {appName}");
                Log($"APK: {apkPath}");
                Log($"Runtime URL: {runtimeUrl}");

                Log("Creating work folder...");
                Directory.CreateDirectory(workDir);

                Log("Copying APK...");
                string apkDest = Path.Combine(workDir, "app.apk");
                File.Copy(apkPath, apkDest, true);

                Log("Generating start script...");
                string batPath = Path.Combine(workDir, "start.bat");
                File.WriteAllText(batPath, startCmd, Encoding.Default);

                File.WriteAllText(Path.Combine(workDir, "appname.txt"), appName, Encoding.UTF8);
                File.WriteAllText(Path.Combine(workDir, "runtime_url.txt"), runtimeUrl, Encoding.UTF8);

                Log("Compressing...");
                string zipPath = Path.Combine(tempRoot, "payload.zip");
                if (File.Exists(zipPath)) File.Delete(zipPath);
                ZipFile.CreateFromDirectory(workDir, zipPath, CompressionLevel.Optimal, false);
                long zipSize = new FileInfo(zipPath).Length;
                Log($"Compressed: {zipSize / 1024.0:F1} KB (APK only, no runtime)");

                Log("Checking launcher...");
                string launcherExe = EnsureLauncherBuilt();

                Log("Merging final EXE...");
                Directory.CreateDirectory(outputDir);
                string outputExe = Path.Combine(outputDir, SanitizeFileName(appName) + ".exe");
                if (File.Exists(outputExe)) File.Delete(outputExe);

                byte[] launcherData = File.ReadAllBytes(launcherExe);
                byte[] zipData = File.ReadAllBytes(zipPath);

                using (var fs = new FileStream(outputExe, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(launcherData, 0, launcherData.Length);
                    long zipOffset = launcherData.Length;
                    fs.Write(zipData, 0, zipData.Length);
                    fs.Write(Encoding.ASCII.GetBytes(Magic), 0, 4);
                    fs.Write(BitConverter.GetBytes(zipOffset), 0, 8);
                }

                long finalSize = new FileInfo(outputExe).Length;
                Log("=== Packaging complete ===");
                Log($"Output: {outputExe}");
                Log($"Size: {finalSize / 1024.0 / 1024.0:F1} MB (launcher + APK, no runtime)");
                Log("Usage: Double-click the EXE to run. First run auto-downloads Android runtime to C: drive (~1GB), then launches directly.");
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempRoot))
                        Directory.Delete(tempRoot, true);
                }
                catch { }
            }
        }

        private string EnsureLauncherBuilt()
        {
            string toolDir = AppDomain.CurrentDomain.BaseDirectory;
            string launcherDir = Path.Combine(toolDir, "launcher");
            string launcherExe = Path.Combine(launcherDir, "APK2EXE_Launcher.exe");

            if (File.Exists(launcherExe))
            {
                Log("Launcher exists, skip compile.");
                return launcherExe;
            }

            Log("Compiling launcher (first run takes a few minutes)...");

            string projectDir = Path.GetFullPath(Path.Combine(toolDir, @"..\..\..\APK2EXE.Launcher"));
            if (!Directory.Exists(projectDir))
            {
                projectDir = Path.GetFullPath(Path.Combine(toolDir, @"..\..\src\APK2EXE.Launcher"));
            }
            string csproj = Path.Combine(projectDir, "APK2EXE.Launcher.csproj");

            if (!File.Exists(csproj))
            {
                throw new FileNotFoundException($"Launcher project not found: {csproj}");
            }

            Directory.CreateDirectory(launcherDir);

            var psi = new ProcessStartInfo
            {
                FileName = DotNetPath,
                Arguments = $"publish \"{csproj}\" -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true -o \"{launcherDir}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var proc = Process.Start(psi);
            string stdout = proc.StandardOutput.ReadToEnd();
            string stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (proc.ExitCode != 0)
            {
                Log("dotnet publish output:");
                Log(stdout);
                Log(stderr);
                throw new Exception($"Launcher compile failed, exit code: {proc.ExitCode}");
            }

            if (!File.Exists(launcherExe))
            {
                throw new FileNotFoundException($"Launcher not found after compile: {launcherExe}");
            }

            Log("Launcher compiled.");
            return launcherExe;
        }

        private static string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return string.IsNullOrWhiteSpace(name) ? "APK2EXE_App" : name;
        }
    }
}
