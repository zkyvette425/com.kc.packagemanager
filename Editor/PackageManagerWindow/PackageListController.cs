using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine.Networking;

namespace KC
{
    public class PackageListController
    {
        private Vector2 _scrollPosition;
        private Dictionary<string, PackageInstallInfo> _packages = new();
        public System.Action<PackageInstallInfo> OnPackageSelected;
        private ListRequest _request;
        public bool IsComplete { get; private set; }

        private List<SearchRequest> _searchRequests = new();
        private int _completedSearchRequests = 0;
        private bool _isInstalledPackagesLoading = true;
        private bool _isUninstalledPackagesLoading = true;
        private float _rotationAngleInstalled = 0f;
        private float _rotationAngleUninstalled = 0f;

        public void RefreshPackages()
        {
            _isInstalledPackagesLoading = true;
            _isUninstalledPackagesLoading = true;
            _rotationAngleInstalled = 0f;
            _rotationAngleUninstalled = 0f;
            _request = Client.List(false,true);
            EditorApplication.update += WaitInstalledPackages;
        }

        private void WaitInstalledPackages()
        {
            if (_request.IsCompleted)
            {
                EditorApplication.update -= WaitInstalledPackages;
                if (_request.Status != StatusCode.Success)
                {
                    Debug.LogError(_request.Error.message);
                    _isInstalledPackagesLoading = false;
                    return;
                }
                _packages.Clear();
                foreach (var info in _request.Result)
                {
                    if (info.name.StartsWith("com.kc."))
                    {
                        _packages.Add(info.name, new PackageInstallInfo { IsInstalled = true, Info = info });
                    }
                }
                _isInstalledPackagesLoading = false;
                CheckKcList();
            }
        }

        private void CheckKcList()
        {
            var request = UnityWebRequest.Get("https://api.openupm.com/packages/extra");
            request.SendWebRequest().completed += op =>
            {
                switch (request.result)
                {
                    case UnityWebRequest.Result.Success:
                        var json = request.downloadHandler.text;
                        var matches = Regex.Matches(json, @"""com.kc.[^""]+""");
                        var packageNames = matches.Select(m => m.Value.Trim('"')).ToList();
                        GetUninstallPackage(packageNames);
                        break;
                    case UnityWebRequest.Result.ConnectionError:
                        Debug.LogError("连接错误: " + request.error);
                        _isUninstalledPackagesLoading = false;
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError("协议错误: " + request.error);
                        _isUninstalledPackagesLoading = false;
                        break;
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError("数据处理错误: " + request.error);
                        _isUninstalledPackagesLoading = false;
                        break;
                }
            };
        }

        private void GetUninstallPackage(List<string> packageNames)
        {
            IsComplete = false;
            _completedSearchRequests = 0;
            bool hasUninstallPackage = false;
            foreach (var packageName in packageNames)
            {
                if (_packages.ContainsKey(packageName))
                {
                    continue;
                }
                hasUninstallPackage = true;
                var searchRequest = Client.Search(packageName);
                _searchRequests.Add(searchRequest);

                EditorApplication.CallbackFunction callback = null;
                callback = () => HandleSearchRequestCompletion(searchRequest, callback);
                EditorApplication.update += callback;
            }
            if (!hasUninstallPackage)
            {
                _isUninstalledPackagesLoading = false;
                IsComplete = true;
            }
        }

        private void HandleSearchRequestCompletion(SearchRequest request, EditorApplication.CallbackFunction callback)
        {
            if (request.IsCompleted)
            {
                EditorApplication.update -= callback;
                if (request.Status == StatusCode.Success)
                {
                    if (request.Result == null || request.Result.Length == 0)
                    {
                        return;
                    }
                    var package = request.Result[0];
                    Debug.Log(package.name);
                    _packages.Add(package.name, new PackageInstallInfo { IsInstalled = false, Info = package });
                }
                _completedSearchRequests++;
                if (_completedSearchRequests == _searchRequests.Count)
                {
                    _isUninstalledPackagesLoading = false;
                    IsComplete = true;
                }
            }
        }

        public void DrawPackageList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.3f));
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("已安装包", EditorStyles.boldLabel);
                if (_isInstalledPackagesLoading)
                {
                    _rotationAngleInstalled += 25f;
                    if (_rotationAngleInstalled >= 360f)
                    {
                        _rotationAngleInstalled -= 360f;
                    }
                    DrawRotatedIcon(_rotationAngleInstalled);
                }
            }
            foreach (var package in _packages.Values)
            {
                if (!package.IsInstalled)
                {
                    continue;
                }
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    var name = package.Info.name;
                    if (package.Info.versions.compatible.Length > 0 && PackageManagerHelper.CompareVersion(package.Info.version,
                            package.Info.versions.compatible[^1]) < 0)
                    {
                        name = $"{name}  ↑";
                    }
                    if (GUILayout.Button(name))
                    {
                        OnPackageSelected?.Invoke(package);
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("未安装包", EditorStyles.boldLabel);
                if (_isUninstalledPackagesLoading)
                {
                    _rotationAngleUninstalled += 25f;
                    if (_rotationAngleUninstalled >= 360f)
                    {
                        _rotationAngleUninstalled -= 360f;
                    }
                    DrawRotatedIcon(_rotationAngleUninstalled);
                }
            }
            foreach (var package in _packages.Values)
            {
                if (package.IsInstalled)
                {
                    continue;
                }
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    if (GUILayout.Button(package.Info.name))
                    {
                        OnPackageSelected?.Invoke(package);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawRotatedIcon(float angle)
        {
            var icon = EditorGUIUtility.IconContent("WaitSpin00");
            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(16), GUILayout.Height(16));
            var matrixBackup = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, rect.center);
            GUI.Label(rect, icon);
            GUI.matrix = matrixBackup;
        }
    }
    
}    