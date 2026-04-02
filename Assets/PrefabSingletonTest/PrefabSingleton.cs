using UnityEngine;

public abstract class PrefabSingleton<T> : MonoBehaviour where T : PrefabSingleton<T>
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindAnyObjectByType<T>();
            if (_instance != null) return _instance;

            // Registry에서 프리팹 찾기
            if (PrefabSingletonLocator.TryGet(typeof(T), out var prefab))
            {
                var go = Object.Instantiate(prefab);
                go.name = typeof(T).Name;
                _instance = go.GetComponent<T>();
            }
            else
            {
                Debug.LogWarning($"[PrefabSingleton] {typeof(T).Name} 프리팹이 Registry에 등록되지 않았습니다. 빈 오브젝트로 폴백합니다.");
                var go = new GameObject(typeof(T).Name);
                _instance = go.AddComponent<T>();
            }

            return _instance;
        }
    }

    [SerializeField] private bool dontDestroyOnLoad;

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = (T)this;
        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
}