using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConvenienceUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button[] menuButtons; // 모든 메뉴 버튼 배열
    [SerializeField] private TextMeshProUGUI speechBubbleText; // 말풍선 텍스트 컴포넌트

    // 식당 멘트 5가지 정의
    private readonly string[] CONVENIENCE_MESSAGES = new string[]
    {
        "어서오세요",
        "계산은 이쪽에서\n도와드리겠습니다",
        "필요한 것 있으시면\n말씀해주세요",
        "봉투 필요하신가요?"
    };

    // 편의점 메뉴 정의: (이름, 가격, HP 회복량)
    private readonly (string name, int price, int hpRestore)[] CONVENIENCE_MENUS =
    {
        ("샌드위치", 250, 2),
        ("오뎅", 400, 2),
        ("컵라면", 150, 2),
        ("도시락", 500, 2),
        ("아이스크림", 200, 1),
        ("푸딩", 200, 1),
        ("과일 스무디", 300, 1),
        ("감자칩", 150, 1)
    };

    void Awake()
    {
        // 버튼에 리스너 연결
        for (int i = 0; i < menuButtons.Length; i++)
        {
            int index = i; // 클로저용 복사
            // 버튼의 개수가 메뉴 개수와 일치한다고 가정합니다.
            menuButtons[i].onClick.AddListener(() => PurchaseConvenienceFood(index));
        }
    }

    void OnEnable()
    {
        UpdateMenuPricesAndState();
        DisplayRandomMessage();
    }

    private void UpdateMenuPricesAndState()
    {
        if (GameManager.Instance == null) return;

        float discountMultiplier = GameManager.Instance.GetCurrentPriceDiscount();

        for (int i = 0; i < menuButtons.Length; i++)
        {
            var menu = CONVENIENCE_MENUS[i];
            int originalPrice = menu.price;
            int discountedPrice = Mathf.RoundToInt(originalPrice * discountMultiplier);

            // 버튼 텍스트 업데이트 (가격 표시)
            menuButtons[i].GetComponentInChildren<TextMeshProUGUI>().text =
                $"{menu.name}: ¥{discountedPrice:N0} [배고픔 회복: {menu.hpRestore}]";

            // 돈이 부족하면 버튼 비활성화 (횟수 제한 없음)
            bool canAfford = GameManager.Instance.money >= discountedPrice;
            menuButtons[i].interactable = canAfford;
        }
    }

    private void PurchaseConvenienceFood(int index)
    {
        if (GameManager.Instance == null) return;

        var menu = CONVENIENCE_MENUS[index];
        float discountMultiplier = GameManager.Instance.GetCurrentPriceDiscount();
        int finalPrice = Mathf.RoundToInt(menu.price * discountMultiplier);

        if (GameManager.Instance.money < finalPrice)
        {
            Debug.Log("구매 실패: 돈 부족.");
            return;
        }

        // 1. 구매 및 HP 회복 적용
        GameManager.Instance.money -= finalPrice;

        // HP 회복 (편의점은 랜덤이 아닌 고정 회복량)
        GameManager.Instance.RestoreHunger(menu.hpRestore);

        Debug.Log($"편의점 식사 완료: {menu.name}. -¥{finalPrice}");

        // 2. UI 업데이트
        UpdateMenuPricesAndState();

        // Money UI 및 Hunger UI, Buff UI 모두 업데이트 호출
        // (UI 업데이트 헬퍼 함수를 GameManager에 만드는 것이 가장 좋음)
        // 임시로 직접 호출:
        MoneyCounterUI moneyUI = UnityEngine.Object.FindFirstObjectByType<MoneyCounterUI>();
        if (moneyUI != null) moneyUI.UpdateMoneyDisplay();

        HungerUI hungerUI = UnityEngine.Object.FindFirstObjectByType<HungerUI>();
        if (hungerUI != null) hungerUI.UpdateHungerDisplay();

        BuffUI buffUI = UnityEngine.Object.FindFirstObjectByType<BuffUI>();
        if (buffUI != null) buffUI.UpdateBuffDisplay();

        // 구매 후 맵으로 자동 복귀하지 않음 (기획 반영)
    }

    // 식당과 유사한 랜덤 멘트 로직 (이미 구현하셨다고 가정)
    private void DisplayRandomMessage()
    {
        if (speechBubbleText == null)
        {
            Debug.LogError("말풍선 텍스트 컴포넌트가 연결되지 않았습니다.");
            return;
        }

        // 1. 랜덤 인덱스 선택
        int randomIndex = Random.Range(0, CONVENIENCE_MESSAGES.Length);

        // 2. 말풍선 텍스트 업데이트
        speechBubbleText.text = CONVENIENCE_MESSAGES[randomIndex];
    }

    // 맵 복귀 버튼 (UI에 수동으로 SceneLoader 연결)
}