using UnityEngine;
using UnityEngine.EventSystems; // 마우스 이벤트 사용
using TMPro; // 툴팁 텍스트용

public class BuffTooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 외부에서 할당될 버프 설명
    [HideInInspector] public string descriptionText = "버프 설명 준비 중...";

    // 툴팁 텍스트 UI 컴포넌트 (씬에 하나만 존재한다고 가정)
    private TextMeshProUGUI tooltipText;
    private GameObject tooltipPanel;

    // 새로 추가: BuffUI가 즉시 호출하여 툴팁 요소를 주입하는 함수
    public void InitTooltip(GameObject panel, TextMeshProUGUI text)
    {
        tooltipPanel = panel;
        tooltipText = text;

        // 초기 상태는 비활성화
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
        Debug.Log("Tooltip Handler initialized successfully.");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPanel != null && tooltipText != null)
        {
            // 임시 디버그: 이 로그가 두 번째 아이콘에서 뜨는지 확인
            Debug.Log("Pointer Entered Icon: " + gameObject.name);
            tooltipText.text = descriptionText;
            tooltipPanel.SetActive(true);

            // 옵션: 툴팁 위치를 커서 근처로 조정하는 로직 추가 가능
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
}