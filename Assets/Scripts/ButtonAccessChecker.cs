using UnityEngine;
using UnityEngine.UI;

public class ButtonAccessChecker : MonoBehaviour
{
    // 인스펙터에 이동할 씬 이름을 입력
    public string targetSceneName;
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        // GameManager가 로드될 때까지 대기
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager가 씬에 없습니다. 로드 순서를 확인하세요!");
            return;
        }

        CheckAccess();
    }

    // 씬 접근 가능 여부를 확인하고 버튼 상태를 업데이트
    public void CheckAccess()
    {
        if (!GameManager.Instance.IsMachineAccessible(targetSceneName))
        {
            // 오늘 클리어했다면 버튼 비활성화
            button.interactable = false;
            // 버튼 색상 변경 (옵션)
            // ColorBlock colors = button.colors;
            // colors.disabledColor = Color.gray;
            // button.colors = colors;
        }
        else
        {
            button.interactable = true;
        }
    }
}