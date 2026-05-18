using UnityEngine;
using System.Collections.Generic;

// 굿즈 정의 스크립트의 enum을 사용합니다.
// (PrizeSeries, PrizeRarity 정의가 필요합니다.) 

public class VisualsManager : MonoBehaviour
{
    // 싱글톤 패턴으로 쉽게 접근할 수 있도록 설정합니다.
    public static VisualsManager Instance { get; private set; }

    [Header("Goods Series Images (0-9)")]
    // PrizeSeries enum 값을 키로, Sprite를 값으로 가집니다.
    [SerializeField]
    private List<SeriesSpriteMapping> seriesSprites = new List<SeriesSpriteMapping>();
    private Dictionary<PrizeSeries, Sprite> seriesMap = new Dictionary<PrizeSeries, Sprite>();

    [Header("Rarity Border Images")]
    // PrizeRarity enum 값을 키로, Sprite를 값으로 가집니다.
    [SerializeField]
    private List<RaritySpriteMapping> rarityBorders = new List<RaritySpriteMapping>();
    private Dictionary<PrizeRarity, Sprite> rarityMap = new Dictionary<PrizeRarity, Sprite>();

    // --- 유니티 Inspector에 표시될 매핑 구조체 ---
    [System.Serializable]
    private struct SeriesSpriteMapping
    {
        public PrizeSeries Series;
        public Sprite Sprite;
    }

    [System.Serializable]
    private struct RaritySpriteMapping
    {
        public PrizeRarity Rarity;
        public Sprite Sprite;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 씬 이동 시 파괴되지 않도록 설정 (GameManager처럼 영속성을 가집니다)
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Inspector에 설정된 List 데이터를 Dictionary로 변환하여 사용 준비
        BuildDictionaries();
    }

    private void BuildDictionaries()
    {
        seriesMap.Clear();
        foreach (var item in seriesSprites)
        {
            seriesMap[item.Series] = item.Sprite;
        }

        rarityMap.Clear();
        foreach (var item in rarityBorders)
        {
            rarityMap[item.Rarity] = item.Sprite;
        }
    }

    // --- 외부에서 호출될 Get 함수 ---

    public Sprite GetSeriesSprite(PrizeSeries series)
    {
        if (seriesMap.TryGetValue(series, out Sprite sprite))
        {
            return sprite;
        }
        return null; // 이미지를 찾지 못했을 경우
    }

    public Sprite GetRarityBorder(PrizeRarity rarity)
    {
        if (rarityMap.TryGetValue(rarity, out Sprite sprite))
        {
            return sprite;
        }
        return null; // 이미지를 찾지 못했을 경우
    }
}