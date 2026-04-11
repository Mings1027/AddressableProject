using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class RotateObjects : MonoBehaviour
{
    [SerializeField] private float speed = 10;
    [SerializeField] private AssetReference assetReference;
    private void Start() { }

    private void Update()
    {
        transform.RotateAround(Vector3.zero, Vector3.up, Time.deltaTime * speed);
    }

    private async UniTask Load()
    {
        var obj = await AddressableManager.Instance.LoadAsync<GameObject>(assetReference, this);
    }

    private async UniTask Get()
    {
        var obj = AddressableManager.Instance.Get<GameObject>(assetReference);
    }

    private async UniTask Unload()
    {
        AddressableManager.Instance.ReleaseByOwner(this);
    }
}