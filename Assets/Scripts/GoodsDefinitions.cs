public enum PrizeSeries { SERIES_0, SERIES_1, SERIES_2, SERIES_3, SERIES_4, SERIES_5, SERIES_6, SERIES_7, SERIES_8, SERIES_9 }

public enum PrizeRarity { Normal = 0, Rare = 1, Legendary = 2 }

[System.Serializable]
public struct GoodsData
{
    public PrizeSeries Series;
    public PrizeRarity Rarity;
    // 넘버링: 희귀도 * 10 + 시리즈 번호 (예: 전설(2) + 시리즈3 = 23)
    public int BasePrice => 2500; // 추가: 이 굿즈의 기본 판매 가격
    public int Numbering => (int)Rarity * 10 + (int)Series;
}