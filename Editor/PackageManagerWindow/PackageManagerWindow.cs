using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace KC
{
    public class PackageManagerWindow : EditorWindow
    {
        private PackageListController _packageListController;
        private PackageDetailsController _packageDetailsController;
        private PackageInstallController _packageInstallController;
        
        [MenuItem("KC/PackageManager/包管理面板",false,1)]
        public static void ShowWindow()
        {
            GetWindow<PackageManagerWindow>("包管理面板");
        }
        
        private void OnEnable()
        {
            _packageListController = new PackageListController();
            _packageDetailsController = new PackageDetailsController();
            _packageInstallController = new PackageInstallController();

            _packageListController.OnPackageSelected +=_packageDetailsController.OnPackageSelected;
            _packageListController.OnPackageSelected += _packageInstallController.OnPackageSelected;

            _packageListController.RefreshPackages();
        }

        private void OnDisable()
        {
            _packageListController.OnPackageSelected -=_packageDetailsController.OnPackageSelected;
            _packageListController.OnPackageSelected -= _packageInstallController.OnPackageSelected;
            
            _packageListController = null;
            _packageDetailsController = null;
            _packageInstallController = null;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            _packageListController.DrawPackageList();

            if (_packageListController.IsComplete)
            {
                EditorGUILayout.BeginVertical();
                _packageDetailsController.DrawPackageDetails();
                _packageInstallController.DrawInstallButtons();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}