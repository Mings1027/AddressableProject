using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class AddressableManager : MonoBehaviour
{
    [SerializeField] private AssetReferenceGameObject cubeObj;
    [SerializeField] private AssetReferenceAtlasedSprite sprite;
    [SerializeField] private Image image;

    private List<GameObject> gameObjects;

    private void Awake()
    {
        gameObjects = new List<GameObject>();
    }

    private void Start()
    {
        InitAddressable().Forget();
    }

    private void OnDestroy()
    {
        ReleaseObject();
    }

    private async UniTask InitAddressable()
    {
        var init = Addressables.InitializeAsync();
        await init.ToUniTask(cancellationToken: destroyCancellationToken);
    }

    public void SpawnObject()
    {
        cubeObj.InstantiateAsync().Completed += (obj) => { gameObjects.Add(obj.Result); };
    }

    public void SpawnSprite()
    {
        Debug.Log(sprite.IsDone);
        if (sprite.IsValid())
        {
            Debug.Log("Isvalid");
        }
        sprite.LoadAssetAsync().Completed += img => { image.sprite = img.Result; };
    }

    public void ReleaseObject()
    {
        if (gameObjects == null) return;
        if (gameObjects.Count == 0) return;

        sprite.ReleaseAsset();
        image.sprite = null;

        for (int i = gameObjects.Count - 1; i >= 0; i--)
        {
            Addressables.ReleaseInstance(gameObjects[i]);
            gameObjects.RemoveAt(i);
        }
    }
}