using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;
using Object = UnityEngine.Object;

public class AddressableManager : Singleton<AddressableManager>
{
    private readonly Dictionary<AssetReference, (AsyncOperationHandle handle, int refCount)> _loaded = new();
    private readonly Dictionary<object, Dictionary<AssetReference, int>> _ownerRefs = new();

    public async UniTask<T> LoadAsync<T>(AssetReference assetRef, object owner) where T : Object
    {
        if (assetRef == null || !assetRef.RuntimeKeyIsValid())
            return null;

        if (_loaded.TryGetValue(assetRef, out var existing))
        {
            if (existing.handle.Result is not T)
            {
                Debug.LogError($"[Addressable] Type mismatch: {assetRef.RuntimeKey} " +
                               $"(loaded={existing.handle.Result.GetType().Name}, requested={typeof(T).Name})");
                return null;
            }

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
            Debug.Log($"[Addressable] Loaded: {assetRef.RuntimeKey} ({typeof(T).Name})");
        }

        TrackOwner(owner, assetRef);
        return _loaded[assetRef].handle.Result as T;
    }

    public T Get<T>(AssetReference assetRef) where T : Object
    {
        if (assetRef != null
            && _loaded.TryGetValue(assetRef, out var entry)
            && entry.handle.IsValid()
            && entry.handle.Status == AsyncOperationStatus.Succeeded)
            return entry.handle.Result as T;
        return null;
    }

    public void ReleaseByOwner(object owner)
    {
        if (!_ownerRefs.Remove(owner, out var refs))
            return;

        foreach (var (assetRef, count) in refs)
            for (var i = 0; i < count; i++)
                Release(assetRef);
    }

    public void ReleaseAll()
    {
        foreach (var entry in _loaded)
            if (entry.Value.handle.IsValid())
                Addressables.Release(entry.Value.handle);
        _loaded.Clear();
        _ownerRefs.Clear();
    }

    private void Release(AssetReference assetRef)
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

    private void TrackOwner(object owner, AssetReference assetRef)
    {
        if (!_ownerRefs.TryGetValue(owner, out var map))
        {
            map = new Dictionary<AssetReference, int>();
            _ownerRefs[owner] = map;
        }

        map[assetRef] = map.TryGetValue(assetRef, out var c) ? c + 1 : 1;
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void LogStatus()
    {
        Debug.Log($"[Addressable] Loaded assets: {_loaded.Count}, Owners: {_ownerRefs.Count}");
        foreach (var (assetRef, (handle, refCount)) in _loaded)
            Debug.Log($"  {assetRef.RuntimeKey} | refCount={refCount} | valid={handle.IsValid()}");
    }
}
