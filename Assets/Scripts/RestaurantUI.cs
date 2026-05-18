using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RestaurantUI : MonoBehaviour
{
    [Header("UI/Scene References")]
    [SerializeField] private Button[] menuButtons; // 모든 메뉴 버튼 배열
    [SerializeField] private TextMeshProUGUI statusText; // 상태 메시지 표시
    [SerializeField] private string mapSceneName = "Map";

    private string[] menuNames = { "카이센동", "돈카츠", "호르몬동", "오므라이스", "소바" };
    private int[] menuPrices = { 2000, 1800, 1500, 1200, 900 };

    [Header("Speech Bubble")]
    [SerializeField] private TextMeshProUGUI speechBubbleText; // 말풍선 텍스트 컴포넌트

    // 식당 멘트 5가지 정의
    private readonly string[] RESTAURANT_MESSAGES = new string[]
    {
        "어서오세요!",
        "식사 한 끼에 배고픔은\n[3]씩 회복됩니다~",
        "같은 메뉴의 버프는\n중복적용되지 않으니\n주의하셔야 합니다!",
        "필요한 것 있으시면\n편하게 말씀하세요~",
        "음식 나왔습니다~\n맛있게 드세요!"
    };

    void Start()
    {
        // 버튼에 리스너 연결 (Awake에서 하는 것이 더 좋지만 Start에서도 가능)
        for (int i = 0; i < menuButtons.Length; i++)
        {
            int index = i; // 클로저용 복사
            menuButtons[i].onClick.AddListener(() => PurchaseMenu(index));
        }
    }

    void OnEnable()
    {
        // 씬 활성화 시 가격 및 버튼 상태 업데이트
        UpdateMenuPricesAndState();
        // 씬 진입 시마다 멘트 출력
        DisplayRandomMessage();
    }

    private void UpdateMenuPricesAndState()
    {
        if (GameManager.Instance == null) return;

        float discountMultiplier = GameManager.Instance.GetCurrentPriceDiscount();

        for (int i = 0; i < menuButtons.Length; i++)
        {
            int originalPrice = menuPrices[i];
            // 할인 적용
            int discountedPrice = Mathf.RoundToInt(originalPrice * discountMultiplier);

            // 버튼 텍스트 업데이트 (가격 표시)
            menuButtons[i].GetComponentInChildren<TextMeshProUGUI>().text =
                $"{menuNames[i]}: ¥{discountedPrice:N0}";

            // 돈이 부족하거나 식당 횟수를 모두 사용했으면 버튼 비활성화
            bool canAfford = GameManager.Instance.money >= discountedPrice;
            bool withinLimit = GameManager.Instance.dailyFoodCount < GameManager.Instance.dailyRestaurantCount;

            menuButtons[i].interactable = canAfford && withinLimit;
        }

        // 상태 텍스트 업데이트
        statusText.text = $"오늘 남은 식사 횟수: {GameManager.Instance.dailyRestaurantCount - GameManager.Instance.dailyFoodCount}회";
    }

    private void PurchaseMenu(int index)
    {
        if (GameManager.Instance == null) return;
        var gm = GameManager.Instance;

        float discountMultiplier = GameManager.Instance.GetCurrentPriceDiscount();
        int originalPrice = menuPrices[index];
        int finalPrice = Mathf.RoundToInt(originalPrice * discountMultiplier);
        string menuName = menuNames[index];

        // 1. 가격 및 횟수 최종 체크
        if (GameManager.Instance.money < finalPrice || GameManager.Instance.dailyFoodCount >= GameManager.Instance.dailyRestaurantCount)
        {
            Debug.Log("구매 실패: 돈 부족 또는 횟수 초과.");
            return;
        }

        // 2. 구매 및 버프 적용
        GameManager.Instance.money -= finalPrice;
        // 핵심 수정: ApplyRestaurantBuff 호출 시, HP 회복과 버프 적용을 GameManager가 내부에서 처리 (중복 체크 포함)
        gm.ApplyRestaurantBuff(menuName);

        // 3. UI 업데이트 (돈 UI, Hunger UI, Buff UI 모두 업데이트 필요)

        // MoneyCounterUI 업데이트
        MoneyCounterUI moneyUI = UnityEngine.Object.FindFirstObjectByType<MoneyCounterUI>();
        if (moneyUI != null) moneyUI.UpdateMoneyDisplay();

        // HungerUI 업데이트 (GameManager에서 이미 처리하지만 안전을 위해 호출)
        HungerUI hungerUI = UnityEngine.Object.FindFirstObjectByType<HungerUI>();
        if (hungerUI != null) hungerUI.UpdateHungerDisplay();

        // BuffUI 업데이트
        BuffUI buffUI = UnityEngine.Object.FindFirstObjectByType<BuffUI>();
        if (buffUI != null) buffUI.UpdateBuffDisplay();

        // 4. 식사 후 씬 이동 처리
        SceneLoader sceneLoader = UnityEngine.Object.FindFirstObjectByType<SceneLoader>();
        if (sceneLoader == null) return;

        if (menuName == "오므라이스")
        {
            // 오므라이스 로직 시작

            // 20% 확률 체크
            if (UnityEngine.Random.value < 0.2f)
            {
                // 20% 확률 (실패): 버프 소멸 및 맵으로 복귀
                gm.hasOmuriceBuff = false; // 버프 소멸
                Debug.Log("오므라이스 버프 소멸. 랜덤 이벤트는 발생하지 않았습니다.");
                sceneLoader.ReturnToMapScene(mapSceneName);
            }
            else
            {
                // 80% 확률 (성공): VN 씬으로 이동 (버프는 VN 씬 종료 시 소멸)
                Debug.Log("오므라이스 버프 발동! 랜덤 이벤트 씬으로 이동합니다.");
                sceneLoader.LoadOmuriceVNScene(); // 새로운 함수 호출
            }
        }
        else
        {
            // 오므라이스가 아닌 메뉴 (일반 복귀)
            sceneLoader.ReturnToMapScene(mapSceneName);
        }
    }

    // 새로 추가: 5가지 멘트 중 하나를 랜덤으로 출력하는 함수
    private void DisplayRandomMessage()
    {
        if (speechBubbleText == null)
        {
            Debug.LogError("말풍선 텍스트 컴포넌트가 연결되지 않았습니다.");
            return;
        }

        // 1. 랜덤 인덱스 선택
        int randomIndex = Random.Range(0, RESTAURANT_MESSAGES.Length);

        // 2. 말풍선 텍스트 업데이트
        speechBubbleText.text = RESTAURANT_MESSAGES[randomIndex];
    }
}