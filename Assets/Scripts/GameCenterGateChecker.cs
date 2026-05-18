using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// 2D 맵 씬의 '게임센터 진입 버튼'에 부착
public class GameCenterGateChecker : MonoBehaviour
{
    private Button button;

    // GameManager의 allCraneScenes와 동일하게 설정합니다.
    private static readonly List<string> craneSceneNames = new List<string> { "CraneScene1", "CraneScene2", "CraneScene3" };

    void Start()
    {
        button = GetComponent<Button>();
        // GameManager가 로드될 때까지 대기
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager가 로드되지 않았습니다.");
            return;
        }

        // 씬 로드 시 버튼 상태를 즉시 업데이트
        CheckGateAccess();
    }

    // 다른 씬에서 복귀할 때 버튼 상태를 확인하기 위해 OnEnable에서 다시 체크
    void OnEnable()
    {
        if (button != null && GameManager.Instance != null)
        {
            CheckGateAccess();
        }
    }


    public void CheckGateAccess()
    {
        // GameManager의 AreAllMachinesClearedToday 함수를 호출하여 확인
        if (GameManager.Instance.AreAllMachinesClearedToday())
        {
            // 모든 기계가 클리어되었다면 (하루치 할당량 끝)
            button.interactable = false;
            Debug.Log("오늘 크레인 게임 할당량 끝! 숙소에서 다음 날로 넘겨야 합니다.");

            // TODO: (옵션) 버튼 색상 변경 등 시각적 피드백 추가
        }
        else
        {
            // 아직 클리어할 기계가 남아있다면
            button.interactable = true;
        }
    }
}