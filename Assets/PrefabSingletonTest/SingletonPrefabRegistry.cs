using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Singleton/PrefabRegistry", fileName = "SingletonPrefabRegistry")]
public class SingletonPrefabRegistry : ScriptableObject
{
    [SerializeField] private List<GameObject> prefabs = new();

    public IReadOnlyList<GameObject> Prefabs => prefabs;
}