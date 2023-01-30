using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using UnityEditor.Android;
using System.Collections.Generic;
using System;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Linq;

namespace YsoCorp {

    namespace GameUtils {
        public class GameUtilsProcessor : IPreprocessBuildWithReport, IPostGenerateGradleAndroidProject {

            public int callbackOrder {
                get { return int.MaxValue; }
            }

            public void OnPreprocessBuild(BuildReport report) {
                YCConfig ycConfig = YCConfig.Create();
                if (ycConfig.gameYcId == "") {
                    throw new Exception("[GameUtils] Empty Game Yc Id");
                }
                if (ycConfig.FbAppId == "") {
                    throw new Exception("[GameUtils] Empty Fb App Id");
                }
#if UNITY_IOS
                if (Directory.Exists("Assets/MaxSdk/Mediation/Google") && ycConfig.AdMobIosAppId == "") {
                    throw new BuildFailedException("[GameUtils] Empty AdMob IOS Id");
                } else if (Directory.Exists("Assets/MaxSdk/Mediation/Google") == false && ycConfig.AdMobIosAppId != "") {
                    throw new BuildFailedException("[GameUtils] AdMob IOS Id found but the network is not installed");
                }
                if (ycConfig.IosInterstitial == "" || ycConfig.IosRewarded == "" || ycConfig.IosBanner == "") {
                    throw new Exception("[GameUtils] Empty iOS Ad Units");
                }
#elif UNITY_ANDROID
                if (Directory.Exists("Assets/MaxSdk/Mediation/Google") && ycConfig.AdMobAndroidAppId == "") {
                    throw new BuildFailedException("[GameUtils] Empty AdMob Android Id");
                } else if (Directory.Exists("Assets/MaxSdk/Mediation/Google") == false && ycConfig.AdMobAndroidAppId != "") {
                    throw new BuildFailedException("[GameUtils] AdMob Android Id found but the network is not installed");
                }
                if (ycConfig.AndroidInterstitial == "" || ycConfig.AndroidRewarded == "" || ycConfig.AndroidBanner == "") {
                    throw new Exception("[GameUtils] Empty Android Ad Units");
                }
#endif
                ycConfig.InitFacebook();
                ycConfig.InitMax();
                ycConfig.InitAmazon();
            }

            private void GradleReplaces(string path, string file, List<KeyValuePair<string, string>> replaces) {
                try {
                    string gradleBuildPath = Path.Combine(path, file);
                    string content = File.ReadAllText(gradleBuildPath);
                    foreach (KeyValuePair<string, string> r in replaces) {
                        content = content.Replace(r.Key, r.Value);
                    }
                    File.WriteAllText(gradleBuildPath, content);
                } catch { }
            }

            public void OnPostGenerateGradleAndroidProject(string path) {
#if UNITY_ANDROID
                this.GradleReplaces(path, "../build.gradle", new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("com.android.tools.build:gradle:3.4.0", "com.android.tools.build:gradle:3.4.+")
                });
                this.GradleReplaces(path, "../unityLibrary/Tenjin/build.gradle", new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("implementation fileTree(dir: 'libs', include: ['*.jar'])", "implementation fileTree(dir: 'libs', include: ['*.jar', '*.aar'])")
                });
#endif
            }

            [PostProcessBuild(int.MaxValue)]
            public static void ChangeXcodePlist(BuildTarget buildTarget, string path) {
                if (buildTarget == BuildTarget.iOS) {
#if UNITY_IOS
                    YCConfig ycConfig = YCConfig.Create();
                    string plistPath = path + "/Info.plist";
                    PlistDocument plist = new PlistDocument();
                    plist.ReadFromFile(plistPath);
                    PlistElementDict rootDict = plist.root;

                    PlistElementArray rootCapacities = (PlistElementArray)rootDict.values["UIRequiredDeviceCapabilities"];
                    rootCapacities.values.RemoveAll((PlistElement elem) => {
                        return elem.AsString() == "metal";
                    });

                    rootDict.SetString("NSCalendarsUsageDescription", "Used to deliver better advertising experience");
                    rootDict.SetString("NSLocationWhenInUseUsageDescription", "Used to deliver better advertising experience");
                    rootDict.SetString("NSPhotoLibraryUsageDescription", "Used to deliver better advertising experience");
                    rootDict.SetString("NSAdvertisingAttributionReportEndpoint", "https://tenjin-skan.com");
                    rootDict.values.Remove("UIApplicationExitsOnSuspend");
#if AMAZON_APS
                    bool hasAmazonSKAdNetwork = false;
                    PlistElement SKAdNetworkItems;
                    plist.root.values.TryGetValue("SKAdNetworkItems", out SKAdNetworkItems);
                    if (SKAdNetworkItems == null || SKAdNetworkItems.GetType() != typeof(PlistElementArray)) { // if the array does not exist, create it
                        SKAdNetworkItems = plist.root.CreateArray("SKAdNetworkItems");
                    } else {
                        IEnumerable<PlistElement> SKAdNetworks = SKAdNetworkItems.AsArray().values.Where(plistElement => plistElement.GetType() == typeof(PlistElementDict));
                        foreach (PlistElement SKAdNetwork in SKAdNetworks) { // Check if the SKAdNetwork already exists
                            PlistElement current;
                            SKAdNetwork.AsDict().values.TryGetValue("SKAdNetworkIdentifier", out current);
                            if (current != null && current.GetType() == typeof(PlistElementString) && current.AsString() == "p78axxw29g.skadnetwork") {
                                hasAmazonSKAdNetwork = true;
                                break;
                            }
                        }
                    }
                    if (hasAmazonSKAdNetwork == false) {
                        PlistElementDict amazonSKAd = SKAdNetworkItems.AsArray().AddDict();
                        amazonSKAd.SetString("SKAdNetworkIdentifier", "p78axxw29g.skadnetwork");
                    }
#endif
                    File.WriteAllText(plistPath, plist.WriteToString());
#endif
                }
            }

        }

    }

}