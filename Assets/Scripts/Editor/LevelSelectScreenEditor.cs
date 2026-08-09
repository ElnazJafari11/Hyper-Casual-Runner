using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using HyperCasualRunner.UI;

namespace HyperCasualRunner.Editor
{
    [CustomEditor(typeof(LevelSelectScreenController))]
    public class LevelSelectScreenEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LevelSelectScreenController controller = (LevelSelectScreenController)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Level Select Utility Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("Auto-Assign UI Toolkit Assets"))
            {
                AutoAssignAssets(controller);
            }

            if (GUILayout.Button("Rebuild Level Grid"))
            {
                controller.BuildLevelGrid();
                EditorUtility.SetDirty(controller);
            }
        }

        private static void AutoAssignAssets(LevelSelectScreenController controller)
        {
            SerializedObject serializedObject = new SerializedObject(controller);
            
            SerializedProperty uxmlProp = serializedObject.FindProperty("_levelSelectScreenAsset");
            if (uxmlProp != null && uxmlProp.objectReferenceValue == null)
            {
                VisualTreeAsset levelSelectAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/LevelSelectScreen.uxml");
                if (levelSelectAsset == null)
                {
                    levelSelectAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/LevelSelect.uxml");
                }
                if (levelSelectAsset != null)
                {
                    uxmlProp.objectReferenceValue = levelSelectAsset;
                }
            }

            SerializedProperty cardProp = serializedObject.FindProperty("_levelCardItemAsset");
            if (cardProp != null && cardProp.objectReferenceValue == null)
            {
                VisualTreeAsset cardAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/LevelCardItem.uxml");
                if (cardAsset != null)
                {
                    cardProp.objectReferenceValue = cardAsset;
                }
            }

            SerializedProperty uiDocProp = serializedObject.FindProperty("_uiDocument");
            if (uiDocProp != null && uiDocProp.objectReferenceValue == null)
            {
                UIDocument doc = controller.GetComponent<UIDocument>();
                if (doc != null)
                {
                    uiDocProp.objectReferenceValue = doc;
                }
            }

            serializedObject.ApplyModifiedProperties();
            controller.InitializeUI();
            Debug.Log("[LevelSelectScreenEditor] Auto-assigned UI Toolkit assets to LevelSelectScreenController.");
        }

        [MenuItem("Tools/Hyper-Casual Runner/Setup Level Select Screen")]
        [MenuItem("GameObject/UI/UI Toolkit Level Select Screen", false, 10)]
        public static void CreateLevelSelectScreenObject()
        {
            GameObject existing = GameObject.Find("LevelSelectScreen");
            if (existing != null)
            {
                Selection.activeGameObject = existing;
                Debug.Log("[LevelSelectScreenEditor] Selected existing LevelSelectScreen GameObject.");
                return;
            }

            GameObject go = new GameObject("LevelSelectScreen");
            Undo.RegisterCreatedObjectUndo(go, "Create Level Select Screen");

            UIDocument uiDoc = go.AddComponent<UIDocument>();
            VisualTreeAsset screenAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/LevelSelectScreen.uxml");
            if (screenAsset == null)
            {
                screenAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/LevelSelect.uxml");
            }
            uiDoc.visualTreeAsset = screenAsset;

            LevelSelectScreenController controller = go.AddComponent<LevelSelectScreenController>();
            AutoAssignAssets(controller);

            Selection.activeGameObject = go;
            Debug.Log("[LevelSelectScreenEditor] Created and configured LevelSelectScreen GameObject.");
        }
    }
}
