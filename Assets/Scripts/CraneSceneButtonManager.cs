using UnityEngine;
using UnityEngine.UI;

// 3D 씬 내에서 다른 크레인 씬으로 이동하는 버튼에 부착
public class CraneSceneButtonManager : MonoBehaviour
{
    // 인스펙터에서 버튼이 이동할 목표 씬 이름을 설정해야 합니다! 
    [SerializeField]
    private string targetSceneName;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("이 스크립트는 Button 컴포넌트에 부착되어야 합니다.");
            enabled = false;
            return;
        }
    }

    // 씬이 활성화되거나 다시 돌아왔을 때 버튼 상태를 확인합니다.
    void OnEnable()
    {
        UpdateAccessibility();
    }

    // GameManager를 통해 해당 씬으로의 이동이 가능한지 확인하고 버튼 상태를 업데이트합니다.
    public void UpdateAccessibility()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager 인스턴스를 찾을 수 없습니다.");
            button.interactable = true; // 안전을 위해 기본 활성화
            return;
        }

        // 1. GameManager에게 목표 씬이 접근 가능한지 (클리어되지 않았는지) 묻습니다.
        // IsMachineAccessible이 True면 (클리어되지 않음) 버튼 활성화
        // IsMachineAccessible이 False면 (클리어됨) 버튼 비활성화
        bool isAccessible = GameManager.Instance.IsMachineAccessible(targetSceneName);

        button.interactable = isAccessible;

        if (!isAccessible)
        {
            Debug.Log($"[UI] 씬 '{targetSceneName}'은(는) 오늘 클리어되어 버튼이 비활성화됩니다.");
        }
    }
}