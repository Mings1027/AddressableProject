using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "MonsterDatabase", menuName = "Game Data/Monster Database")]
public class MonsterDatabaseSO : ScriptableObject
{
    [Header("Chapter Settings")] [Tooltip("이 데이터베이스가 담당하는 챕터의 라벨 (예: Chapter1)")]
    public string chapterLabel;

    [Header("Monster References (Addressables)")] [Tooltip("일반 몬스터 리스트. 인덱스 순서대로 스테이지에 등장합니다.")] [SerializeField]
    private List<AssetReferenceGameObject> stageMonsters;

    [Tooltip("챕터 보스 몬스터")] [SerializeField]
    private AssetReferenceGameObject chapterBoss;

    [Tooltip("특수 몬스터 (침투/이벤트 등)")] [SerializeField]
    private List<AssetReferenceGameObject> infiltrationMonsters;

    /// <summary>
    /// 특정 인덱스의 일반 몬스터 참조(Reference)를 반환합니다.
    /// </summary>
    public AssetReferenceGameObject GetMonsterReference(int index)
    {
        if (stageMonsters == null || stageMonsters.Count == 0)
        {
            Debug.LogError($"[MonsterDB] 몬스터 리스트가 비어있습니다! ({name})");
            return null;
        }

        // 인덱스 범위 체크 (안전 장치)
        if (index < 0 || index >= stageMonsters.Count)
        {
            Debug.LogWarning($"[MonsterDB] 요청한 인덱스({index})가 범위를 벗어났습니다. 0번 몬스터를 반환합니다.");
            return stageMonsters[0];
        }

        return stageMonsters[index];
    }

    /// <summary>
    /// 챕터 보스 참조를 반환합니다.
    /// </summary>
    public AssetReferenceGameObject GetBossReference()
    {
        if (chapterBoss == null)
        {
            Debug.LogError($"[MonsterDB] 보스 데이터가 설정되지 않았습니다! ({name})");
        }

        return chapterBoss;
    }

    /// <summary>
    /// 침투(특수) 몬스터 참조를 반환합니다.
    /// </summary>
    public AssetReferenceGameObject GetInfiltrationMonsterReference(int index)
    {
        if (infiltrationMonsters == null || index < 0 || index >= infiltrationMonsters.Count)
        {
            return null;
        }

        return infiltrationMonsters[index];
    }

    /// <summary>
    /// [최적화용] 풀링 시스템 등록을 위해 모든 몬스터 리스트를 반환합니다.
    /// (일반, 보스, 특수 몬스터 전부 포함)
    /// </summary>
    public List<AssetReferenceGameObject> GetAllMonstersForPooling()
    {
        // 새로운 리스트를 만들어서 모든 종류를 합쳐서 리턴
        List<AssetReferenceGameObject> allMonsters = new List<AssetReferenceGameObject>();

        if (stageMonsters != null)
            allMonsters.AddRange(stageMonsters);

        if (chapterBoss != null)
            allMonsters.Add(chapterBoss);

        if (infiltrationMonsters != null)
            allMonsters.AddRange(infiltrationMonsters);

        return allMonsters;
    }

    public List<AssetReferenceGameObject> GetAllMonsters()
    {
        return stageMonsters; // 리스트 전체 반환
    }
}