#if UNITY_ANDROID
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

namespace MondayOFF.Editor {
    class AndroidPreprocessor : IPreprocessBuildWithReport {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report) {
            AddFacebookToProguard();
        }

        private static void AddFacebookToProguard() {
            string android_path = $"Plugins{Path.DirectorySeparatorChar}Android{Path.DirectorySeparatorChar}";
            string proguardFilename =
#if UNITY_2021_2_OR_NEWER
            "proguard-user.txt"
#else
            "proguard.txt"
#endif
            ;
            
            const string CONTENT = @"-keep class com.facebook.** { *; }";

            string androidPath = Path.Combine(Application.dataPath, android_path);
            Directory.CreateDirectory(androidPath);

            string proguardPath = Path.Combine(androidPath, proguardFilename);

            if (File.Exists(proguardPath)) {
                string currentContent = File.ReadAllText(proguardPath);
                if (currentContent.Contains(CONTENT)) {
                    Debug.Log("Facebook libraries are already added to proguard");
                } else {
                    Debug.Log($"Appending \n{CONTENT}\n to {proguardPath}");

                    currentContent += $"\n\n{CONTENT}";
                    File.WriteAllText(proguardPath, currentContent);
                }
            } else {
                Debug.Log($"Creating {proguardPath}");

                using (StreamWriter writer = File.CreateText(proguardPath)) {
                    writer.Write(CONTENT);
                }
            }
        }
    }
}
#endif