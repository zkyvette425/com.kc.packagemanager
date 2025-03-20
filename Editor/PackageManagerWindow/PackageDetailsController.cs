using UnityEditor;
using UnityEngine;

namespace KC
{
    public class PackageDetailsController
    {
        private PackageInstallInfo _selectedPackage;
        private Vector2 _scrollPosition;

        public void OnPackageSelected(PackageInstallInfo package)
        {
            _selectedPackage = package;
        }

        public void DrawPackageDetails()
        {
            if (_selectedPackage != null)
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.7f));
                EditorGUILayout.LabelField(_selectedPackage.Info.name, EditorStyles.boldLabel);
                if (_selectedPackage.IsInstalled)
                {
                    EditorGUILayout.LabelField($"Version: {_selectedPackage.Info.version}");
                }
                EditorGUILayout.LabelField($"Author: {_selectedPackage.Info.author.name}");
                EditorGUILayout.LabelField($"Description: {_selectedPackage.Info.description}");
                EditorGUILayout.EndVertical();
            }
        }
    }
}