using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MonsterResourceManager : MonoBehaviour
{
    [SerializeField] private MonsterDatabaseSO monsterDB;
    
    // 챕터 전체 로딩 핸들 (메모리 유지용)
    private AsyncOperationHandle<IList<GameObject>> _chapterHandle;
    
    // 개별 에셋 핸들 저장용 (나중에 해제하기 위해 필요)
    private List<AsyncOperationHandle> _individualHandles = new List<AsyncOperationHandle>();

    public async UniTask LoadChapter(string label)
    {
        UnloadCurrentChapter();

        Debug.Log($"[{label}] 리소스 로딩 시작...");

        _chapterHandle = Addressables.LoadAssetsAsync<GameObject>(label, null);
        await _chapterHandle.ToUniTask();

        foreach (var assetRef in monsterDB.GetAllMonsters()) 
        {
            var handle = assetRef.LoadAssetAsync<GameObject>();
            await handle.ToUniTask();
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject prefab = handle.Result;
                
                // PoolManager.Instance.CreatePool(prefab, 10); 
                
                _individualHandles.Add(handle);
            }
        }
        
        Debug.Log("챕터 로딩 및 풀링 준비 완료!");
    }

    public GameObject SpawnMonster(int index, Transform parent)
    {
        var assetRef = monsterDB.GetMonsterReference(index);
        
        if (assetRef.Asset is GameObject prefab)
        {
            // GameObject monster = PoolManager.Instance.Get(prefab);
            //
            // monster.transform.SetParent(parent);
            // monster.transform.localPosition = Vector3.zero;
            // return monster;
            return null;
        }
        else
        {
            Debug.LogError("몬스터가 로드되지 않았습니다! LoadChapter를 먼저 호출하세요.");
            return null;
        }
    }

    private void UnloadCurrentChapter()
    {
        // PoolManager에 ClearAll() 같은 기능이 있다면 호출
        // PoolManager.Instance.ClearAll(); 

        foreach (var handle in _individualHandles)
        {
            Addressables.Release(handle);
        }
        _individualHandles.Clear();

        if (_chapterHandle.IsValid())
        {
            Addressables.Release(_chapterHandle);
        }
    }
}