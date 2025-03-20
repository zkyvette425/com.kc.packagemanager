using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace KC
{
    public static partial class PackageManagerHelper
    {
        public static string ProjectPath { get; } = Path.GetDirectoryName(Application.dataPath);
        public static string PackageCachePath { get; } = Path.Combine(ProjectPath, "Library", "PackageCache");
        public static string PackagesPath { get; } = Path.Combine(ProjectPath, "Packages");
        public static string BatPath { get; } = Path.Combine(ProjectPath, "Packages/com.kc.packagemanager/MoveToPackages.ps1");


        public static void FromLibraryToPackageWithWaitForExit(string packageName, string version)
        {
            if (!File.Exists(BatPath))
            {
                Debug.LogError($"找不到目录: {BatPath} 的bat文件");
                return;
            }
            if (!Directory.Exists(PackageCachePath))
            {
                Debug.LogError($"找不到包缓存目录: {PackageCachePath}");
                return;
            }

            // 确保目标目录存在
            Directory.CreateDirectory(PackagesPath);

            Debug.Log($"move package: {packageName}");
            Process process = ProcessHelper.PowerShell($"-NoExit -ExecutionPolicy Bypass -File ./Packages/com.kc.packagemanager/MoveToPackages.ps1 {packageName} {version}", waitExit: true);
            process.WaitForExit(); // 等待进程执行完成
            string output = process.StandardOutput.ReadToEnd();
            Debug.Log(output);
        }
        
        public static void FromLibraryToPackageWithExitedEvent(string packageName, string version)
        {
            if (!File.Exists(BatPath))
            {
                Debug.LogError($"找不到目录: {BatPath} 的bat文件");
                return;
            }
            if (!Directory.Exists(PackageCachePath))
            {
                Debug.LogError($"找不到包缓存目录: {PackageCachePath}");
                return;
            }

            // 确保目标目录存在
            Directory.CreateDirectory(PackagesPath);

            Debug.Log($"move package: {packageName}");
            Process process = ProcessHelper.PowerShell($"-NoExit -ExecutionPolicy Bypass -File ./Packages/com.kc.packagemanager/MoveToPackages.ps1 {packageName} {version}", waitExit: true);
            process.EnableRaisingEvents = true; // 启用事件触发
            process.Exited += (sender, args) =>
            {
                string output = process.StandardOutput.ReadToEnd();
                Debug.Log(output);
                AssetDatabase.Refresh();
            };
            process.Start();
        }

        public static void DeletePackage(string packageName)
        {
            var path = Path.Combine(PackagesPath, packageName);
            if (!Directory.Exists(path))
            {
                return;
            }
            Directory.Delete(path, true);
        }
    }
}    