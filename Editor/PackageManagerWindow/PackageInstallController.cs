using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace KC
{
    public class PackageInstallController
    {
        private PackageInstallInfo _package;
        private Vector2 _scrollPosition;

        public void OnPackageSelected(PackageInstallInfo package)
        {
            this._package = package;
        }

        public void DrawInstallButtons()
        {
            if (_package != null)
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.7f));
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

                EditorGUILayout.LabelField("Version History", EditorStyles.boldLabel);
                for (int i = _package.Info.versions.compatible.Length - 1; i >= 0; i--)
                {
                    var version = _package.Info.versions.compatible[i];
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        // 明确设置版本标签宽度
                        EditorGUILayout.LabelField(version, GUILayout.Width(120));

                        if (_package.IsInstalled)
                        {
                            if (version == _package.Info.version)
                            {
                                // 按钮设置固定宽度
                                if (GUILayout.Button("Remove", GUILayout.Width(80))) 
                                {
                                    RemovePackage(_package.Info.name);
                                }
                                EditorGUILayout.LabelField("current", EditorStyles.label, GUILayout.Width(60)); // 设置宽度
                            }
                            else
                            {
                                // 按钮设置固定宽度
                                if (GUILayout.Button("Update", GUILayout.Width(80))) 
                                {
                                    InstallPackage(_package.Info.name,version);
                                }
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("Setup", GUILayout.Width(80))) 
                            {
                                InstallPackage(_package.Info.name,version);
                            }
                        }
                        
                        
                        if (i == _package.Info.versions.compatible.Length - 1)
                        {
                            EditorGUILayout.LabelField("latest", EditorStyles.label, GUILayout.Width(60)); // 设置宽度
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
        }
        
        private void InstallPackage(string packageName,string version)
        {
            if (_package.IsInstalled)
            {
                PackageManagerHelper.DeletePackage(packageName);
            }
            var installRequest = Client.Add($"{packageName}@{version}");
            EditorApplication.update += () =>
            {
                if (installRequest.IsCompleted)
                {
                    EditorApplication.update -= () => { };
                    if (installRequest.Status == StatusCode.Success)
                    {
                        // 刷新包列表
                        var packageListController = new PackageListController();
                        packageListController.RefreshPackages();
                    }
                    else if (installRequest.Status >= StatusCode.Failure)
                    {
                        Debug.LogError(installRequest.Error.message);
                    }
                }
            };
        }

        private void RemovePackage(string packageName)
        {
            PackageManagerHelper.DeletePackage(packageName);
            var removeRequest = Client.Remove(packageName);
            EditorApplication.update += () =>
            {
                if (removeRequest.IsCompleted)
                {
                    EditorApplication.update -= () => { };
                    if (removeRequest.Status == StatusCode.Success)
                    {
                        // 刷新包列表
                        var packageListController = new PackageListController();
                        packageListController.RefreshPackages();
                    }
                    else if (removeRequest.Status >= StatusCode.Failure)
                    {
                        Debug.LogError(removeRequest.Error.message);
                    }
                }
            };
        }
    }
}