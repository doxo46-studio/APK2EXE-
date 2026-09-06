using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading;

namespace APK2EXE.Launcher
{
    class Program
    {
        const string Magic = "AP2E";
        const int FooterSize = 12;

        // 运行时安装目录（所有应用共享）
        static readonly string RuntimeDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "APK2EXE", "Runtime");
        static readonly string RuntimeReadyFile = Path.Combine(RuntimeDir, ".ready");
        static readonly string RuntimeUrlFile = Path.Combine(RuntimeDir, ".runtime_url");

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("=== APK2EXE 启动器 ===");

                string exePath = Process.GetCurrentProcess().MainModule.FileName;
                byte[] fileData = File.ReadAllBytes(exePath);

                if (fileData.Length < FooterSize)
                {
                    Console.WriteLine("错误：文件格式不正确");
                    Pause();
                    return;
                }

                string magic = Encoding.ASCII.GetString(fileData, fileData.Length - FooterSize, 4);
                if (magic != Magic)
                {
                    Console.WriteLine("错误：不是有效的 APK2EXE 文件");
                    Pause();
                    return;
                }

                long zipOffset = BitConverter.ToInt64(fileData, fileData.Length - 8);
                int zipLength = (int)(fileData.Length - FooterSize - zipOffset);

                // 读取应用名和运行时下载地址
                string appName = "APK2EXE_App";
                string runtimeUrl = "";

                using (var ms = new MemoryStream(fileData, (int)zipOffset, zipLength))
                using (var zip = new ZipArchive(ms, ZipArchiveMode.Read))
                {
                    var infoEntry = zip.GetEntry("appname.txt");
                    if (infoEntry != null)
                    {
                        using (var reader = new StreamReader(infoEntry.Open()))
                            appName = reader.ReadToEnd().Trim();
                    }
                    var urlEntry = zip.GetEntry("runtime_url.txt");
                    if (urlEntry != null)
                    {
                        using (var reader = new StreamReader(urlEntry.Open()))
                            runtimeUrl = reader.ReadToEnd().Trim();
                    }
                }

                Console.WriteLine($"应用：{appName}");

                // 确保运行时已安装
                EnsureRuntime(runtimeUrl);

                // 解压应用文件到临时目录
                string appDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "APK2EXE", "Apps", SanitizeFileName(appName));
                string appReadyFile = Path.Combine(appDir, ".ready");

                if (!File.Exists(appReadyFile))
                {
                    Console.WriteLine($"正在解压应用文件到 {appDir} ...");
                    if (Directory.Exists(appDir))
                        Directory.Delete(appDir, true);
                    Directory.CreateDirectory(appDir);

                    using (var ms = new MemoryStream(fileData, (int)zipOffset, zipLength))
                    using (var zip = new ZipArchive(ms, ZipArchiveMode.Read))
                    {
                        foreach (var entry in zip.Entries)
                        {
                            if (entry.FullName.EndsWith("/")) continue;
                            string destPath = Path.Combine(appDir, entry.FullName);
                            Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                            entry.ExtractToFile(destPath, true);
                        }
                    }
                    File.WriteAllText(appReadyFile, DateTime.Now.ToString("o"));
                    Console.WriteLine("应用解压完成。");
                }

                // 运行启动脚本
                string startScript = Path.Combine(appDir, "start.bat");
                if (!File.Exists(startScript))
                {
                    Console.WriteLine("错误：找不到启动脚本 start.bat");
                    Pause();
                    return;
                }

                Console.WriteLine($"正在启动 {appName} ...");
                var psi = new ProcessStartInfo
                {
                    FileName = startScript,
                    WorkingDirectory = appDir,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"启动失败：{ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Pause();
            }
        }

        static void EnsureRuntime(string runtimeUrl)
        {
            if (File.Exists(RuntimeReadyFile))
            {
                Console.WriteLine("运行时已安装，跳过下载。");
                return;
            }

            if (string.IsNullOrWhiteSpace(runtimeUrl))
            {
                Console.WriteLine("警告：未配置运行时下载地址，跳过运行时下载。");
                return;
            }

            Console.WriteLine("运行时未安装，正在下载...");
            Console.WriteLine($"下载地址：{runtimeUrl}");

            Directory.CreateDirectory(RuntimeDir);

            // 下载运行时 ZIP
            string zipPath = Path.Combine(Path.GetTempPath(), $"ap2e_runtime_{Guid.NewGuid():N}.zip");
            try
            {
                DownloadFileWithProgress(runtimeUrl, zipPath);

                Console.WriteLine("下载完成，正在解压运行时...");
                ZipFile.ExtractToDirectory(zipPath, RuntimeDir);

                // 写入标记文件
                File.WriteAllText(RuntimeReadyFile, DateTime.Now.ToString("o"));
                File.WriteAllText(RuntimeUrlFile, runtimeUrl);

                Console.WriteLine("运行时安装完成。");
            }
            finally
            {
                if (File.Exists(zipPath))
                {
                    try { File.Delete(zipPath); } catch { }
                }
            }
        }

        static void DownloadFileWithProgress(string url, string destPath)
        {
            using (var client = new WebClient())
            {
                // 支持 TLS 1.2
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                long lastBytes = 0;
                DateTime lastTime = DateTime.Now;

                client.DownloadProgressChanged += (s, e) =>
                {
                    if (e.TotalBytesToReceive > 0)
                    {
                        double percent = e.BytesReceived * 100.0 / e.TotalBytesToReceive;
                        double mbReceived = e.BytesReceived / 1024.0 / 1024.0;
                        double mbTotal = e.TotalBytesToReceive / 1024.0 / 1024.0;

                        TimeSpan elapsed = DateTime.Now - lastTime;
                        if (elapsed.TotalSeconds >= 1)
                        {
                            double speed = (e.BytesReceived - lastBytes) / 1024.0 / 1024.0 / elapsed.TotalSeconds;
                            lastBytes = e.BytesReceived;
                            lastTime = DateTime.Now;
                            Console.WriteLine($"\r下载中：{percent:F1}%  ({mbReceived:F1}/{mbTotal:F1} MB, {speed:F1} MB/s)   ");
                        }
                    }
                    else
                    {
                        double mbReceived = e.BytesReceived / 1024.0 / 1024.0;
                        Console.WriteLine($"\r下载中：{mbReceived:F1} MB   ");
                    }
                };

                var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
                client.DownloadFileCompleted += (s, e) =>
                {
                    if (e.Error != null)
                        tcs.SetException(e.Error);
                    else
                        tcs.SetResult(true);
                };

                client.DownloadFileAsync(new Uri(url), destPath);
                tcs.Task.Wait();
                Console.WriteLine();
            }
        }

        static string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return string.IsNullOrWhiteSpace(name) ? "APK2EXE_App" : name;
        }

        static void Pause()
        {
            Console.WriteLine();
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
    }
}
