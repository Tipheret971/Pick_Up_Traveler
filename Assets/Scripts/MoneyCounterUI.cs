using UnityEngine;
using TMPro;

public class MoneyCounterUI : MonoBehaviour
{
    private TextMeshProUGUI moneyText;
    void Start()
    {
        moneyText = GetComponent<TextMeshProUGUI>();
        // GameManager가 로드되었는지 확인
        if (GameManager.Instance == null) return;

        // 초기 표시 업데이트
        UpdateMoneyDisplay();
    }

    public void UpdateMoneyDisplay()
    {
        if (GameManager.Instance != null && moneyText != null)
        {
            // 돈을 엔화 단위로 표시 (예: ¥10,000)
            moneyText.text = $"¥{GameManager.Instance.money:N0}"; // N0 포맷으로 콤마(,) 표시
        }
    }

    // 씬이 활성화되거나 다시 돌아올 때마다 업데이트
    void OnEnable()
    {
        UpdateMoneyDisplay();
    }
}