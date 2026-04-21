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
            // Només executar a l'editor fora del mode de joc
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;

            // Obtenir l'escena activa
            Scene activeScene = EditorSceneManager.GetActiveScene();
            
            // Assegurar-nos que és l'escena principal, canvia el nom si és diferent
            if (activeScene.name != "TickTack-Tag" && activeScene.name != "Menu") 
            {
                // Si l'escena activa no es ni TickTack-Tag ni Menu, potser no hem d'actuar
                // Però només instal·larem el HUD on no existeixi prèviament.
            }

            // Comprovar si ja hem configurat el HUD
            GameObject existingHUD = GameObject.Find("InGameHUD");
            if (existingHUD != null)
            {
                return; // Ja existeix
            }

            // Crear el GameObject
            GameObject hudObj = new GameObject("InGameHUD");

            // Afegir UIDocument
            UIDocument uiDoc = hudObj.AddComponent<UIDocument>();
            uiDoc.sortingOrder = 10;
            
            // Carregar i assignar el HUD.uxml
            VisualTreeAsset hudUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/HUD.uxml");
            if (hudUxml != null)
            {
                uiDoc.visualTreeAsset = hudUxml;
            }
            else
            {
                Debug.LogError("[SetupHUD] No s'ha trobat l'arxiu Assets/UI/HUD.uxml");
            }

            // Afegir HUDController
            HUDController hudController = hudObj.AddComponent<HUDController>();
            
            // Carregar i assignar el GameStateSO
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

            // Marcar l'escena com a modificada i desar-la
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);

            Debug.Log("[SetupHUD] HUD configurat automàticament a l'escena: " + activeScene.name);
        }
    }
}
