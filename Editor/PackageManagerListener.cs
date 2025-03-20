using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace KC
{
    [InitializeOnLoad]
    public class PackageManagerListener
    {
        static PackageManagerListener()
        {
            Events.registeredPackages += OnRegisteredPackages;
        }

        private static void OnRegisteredPackages(PackageRegistrationEventArgs obj)
        {
            foreach (var added in obj.added)
            {
                if (!CheckKc(added))
                {
                    continue;
                }
                PackageManagerHelper.FromLibraryToPackageWithExitedEvent(added.name, added.version);
            }

            foreach (var removed in obj.removed)
            {
                if (!CheckKc(removed))
                {
                    continue;
                }
                Debug.Log($"removed package:{removed.name}@{removed.version}");
            }

            foreach (var changedFrom in obj.changedFrom)
            {
                if (!CheckKc(changedFrom))
                {
                    continue;
                }
                PackageManagerHelper.FromLibraryToPackageWithExitedEvent(changedFrom.name, changedFrom.version);
            }
            
            foreach (var changedTo in obj.changedTo)
            {
                if (!CheckKc(changedTo))
                {
                    continue;
                }
                PackageManagerHelper.FromLibraryToPackageWithExitedEvent(changedTo.name, changedTo.version);
            }
            
            AssetDatabase.Refresh();
        }

        private static bool CheckKc(PackageInfo info)
        {
            return info.name.StartsWith("com.kc.");
        }
    }
}