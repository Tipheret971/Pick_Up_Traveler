using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int day;
    public int money;
    public int hunger;
    public int hungerDebuff;
    public int dailyFoodCount;
    public int affection;
    public int day24Choice;
    public bool visitedSugaShrine;
    public bool isTsukinagaSpecialEnded;
    public float resaleMultiplier;
    public float priceDiscountMultiplier;
    public bool hasHormoneBuff;
    public bool hasOmuriceBuff;
    public bool hasSobaBuff;
    public bool hasShrineDiscountBuff;
    public bool hasShrineResaleBuff;
    public bool hasShrineLuckyItemBuff;
    public bool hasPrayedToday;

    // 추가: 인벤토리의 GoodsData 리스트도 함께 저장
    public List<GoodsData> inventory;

    public float globalPriceMultiplier; // 추가
}