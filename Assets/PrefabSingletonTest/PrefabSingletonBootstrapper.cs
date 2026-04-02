using UnityEngine;

internal static class PrefabSingletonBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        var registries = Resources.FindObjectsOfTypeAll<SingletonPrefabRegistry>();

        if (registries.Length == 0)
        {
            Debug.LogWarning("[PrefabSingleton] SingletonPrefabRegistry가 Preloaded Assets에 등록되지 않았습니다.");
            return;
        }

        PrefabSingletonLocator.Register(registries[0]);
        Debug.Log("[PrefabSingleton] Registry 초기화 완료 (BeforeSceneLoad)");
    }
}