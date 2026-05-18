using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic; // List를 사용하기 위해 추가

public class ResaleShopUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform contentParent; // Scroll View의 Content 오브젝트
    [SerializeField] private GameObject itemTemplatePrefab; // InventoryItemTemplate 프리팹

    // Money UI 참조: 판매 후 즉시 업데이트를 위해 필요
    [SerializeField] private MoneyCounterUI moneyCounter;

    [Header("Resale Bonus Display")] // 새로 추가
    [SerializeField] private TextMeshProUGUI bonusSeriesText1;
    [SerializeField] private TextMeshProUGUI bonusSeriesText2;

    [Header("Character Dialogue")] // 새로 추가
    [SerializeField] private TextMeshProUGUI characterDialogueText; // 말풍선 속 텍스트 컴포넌트
    [SerializeField]
    [TextArea(3, 6)]
    private List<string> randomDialogues = new List<string> // 랜덤 출력할 대사 목록
    {
        "어서 오세요, 손님!",
        "아래 보너스 상품들은\n비싸게 매입해요~",
        "인벤토리의 상품은\n클릭 즉시 판매되니\n주의하세요!",
        "오늘은 어떤 상품을\n가져오셨나요?",
        "테두리 색깔요?\n파란색은 희귀,\n노란색은 전설이에요!"
    };


    void OnEnable()
    {
        // 씬 활성화 시 인벤토리를 다시 로드
        DisplayInventory();
        // OnEnable 시 보너스 정보도 함께 표시하도록 호출
        DisplayBonusInformation();
        // DisplayBonusSeries();
        // 추가: 캐릭터 대사를 랜덤으로 출력
        DisplayRandomDialogue();
    }

    // 랜덤 대사 출력 함수
    private void DisplayRandomDialogue()
    {
        if (characterDialogueText == null || randomDialogues.Count == 0) return;
    
        // 1. 대사 목록에서 인덱스를 랜덤으로 선택
        int randomIndex = Random.Range(0, randomDialogues.Count);
    
        // 2. 텍스트 컴포넌트에 대사 할당
        characterDialogueText.text = randomDialogues[randomIndex];
    }

    // 인벤토리 목록을 생성하고 표시
    private void DisplayInventory()
    {
        if (GameManager.Instance == null || itemTemplatePrefab == null) return;

        // 이전 목록 제거
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 인벤토리 목록 가져오기
        var inventoryList = GameManager.Instance.inventory;

        for (int i = 0; i < inventoryList.Count; i++)
        {
            GoodsData goods = inventoryList[i];

            // 템플릿 복제 및 부모 설정
            GameObject itemObj = Instantiate(itemTemplatePrefab, contentParent);
            itemObj.SetActive(true);

            // 이미지 컴포넌트 참조 (프리팹 구조에 따라 GetComponentsInChildren 사용)
            Image[] images = itemObj.GetComponentsInChildren<Image>();

            // VisualsManager를 통해 이미지 가져오기
            if (VisualsManager.Instance != null && images.Length >= 2)
            {
                // 가정: images[1] = 굿즈 시리즈 이미지, images[2] = 희귀도 테두리 이미지
                // (프리팹 구조를 확인하고 인덱스 조정 필요)

                // 굿즈 시리즈 이미지 연결
                Sprite seriesSprite = VisualsManager.Instance.GetSeriesSprite(goods.Series);
                if (seriesSprite != null)
                {
                    // images[1]이 굿즈 이미지라고 가정
                    images[1].sprite = seriesSprite;
                }

                // 희귀도 테두리 이미지 연결
                Sprite borderSprite = VisualsManager.Instance.GetRarityBorder(goods.Rarity);
                if (borderSprite != null)
                {
                    // images[2]이 테두리 이미지라고 가정
                    images[2].sprite = borderSprite;
                    // 테두리 이미지의 색상이나 활성화/비활성화 로직을 추가할 수 있습니다.
                }
            }

            // 굿즈 가격 계산
            // 핵심 수정: GoodsData 안에 저장된 BasePrice 사용
            int basePrice = goods.BasePrice;
            int resalePrice = GameManager.Instance.CalculateFinalResalePrice(goods, basePrice);

            // 굿즈 이름/시리즈를 위한 TMP 컴포넌트 참조
            TextMeshProUGUI[] tmps = itemObj.GetComponentsInChildren<TextMeshProUGUI>();

            // UI 요소 업데이트
            if (tmps.Length > 0)
            {
                // tmps[0]에는 굿즈 이름 + 희귀도 + 넘버링을 표시합니다.
                string seriesName = GameManager.Instance.GetSeriesName(goods.Series); // 새 함수 호출

                tmps[0].text =
                    $"{seriesName}"; // 이름 표시
                // {goods.Numbering} -> 넘버링 표시, 제외함
                // \n{goods.Rarity} -> 다음줄로 넘겨 등급 표시, 어울리지 않아 제외
            }

            itemObj.GetComponentsInChildren<TextMeshProUGUI>()[1].text =
                $"¥{resalePrice:N0}"; // 두 번째 Text 컴포넌트가 가격이라고 가정

            // 버튼에 판매 함수 연결
            Button btn = itemObj.GetComponent<Button>();
            int indexToRemove = i; // 클로저가 정확한 인덱스를 참조하도록 복사

            btn.onClick.AddListener(() => SellGoods(indexToRemove, resalePrice));
        }
    }

    // 새로 추가: 보너스 정보를 표시하는 함수
    private void DisplayBonusInformation()
    {
        if (GameManager.Instance == null) return;

        // GameManager에서 오늘 보너스 시리즈 번호를 가져옵니다.
        PrizeSeries bonus1 = GameManager.Instance.bonusSeries1;
        PrizeSeries bonus2 = GameManager.Instance.bonusSeries2;

        // GameManager에 구현된 GetSeriesName 함수를 사용하여 이름을 가져옵니다.
        string name1 = GameManager.Instance.GetSeriesName(bonus1);
        string name2 = GameManager.Instance.GetSeriesName(bonus2);

        // UI에 표시합니다.
        if (bonusSeriesText1 != null)
        {
            bonusSeriesText1.text = $"{name1}";
        }
        if (bonusSeriesText2 != null)
        {
            bonusSeriesText2.text = $"{name2}";
        }
    }

    // 굿즈 판매 로직
    private void SellGoods(int index, int price)
    {
        if (GameManager.Instance == null || index >= GameManager.Instance.inventory.Count) return;

        // 1. 돈 증가
        GameManager.Instance.money += price;

        // 2. 인벤토리에서 제거
        GoodsData soldItem = GameManager.Instance.inventory[index];
        GameManager.Instance.inventory.RemoveAt(index);

        Debug.Log($"[리셀] {soldItem.Series} 판매 완료! +¥{price:N0} 획득.");

        // 3. UI 업데이트
        moneyCounter.UpdateMoneyDisplay();
        DisplayInventory(); // 목록 새로고침
    }
}