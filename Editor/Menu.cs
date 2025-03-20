using System.Diagnostics;
using System.IO;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace KC
{
    public static partial class Menu
    {
        [MenuItem("KC/PackageManager/转移所有kc包至Packages",false,2)]
        public static void FromLibraryToPackage()
        {

            var batPath = PackageManagerHelper.BatPath;
            var packageCachePath = PackageManagerHelper.PackageCachePath;
            var packagesPath = PackageManagerHelper.PackagesPath;
            
            if (!File.Exists(batPath))
            {
                Debug.LogError($"找不到目录: {batPath} 的bat文件");
                return;
            }
            if (!Directory.Exists(packageCachePath))
            {
                Debug.LogError($"找不到包缓存目录: {packageCachePath}");
                return;
            }

            // 确保目标目录存在
            Directory.CreateDirectory(packagesPath);
            
            foreach (string sourceDir in Directory.GetDirectories(packageCachePath, "com.kc*"))
            {
                string folderName = Path.GetFileName(sourceDir);
                var versionSplit = folderName.Split('@');
                
                Debug.Log($"move package: {folderName}");
                Process process = ProcessHelper.PowerShell($"-NoExit -ExecutionPolicy Bypass -File ./Packages/com.kc.packagemanager/MoveToPackages.ps1 {versionSplit[0]} {versionSplit[1]}", waitExit: true);
                Debug.Log(process.StandardOutput.ReadToEnd());
            }
        }
    }
}
