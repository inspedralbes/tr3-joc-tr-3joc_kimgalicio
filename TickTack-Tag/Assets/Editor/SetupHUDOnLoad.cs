using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace EditorScripts
{
    [InitializeOnLoad]
    public static class SetupHUDOnLoad
    {
        static SetupHUDOnLoad()
        {
            EditorApplication.delayCall += SetupHUD;
        }

        private static void SetupHUD()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;

            Scene activeScene = EditorSceneManager.GetActiveScene();
            
            if (activeScene.name != "TickTack-Tag" && activeScene.name != "Menu") 
            {
            }

            GameObject existingHUD = GameObject.Find("InGameHUD");
            if (existingHUD != null)
            {
                return;
            }

            GameObject hudObj = new GameObject("InGameHUD");

            UIDocument uiDoc = hudObj.AddComponent<UIDocument>();
            uiDoc.sortingOrder = 10;
            
            VisualTreeAsset hudUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/HUD.uxml");
            if (hudUxml != null)
            {
                uiDoc.visualTreeAsset = hudUxml;
            }
            else
            {
                Debug.LogError("[SetupHUD] No s'ha trobat l'arxiu Assets/UI/HUD.uxml");
            }

            HUDController hudController = hudObj.AddComponent<HUDController>();
            
            GameStateSO gameStateSO = AssetDatabase.LoadAssetAtPath<GameStateSO>("Assets/scripts/New Game State SO.asset");
            if (gameStateSO != null)
            {
                SerializedObject so = new SerializedObject(hudController);
                SerializedProperty prop = so.FindProperty("gameStateSO");
                if (prop != null)
                {
                    prop.objectReferenceValue = gameStateSO;
                    so.ApplyModifiedProperties();
                }
            }
            else
            {
                Debug.LogWarning("[SetupHUD] No s'ha trobat el 'New Game State SO.asset' a Assets/scripts/.");
            }

            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);

            Debug.Log("[SetupHUD] HUD configurat automàticament a l'escena: " + activeScene.name);
        }
    }
}
