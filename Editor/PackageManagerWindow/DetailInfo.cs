using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace KC
{
    public class PackageInstallInfo
    {
        public bool IsInstalled { get; set; }
        
        public UnityEditor.PackageManager.PackageInfo Info { get; set; }
    }
}