using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;

namespace KC
{
    public class PackageCreationWindow : EditorWindow
    {
        private string _authorName = "";
        private string _initialVersion = "0.1.0";
        private string _packageName = "";
        private string _displayName;
        private static readonly Regex ValidPackageNameRegex = new Regex(@"^[a-z][a-z0-9]*(\.[a-z][a-z0-9]*)*$");

        [MenuItem("KC/PackageManager/创建包",false,2)]
        public static void ShowWindow()
        {
            var window = GetWindow<PackageCreationWindow>("创建包");
            window.Show();
        }

        private void OnGUI()
        {
            _authorName = EditorGUILayout.TextField("作者", _authorName);
            _initialVersion = EditorGUILayout.TextField("初始版本号", _initialVersion);
            _packageName = EditorGUILayout.TextField("包名称", _packageName);
            
            
            GUI.enabled = false;
            _displayName = EditorGUILayout.TextField("显示名称", FormatDisplayName(_packageName));
            GUI.enabled = true;
            
            if (string.IsNullOrEmpty(_packageName))
            {
                return;
            }
            
            if (!ValidPackageNameRegex.IsMatch(_packageName))
            {
                EditorGUILayout.HelpBox("包名称 只能以小写字母开头，包含小写字母、数字和点号，例如 a.b.c 或 a1.b2.c3", MessageType.Error);
            }
            
            if (IsPackageExists(_packageName))
            {
                EditorGUILayout.HelpBox($"包 com.kc.{_packageName} 已存在，请选择其他名称。", MessageType.Error);
            }
            
            GUI.enabled = !string.IsNullOrEmpty(_packageName) && ValidPackageNameRegex.IsMatch(_packageName);
            if (GUILayout.Button("确定"))
            {
                CreatePackage(_authorName, _initialVersion, _packageName);
                Close();
            }
            GUI.enabled = true;
        }
        
        private bool IsPackageExists(string packageName)
        {
            string fullPackageName = $"com.kc.{packageName}";
            string packageCachePath = Path.Combine(PackageManagerHelper.PackageCachePath);
            string packagesPath = Path.Combine(PackageManagerHelper.PackagesPath);

            var regex = new Regex($"^{Regex.Escape(fullPackageName)}(@[\\w.-]+)?$");
            
            // 检查 Library/PackageCache 中是否存在精确匹配的文件夹
            string[] cacheFolders = Directory.GetDirectories(packageCachePath);
            foreach (string folder in cacheFolders)
            {
                if (regex.IsMatch(Path.GetFileName(folder)))
                {
                    return true;
                }
            }

            // 检查 Packages 文件夹中是否存在该包名的文件夹
            string targetPackagePath = Path.Combine(packagesPath, fullPackageName);
            if (Directory.Exists(targetPackagePath))
            {
                return true;
            }

            return false;
        }

        private void CreatePackage(string authorName, string version, string packageName)
        {
            string fullPackageName = $"com.kc.{packageName}";
            string packagePath = Path.Combine(PackageManagerHelper.PackagesPath, fullPackageName);

            Directory.CreateDirectory(packagePath);

            string editorPath = Path.Combine(packagePath, "Editor");
            string runtimePath = Path.Combine(packagePath, "Runtime");
            Directory.CreateDirectory(editorPath);
            Directory.CreateDirectory(runtimePath);

            string packageJson = GeneratePackageJson(authorName, version, fullPackageName);
            File.WriteAllText(Path.Combine(packagePath, "package.json"), packageJson);

            string editorAsmdef = GenerateAsmdef($"KC.{_displayName}.Editor", false);
            string runtimeAsmdef = GenerateAsmdef($"KC.{_displayName}.Runtime", true);
            File.WriteAllText(Path.Combine(editorPath, $"KC.{_displayName}.Editor.asmdef"), editorAsmdef);
            File.WriteAllText(Path.Combine(runtimePath, $"KC.{_displayName}.Runtime.asmdef"), runtimeAsmdef);

            AssetDatabase.Refresh();
        }

        private string GeneratePackageJson(string authorName, string version, string fullPackageName)
        {
            StringBuilder json = new StringBuilder();
            json.AppendLine("{");
            json.AppendLine($"  \"name\": \"{fullPackageName}\",");
            json.AppendLine($"  \"displayName\": \"KC.{_displayName}\",");
            json.AppendLine($"  \"version\": \"{version}\",");
            json.AppendLine($"  \"unity\": \"2022.3\",");
            json.AppendLine($"  \"description\": \"\",");
            json.AppendLine($"  \"author\": {{");
            json.AppendLine($"    \"name\": \"{authorName}\",");
            json.AppendLine($"    \"url\": \"\"");
            json.AppendLine($"  }},");
            json.AppendLine($"  \"repository\": {{");
            json.AppendLine($"    \"type\": \"git\",");
            json.AppendLine($"    \"url\":\"\"");
            json.AppendLine($"  }},");
            json.AppendLine($"  \"relatedPackages\": {{}},");
            json.AppendLine($"  \"license\": \"MIT\",");
            json.AppendLine($"  \"dependencies\": {{}},");
            json.AppendLine($"  \"publishConfig\": {{");
            json.AppendLine($"    \"registry\": \"https://package.openupm.com\"");
            json.AppendLine($"  }}");
            json.AppendLine("}");
            return json.ToString();
        }

        private string GenerateAsmdef(string name, bool isRuntime)
        {
            StringBuilder asmdef = new StringBuilder();
            asmdef.AppendLine("{");
            asmdef.AppendLine($"  \"name\": \"{name}\",");
            asmdef.AppendLine($"  \"rootNamespace\": \"KC\",");
            asmdef.AppendLine($"  \"references\": [],");
            asmdef.AppendLine(!isRuntime ? $"  \"includePlatforms\": [\"Editor\"]," : $"  \"includePlatforms\": [],");
            asmdef.AppendLine($"  \"excludePlatforms\": [],");
            asmdef.AppendLine($"  \"allowUnsafeCode\": false,");
            asmdef.AppendLine($"  \"overrideReferences\": false,");
            asmdef.AppendLine($"  \"precompiledReferences\": [],");
            asmdef.AppendLine($"  \"autoReferenced\": true,");
            asmdef.AppendLine($"  \"defineConstraints\": [],");
            asmdef.AppendLine($"  \"versionDefines\": [],");
            asmdef.AppendLine($"  \"noEngineReferences\": false");
            asmdef.AppendLine("}");
            return asmdef.ToString();
        }
        
        private string FormatDisplayName(string packageName)
        {
            string[] parts = packageName.Split('.');
            StringBuilder displayName = new StringBuilder();
            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                {
                    displayName.Append(" ");
                }
                if (parts[i].Length > 0)
                {
                    displayName.Append(char.ToUpper(parts[i][0]));
                    displayName.Append(parts[i].Substring(1));
                }
            }
            return displayName.ToString();
        }
    }
}    