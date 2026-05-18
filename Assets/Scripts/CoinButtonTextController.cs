using UnityEngine;
using TMPro;

public class CoinButtonTextController : MonoBehaviour
{
    private TextMeshProUGUI buttonText;

    void OnEnable()
    {
        // 1. 버튼 자식이나 본인에게서 TextMeshProUGUI 컴포넌트를 가져옵니다.
        buttonText = GetComponentInChildren<TextMeshProUGUI>();

        if (buttonText == null)
        {
            Debug.LogError("[CoinButtonTextController] TextMeshProUGUI 컴포넌트를 찾을 수 없습니다!");
            return;
        }

        // 2. 날짜를 체크하여 텍스트를 동적으로 변경합니다.
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.gameDay == 1)
            {
                // 1일차 전용 텍스트 (예: 오픈 기념 반값 할인!)
                buttonText.text = "¥100 투입";
            }
            else
            {
                // 평소 가격
                buttonText.text = "¥200 투입";
            }
        }
    }
}