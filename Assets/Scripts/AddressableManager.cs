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
    readonly Dictionary<object, HashSet<AssetReference>> _ownerRefs = new();

    void Awake() => Instance = this;

    public async UniTask<T> LoadAsync<T>(AssetReference assetRef, object owner) where T : Object
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

        TrackOwner(owner, assetRef);
        return _loaded[assetRef].handle.Result as T;
    }

    public T Get<T>(AssetReference assetRef) where T : Object
    {
        if (assetRef != null && _loaded.TryGetValue(assetRef, out var entry))
            return entry.handle.Result as T;
        return null;
    }

    public void ReleaseByOwner(object owner)
    {
        if (!_ownerRefs.Remove(owner, out var refs))
            return;

        foreach (var assetRef in refs)
            Release(assetRef);
    }

    public void ReleaseAll()
    {
        foreach (var entry in _loaded)
            Addressables.Release(entry.Value.handle);
        _loaded.Clear();
        _ownerRefs.Clear();
    }

    void Release(AssetReference assetRef)
    {
        if (!_loaded.TryGetValue(assetRef, out var entry))
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

    void TrackOwner(object owner, AssetReference assetRef)
    {
        if (!_ownerRefs.TryGetValue(owner, out var set))
        {
            set = new HashSet<AssetReference>();
            _ownerRefs[owner] = set;
        }
        set.Add(assetRef); // HashSet이라 중복 안 쌓임
    }
}
