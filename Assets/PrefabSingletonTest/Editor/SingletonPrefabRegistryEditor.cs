#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SingletonPrefabRegistry))]
public class SingletonPrefabRegistryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();

        var registry = (SingletonPrefabRegistry)target;
        bool isPreloaded = IsPreloaded(registry);

        GUI.color = isPreloaded ? Color.green : Color.yellow;
        EditorGUILayout.LabelField(
            isPreloaded ? "✓ Preloaded Assets에 등록됨" : "⚠ Preloaded Assets에 미등록",
            EditorStyles.boldLabel
        );
        GUI.color = Color.white;

        if (!isPreloaded && GUILayout.Button("Preloaded Assets에 등록"))
        {
            AddToPreloadedAssets(registry);
        }
    }

    private static bool IsPreloaded(Object obj)
    {
        var preloaded = PlayerSettings.GetPreloadedAssets();
        foreach (var asset in preloaded)
            if (asset == obj) return true;
        return false;
    }

    private static void AddToPreloadedAssets(Object obj)
    {
        var preloaded = new System.Collections.Generic.List<Object>(
            PlayerSettings.GetPreloadedAssets()
        );
        if (!preloaded.Contains(obj))
        {
            preloaded.Add(obj);
            PlayerSettings.SetPreloadedAssets(preloaded.ToArray());
            Debug.Log($"[PrefabSingleton] {obj.name} → Preloaded Assets 등록 완료");
        }
    }
}
#endif