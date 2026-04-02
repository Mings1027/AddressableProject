using System;
using System.Collections.Generic;
using UnityEngine;

public static class PrefabSingletonLocator
{
    private static readonly Dictionary<Type, GameObject> _map = new();

    public static void Register(SingletonPrefabRegistry registry)
    {
        _map.Clear();

        foreach (var prefab in registry.Prefabs)
        {
            if (prefab == null) continue;

            // 프리팹의 컴포넌트 중 PrefabSingleton 서브타입을 찾아 키로 등록
            foreach (var component in prefab.GetComponents<MonoBehaviour>())
            {
                var type = component.GetType();
                if (IsPrefabSingleton(type))
                {
                    _map[type] = prefab;
                    break;
                }
            }
        }
    }

    public static bool TryGet(Type type, out GameObject prefab)
        => _map.TryGetValue(type, out prefab);

    private static bool IsPrefabSingleton(Type type)
    {
        while (type != null && type != typeof(MonoBehaviour))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(PrefabSingleton<>))
                return true;
            type = type.BaseType;
        }
        return false;
    }
}