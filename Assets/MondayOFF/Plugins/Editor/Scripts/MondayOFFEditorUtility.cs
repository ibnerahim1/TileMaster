using UnityEngine;
using UnityEditor;

namespace MondayOFF.Editor {
    public static class MondayOFFEditorUtility {
        [MenuItem("MondayOFF/Create MondayOFF Game Object", false, 0)]
        private static void AddMondayOFFGameObject() {
            if (Object.FindObjectOfType(typeof(MondayOFF)) == null) {
                var prefabGUIDs = AssetDatabase.FindAssets("MondayOFF t:Prefab");
                if (prefabGUIDs.Length <= 0) {
                    Debug.LogWarning("MondayOFF.Prefab not found!");
                    return;
                }

                GameObject go = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(prefabGUIDs[0]), typeof(GameObject))) as GameObject;
                go.name = "MondayOFF";
                Selection.activeObject = go;
                Undo.RegisterCreatedObjectUndo(go, "Creating MondayOFF Game Object");
            } else {
                Debug.LogWarning("MondayOFFGameObject already exists in the scene");
            }
        }
    }
}