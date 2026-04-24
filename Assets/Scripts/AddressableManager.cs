using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public class AddressableManager
{
    private static AddressableManager _instance;
    public static AddressableManager Instance => _instance ??= new AddressableManager();

    private bool _showLog;

    private readonly Dictionary<AssetReference, (AsyncOperationHandle handle, int refCount)> _loaded = new();

#if UNITY_EDITOR
    public IReadOnlyDictionary<AssetReference, (AsyncOperationHandle handle, int refCount)> DebugLoaded => _loaded;
#endif

    public async UniTask<T> LoadAsync<T>(AssetReference assetRef) where T : Object
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
            return existing.handle.Result as T;
        }

        if (assetRef.OperationHandle.IsValid())
        {
            var existingHandle = assetRef.OperationHandle;

            if (!existingHandle.IsDone)
                await existingHandle.ToUniTask();

            if (existingHandle.Result is not T)
            {
                Debug.LogError($"[Addressable] Type mismatch: {assetRef.RuntimeKey}");
                return null;
            }

            _loaded[assetRef] = (existingHandle, 1);
            return existingHandle.Result as T;
        }

        var handle = assetRef.LoadAssetAsync<T>();

        if (!handle.IsValid())
        {
            Debug.LogWarning($"[Addressable] Load asset failed: {assetRef.RuntimeKey}");
            return null;
        }

        await handle.ToUniTask();

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[Addressable] Load failed: {assetRef.RuntimeKey}");
            return null;
        }

        _loaded[assetRef] = (handle, 1);
        if (_showLog)
        {
            Debug.Log($"[Addressable] Loaded: {assetRef.RuntimeKey} ({typeof(T).Name})");
        }

        return handle.Result;
    }

    public T LoadSync<T>(AssetReference assetRef) where T : Object
    {
        if (assetRef == null || !assetRef.RuntimeKeyIsValid()) return null;

        if (_loaded.TryGetValue(assetRef, out var existing))
        {
            _loaded[assetRef] = (existing.handle, existing.refCount + 1);
            return existing.handle.Result as T;
        }

        var handle = assetRef.LoadAssetAsync<T>();
        var result = handle.WaitForCompletion();
        _loaded[assetRef] = (handle, 1);
        return result;
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

    public async UniTask<T> GetOrLoadAsync<T>(AssetReference assetRef) where T : Object
    {
        return Get<T>(assetRef) ?? await LoadAsync<T>(assetRef);
    }


    public void Release(AssetReference assetRef)
    {
        if (assetRef == null) return;

        if (!_loaded.TryGetValue(assetRef, out var entry)) return;

        var newCount = entry.refCount - 1;
        if (newCount <= 0)
        {
            assetRef.ReleaseAsset();
            _loaded.Remove(assetRef);
        }
        else
        {
            _loaded[assetRef] = (entry.handle, newCount);
        }
    }

    public void ReleaseAll()
    {
        foreach (var assetRef in _loaded.Keys)
        {
            assetRef.ReleaseAsset();
        }

        _loaded.Clear();
    }

    public void Log() => _instance.LogStatus();
    
    // [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void LogStatus()
    {
        Debug.Log($"[Addressable] Loaded assets: {_loaded.Count}");
        foreach (var (assetRef, (handle, refCount)) in _loaded)
            Debug.Log($"  {assetRef.RuntimeKey} | refCount={refCount} | valid={handle.IsValid()}");
    }
}
