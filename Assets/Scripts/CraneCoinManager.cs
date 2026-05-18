using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CraneCoinManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI coinCountText;
    [SerializeField] private Button coinInputButton;

    // 핵심: 씬 리로드 시 코인 개수를 임시 저장하기 위한 정적 변수 (Reset 시 유지됨)
    private static int pendingCoins = 0;

    private int coinCost = 200;

    private int currentCoins = 0;

    void Awake()
    {
        // 코인 투입 버튼에 리스너 연결
        if (coinInputButton != null)
        {
            coinInputButton.onClick.AddListener(AddCoin);
        }
    }

    void OnEnable()
    {
        // 씬 로드 시 코인 초기화/복구 로직
        if (pendingCoins > 0)
        {
            // 리셋 버튼으로 씬이 재로드된 경우: 코인 복구
            currentCoins = pendingCoins;
            // pendingCoins는 Map 씬에서 진입할 때 ResetPendingCoins()를 통해 초기화됩니다.

            Debug.Log($"코인 복구 완료. 현재 코인: {currentCoins}");
        }
        // else: 일반적인 씬 로드 (Map 씬에서 진입): currentCoins = 0 (클래스 초기값 사용)

        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.gameDay == 1)
            {
                coinCost = 100; // 1일차에는 100엔
                Debug.Log($"[이벤트] 1일차 첫 방문 할인 적용! 코인 비용: ¥{coinCost}");
            }
            else
            {
                coinCost = 200; // 그 외에는 정상가 200엔
            }
        }

        UpdateCoinUI();
        UpdateInputButtonState();
    }

    // 코인 투입 버튼이 호출할 함수
    public void AddCoin()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.money >= coinCost)
        {
            // 1. 돈 차감
            GameManager.Instance.money -= coinCost;

            // 2. 코인 증가
            currentCoins++;

            // 3. UI 업데이트 및 버튼 상태 갱신
            UpdateCoinUI();
            UpdateInputButtonState();

            // MoneyCounterUI가 있다면 여기서 업데이트 함수 호출
            MoneyCounterUI moneyCounter = UnityEngine.Object.FindFirstObjectByType<MoneyCounterUI>();
            if (moneyCounter != null) moneyCounter.UpdateMoneyDisplay();

            Debug.Log($"코인 투입 완료. -¥{coinCost}, 현재 코인: {currentCoins}");
        }
        else
        {
            Debug.Log("잔액 부족: 코인을 투입할 수 없습니다.");
            coinInputButton.interactable = false; // 상호작용 비활성화
        }
    }

    // 크레인 암 컨트롤 스크립트가 호출할 함수
    public bool CanPlayAndDeductCoin()
    {
        if (currentCoins > 0)
        {
            currentCoins--;
            UpdateCoinUI();

            Debug.Log("게임 시작! 코인 1개 차감.");
            return true;
        }

        Debug.Log("코인 부족: 크레인 게임을 시작할 수 없습니다.");
        return false;
    }

    // 리셋 버튼 클릭 시 코인 상태를 보존하기 위해 호출할 함수
    public void PrepareForReset()
    {
        pendingCoins = currentCoins;
        Debug.Log($"리셋 준비: {pendingCoins} 코인 임시 저장 완료.");
    }

    // UI 텍스트 포맷 수정: "남은 기회 : X" 형식으로 변경
    private void UpdateCoinUI()
    {
        if (coinCountText != null)
        {
            coinCountText.text = $"남은 기회: {currentCoins}";
        }
    }

    private void UpdateInputButtonState()
    {
        if (coinInputButton == null || GameManager.Instance == null) return;

        // 돈이 200엔 이상일 때만 코인 투입 버튼 활성화
        coinInputButton.interactable = GameManager.Instance.money >= coinCost;
    }

    // Map 씬으로 돌아갈 때 코인 상태를 0으로 초기화하는 함수 추가
    // 이 함수는 SceneLoader.ReturnToMapScene이 호출될 때 실행되어야 합니다.
    public static void ResetPendingCoins()
    {
        pendingCoins = 0;
        Debug.Log("Map 씬 복귀: pendingCoins를 0으로 초기화했습니다.");
    }
}