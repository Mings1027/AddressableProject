using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public class AddressableManager : MonoBehaviour
{
    public static AddressableManager Instance { get; private set; }

    readonly Dictionary<AssetReference, (AsyncOperationHandle handle, int refCount)> _loaded = new();

    // 소유자별 추적 (엘몬이든 UI든 뭐든)
    readonly Dictionary<object, List<AssetReference>> _ownerRefs = new();

    void Awake() => Instance = this;

    /// <summary>
    /// 단일 에셋 로드
    /// </summary>
    public async UniTask<T> LoadAsync<T>(AssetReference assetRef, object owner = null) where T : Object
    {
        if (assetRef == null || !assetRef.RuntimeKeyIsValid())
            return null;

        if (_loaded.TryGetValue(assetRef, out var existing))
        {
            _loaded[assetRef] = (existing.handle, existing.refCount + 1);
        }
        else
        {
            var handle = assetRef.LoadAssetAsync<T>();
            await handle.ToUniTask();

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[Addressable] Load failed: {assetRef.RuntimeKey}");
                return null;
            }

            _loaded[assetRef] = (handle, 1);
        }

        if (owner != null)
            TrackOwner(owner, assetRef);

        return _loaded[assetRef].handle.Result as T;
    }

    /// <summary>
    /// 여러 에셋 병렬 로드
    /// </summary>
    public async UniTask<T[]> LoadAllAsync<T>(IReadOnlyList<AssetReference> refs, object owner = null) where T : Object
    {
        if (refs == null || refs.Count == 0)
            return Array.Empty<T>();

        var tasks = new UniTask<T>[refs.Count];
        for (int i = 0; i < refs.Count; i++)
            tasks[i] = LoadAsync<T>(refs[i], owner);

        return await UniTask.WhenAll(tasks);
    }

    /// <summary>
    /// 이미 로드된 에셋 가져오기
    /// </summary>
    public T Get<T>(AssetReference assetRef) where T : Object
    {
        if (assetRef != null && _loaded.TryGetValue(assetRef, out var entry))
            return entry.handle.Result as T;

        return null;
    }

    /// <summary>
    /// 단일 에셋 릴리즈
    /// </summary>
    public void Release(AssetReference assetRef)
    {
        if (assetRef == null || !_loaded.TryGetValue(assetRef, out var entry))
            return;

        var newCount = entry.refCount - 1;
        if (newCount <= 0)
        {
            Addressables.Release(entry.handle);
            _loaded.Remove(assetRef);
        }
        else
        {
            _loaded[assetRef] = (entry.handle, newCount);
        }
    }

    /// <summary>
    /// 소유자가 로드한 에셋 전부 릴리즈
    /// </summary>
    public void ReleaseByOwner(object owner)
    {
        if (!_ownerRefs.Remove(owner, out var refs))
            return;

        foreach (var assetRef in refs)
            Release(assetRef);
    }

    /// <summary>
    /// 전부 해제 (씬 전환, 챕터 전환 등)
    /// </summary>
    public void ReleaseAll()
    {
        foreach (var entry in _loaded)
            Addressables.Release(entry.Value.handle);

        _loaded.Clear();
        _ownerRefs.Clear();
    }

    private void TrackOwner(object owner, AssetReference assetRef)
    {
        if (!_ownerRefs.TryGetValue(owner, out var list))
        {
            list = new List<AssetReference>();
            _ownerRefs[owner] = list;
        }

        list.Add(assetRef);
    }
}