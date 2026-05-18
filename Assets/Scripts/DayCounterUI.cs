using UnityEngine;
using TMPro; // 1. TMP 네임스페이스 추가

public class DayCounterUI : MonoBehaviour
{
    // private Text dayText; // 이전 (일반 Text)
    private TextMeshProUGUI dayText; // 2. TMP 전용 컴포넌트로 변경

    void Start()
    {
        // 씬 로드 시마다 컴포넌트 참조를 가져옵니다.
        dayText = GetComponent<TextMeshProUGUI>();

        // 참조가 성공하면 업데이트 실행
        if (dayText != null)
        {
            UpdateDayDisplay();
        }
        else
        {
            Debug.LogError("DayCounterUI: TextMeshProUGUI 컴포넌트를 찾을 수 없습니다. 오브젝트에 TMP Text 컴포넌트가 붙어 있는지 확인하세요.");
        }
    }

    void UpdateDayDisplay()
    {
        if (GameManager.Instance != null && dayText != null)
        {
            // Day 1, 2 등으로 표시
            dayText.text = $"Day {GameManager.Instance.gameDay}";
        }
    }

    void OnEnable()
    {
        // Start에서 참조가 성공적으로 이루어졌다면 OnEnable에서도 업데이트합니다.
        if (dayText != null)
        {
            UpdateDayDisplay();
        }
    }
}